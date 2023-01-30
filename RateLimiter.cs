using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace TwitchDotNet;
internal class RateLimiter
{
    private readonly TwitchClient _twitchClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly TokenBucketRateLimiter _limiter;
    private readonly HttpClient _httpClient = new HttpClient();

    internal RateLimiter(TwitchClient client)
    {
        _twitchClient = client;

        _httpClient.BaseAddress = new("https://api.twitch.tv/helix/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Client-Id", _twitchClient.Settings.ClientId);

        _limiter = new(new()
        {
            TokenLimit = 800,
            TokensPerPeriod = 80,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 30,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    internal async Task<Response<T>> ExecuteGetAsync<T>(string route, CancellationToken cancellationToken)
    {
        HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, route);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _twitchClient.Settings.AccessToken.Token);

        HttpResponseMessage response = await ExecuteAsync(msg, cancellationToken);

        if(response.IsSuccessStatusCode)
        {
            //TODO: AddLogging
            return (await response.Content.ReadFromJsonAsync<Response<T>>(_jsonSerializerOptions, cancellationToken))!;
        }

        if (response.StatusCode is HttpStatusCode.TooManyRequests)
        {
            byte retries = 1;

            do
            {
                //TODO: AddLogging

                await Task.Delay(response.Headers.RetryAfter!.Delta!.Value);

                response = await ExecuteAsync(msg, cancellationToken);

            } while (!response.IsSuccessStatusCode && retries < 6);

            if(response.IsSuccessStatusCode)
            {
                //TODO: AddLogging
                return (await response.Content.ReadFromJsonAsync<Response<T>>(_jsonSerializerOptions, cancellationToken))!;
            }

            throw new Exception("failed more than 5 times");
        }

        throw new Exception($"[{response.StatusCode}] - {response.ReasonPhrase}");
    }

    private async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        using var x = await _limiter.AcquireAsync(1, cancellationToken);

        if (x.IsAcquired)
        {
            return await _httpClient.SendAsync(message, cancellationToken);
        }

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if(x.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = new(retryAfter);
        }

        return response;
    }
}
