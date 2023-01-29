using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace TwitchDotNet;
internal class RateLimiter
{
    private readonly TwitchClient _twitchClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly SlidingWindowRateLimiter _limiter;
    private readonly HttpClient _httpClient = new HttpClient();

    internal RateLimiter(TwitchClient client)
    {
        _twitchClient = client;

        _httpClient.BaseAddress = new("https://api.twitch.tv/helix/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("Client-Id", _twitchClient.Settings.ClientId);

        _limiter = new(new()
        {
            PermitLimit = 800, //800
            Window = TimeSpan.FromSeconds(60), //60
            SegmentsPerWindow = 1, //10
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 30, //30
            AutoReplenishment = true,
        });

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    internal async Task<Response<T>> ExecuteAsync<T>(string route, CancellationToken cancellationToken)
    {
        using var x = await _limiter.AcquireAsync(1, cancellationToken);

        if (!x.IsAcquired)
            throw new HttpRequestException("Too many requests");

        HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, route);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _twitchClient.Settings.AccessToken.Token);

        HttpResponseMessage response = await _httpClient.SendAsync(msg, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"{response.StatusCode} - {response.ReasonPhrase}");
        }

        return (await response.Content.ReadFromJsonAsync<Response<T>>(_jsonSerializerOptions, cancellationToken))!;
    }
}
