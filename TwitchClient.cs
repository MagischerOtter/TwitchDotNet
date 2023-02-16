using Microsoft.Extensions.Logging;
using System.Text;

namespace TwitchDotNet;
public class TwitchClient
{
    public Settings Settings { get; }

    private readonly RateLimiter _limiter;
    private readonly CancellationToken _cancellationToken;
    internal ILogger Logger { get; }

    public TwitchClient(Settings settings, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        Settings.Validate(settings);

        Logger = logger;

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

        return await _limiter.ExecuteGetAsync<Stream>(sb.ToString(), _cancellationToken);
    }

    public async Task<Response<User>> GetUsersAsync(List<string>? loginNames = null, List<string>? ids = null)
    {
        if (loginNames?.Count() < 1 || ids?.Count < 1)
            throw new ArgumentOutOfRangeException(nameof(loginNames), loginNames, "Needs atleast 1 entry");

        await Settings.AccessToken.ValidateAsync(this, _cancellationToken);

        StringBuilder sb = new StringBuilder();
        sb.Append("users?");

        if(loginNames is not null)
        {
            foreach (string id in loginNames)
            {
                sb.Append("login=").Append(id).Append('&');
            }
        }

        if(ids is not null)
        {
            foreach (string id in ids)
            {
                sb.Append("id=").Append(id).Append('&');
            }
        }

        return await _limiter.ExecuteGetAsync<User>(sb.ToString(), _cancellationToken);
    }
}
