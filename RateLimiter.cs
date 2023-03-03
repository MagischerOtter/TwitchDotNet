using Microsoft.Extensions.Logging;
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
            ReplenishmentPeriod = TimeSpan.FromSeconds(6),
            QueueLimit = 30,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    internal Task<Response<T>> ExecuteGetAsync<T>(string route, CancellationToken cancellationToken)
    {
        try
        {
            return ExecuteAsync<T>(HttpMethod.Get, route, cancellationToken);
        }
        catch (Exception e)
        {
            _twitchClient.Logger?.LogError(e, "An error has occurred while calling Twitch API");
            throw;
        }
    }

    private async Task<Response<T>> ExecuteAsync<T>(HttpMethod httpMethod, string route, CancellationToken cancellationToken)
    {
        HttpRequestMessage msg = new HttpRequestMessage(httpMethod, route);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _twitchClient.Settings.AccessToken.Token);

        HttpResponseMessage response = await ExecuteAsync(msg, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<Response<T>>(_jsonSerializerOptions, cancellationToken))!;
        }

        if (response.StatusCode is HttpStatusCode.TooManyRequests)
        {
            byte retries = 1;
            TimeSpan delay;

            do
            {
                delay = response.Headers.RetryAfter!.Delta!.Value;

                _twitchClient.Logger?.LogWarning("Hit ratelimit, trying again in {0} sec", delay.TotalSeconds);

                await Task.Delay(delay);

                response = await ExecuteAsync(msg, cancellationToken);

            } while (!response.IsSuccessStatusCode && retries < 3);

            if (response.IsSuccessStatusCode)
            {
                _twitchClient.Logger?.LogInformation("Successful request after {0} attempts", retries);
                return (await response.Content.ReadFromJsonAsync<Response<T>>(_jsonSerializerOptions, cancellationToken))!;
            }

            throw new Exception("failed more than 2 times");
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
        if (x.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = new(retryAfter);
        }

        return response;
    }
}