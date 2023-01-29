namespace TwitchDotNet;
public class Settings
{
    public Settings() 
    {
        AccessToken = new AccessToken();
    }

    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public AccessToken AccessToken { get; }

    internal static void Validate(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        if(string.IsNullOrWhiteSpace(settings.ClientId))
            throw new ArgumentNullException(nameof(ClientId));

        if(string.IsNullOrWhiteSpace(settings.ClientSecret))
            throw new ArgumentNullException(nameof(ClientSecret));
    }
}
