using System.Text.Json.Serialization;

namespace TwitchDotNet;
public class User
{
    [JsonInclude, JsonPropertyName("id")]
    public string Id { get; private set; }

    [JsonInclude, JsonPropertyName("login")]
    public string Login { get; private set; }

    [JsonInclude, JsonPropertyName("display_name")]
    public string DisplayName { get; private set; }

    [JsonInclude, JsonPropertyName("type")]
    public string Type { get; private set; }

    [JsonInclude, JsonPropertyName("broadcaster_type")]
    public string BroadcasterType { get; private set; }

    [JsonInclude, JsonPropertyName("description")]
    public string Description { get; private set; }

    [JsonInclude, JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; private set; }

    [JsonInclude, JsonPropertyName("offline_image_url")]
    public string OfflineImageUrl { get; private set; }

    //[JsonInclude, JsonPropertyName("view_count")]
    //public int ViewCount { get; private set; }

    //[JsonInclude, JsonPropertyName("email")]
    //public string Email { get; private set; }

    [JsonInclude, JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; private set; }
}
