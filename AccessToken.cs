using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace TwitchDotNet;
public class AccessToken
{
    public string Token { get; private set; } = "";
    private bool IsExpierd => _expiresDateTime < DateTime.Now;

    private DateTime _expiresDateTime = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly HttpClient _httpClient = new();

    internal async ValueTask ValidateAsync(TwitchClient twitchClient, CancellationToken cancellationToken)
    {
        try
        {
            await _lock.WaitAsync();

            if (!IsExpierd)
                return;

            twitchClient.Logger?.LogInformation("Acquiring new access token");

            var model = await GetNewAccessTokenAsync(twitchClient, cancellationToken);

            twitchClient.Settings.AccessToken.Token = model.Token;
            twitchClient.Settings.AccessToken.SetExpierDate(model.ExpiresIn - 60);

            twitchClient.Logger?.LogInformation("Acquired new access token: {0}", string.Join(null, Enumerable.Repeat('*', Token.Length - 3)) + Token[^3..]);
        }
        catch(Exception e)
        {
            twitchClient.Logger?.LogError(e, "Something went wrong getting a new AccessToken");
            throw;
        }

        finally
        {
            _lock.Release();
        }
    }

    async Task<AccessTokenModel> GetNewAccessTokenAsync(TwitchClient twitchClient, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"https://id.twitch.tv/oauth2/token?client_id={twitchClient.Settings.ClientId}&client_secret={twitchClient.Settings.ClientSecret}&grant_type=client_credentials", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<AccessTokenModel>(cancellationToken: cancellationToken))!;
    }

    private void SetExpierDate(int inSeconds)
    {
        _expiresDateTime = DateTime.Now.AddSeconds(inSeconds);
    }

    class AccessTokenModel
    {
        [JsonPropertyName("access_token")]
        public string Token { get; set; } = "";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
