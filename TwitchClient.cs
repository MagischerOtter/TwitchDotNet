using System.Text;

namespace TwitchDotNet;
public class TwitchClient
{
    public Settings Settings { get; }

    private readonly RateLimiter _limiter;
    private readonly CancellationToken _cancellationToken;

    public TwitchClient(Settings settings, CancellationToken cancellationToken = default)
    {
        Settings.Validate(settings);

        Settings = settings;

        _limiter = new RateLimiter(this);
        _cancellationToken = cancellationToken;
    }

    public async Task<Response<Stream>> GetStreamsAsync(params string[] userIds)
    {
        if (userIds.Count() < 1)
            throw new ArgumentOutOfRangeException(nameof(userIds), userIds, "Needs atleast 1 userId");

        await Settings.AccessToken.ValidateAsync(this, _cancellationToken);

        StringBuilder sb = new StringBuilder();
        sb.Append("streams?");

        foreach (string id in userIds)
        {
            sb.Append("user_id=").Append(id).Append('&');
        }

        return await _limiter.ExecuteAsync<Stream>(sb.ToString(), _cancellationToken);
    }

    public async Task<Response<User>> GetUsersAsync(params string[] loginNames)
    {
        if (loginNames.Count() < 1)
            throw new ArgumentOutOfRangeException(nameof(loginNames), loginNames, "Needs atleast 1 loginName");

        await Settings.AccessToken.ValidateAsync(this, _cancellationToken);

        StringBuilder sb = new StringBuilder();
        sb.Append("users?");

        foreach (string id in loginNames)
        {
            sb.Append("login=").Append(id).Append('&');
        }

        return await _limiter.ExecuteAsync<User>(sb.ToString(), _cancellationToken);
    }
}
