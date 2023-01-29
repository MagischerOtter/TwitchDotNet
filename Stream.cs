using System.Text.Json.Serialization;

namespace TwitchDotNet;
public class Stream
{
    [JsonInclude, JsonPropertyName("id")]
    public string Id { get; private set; }

    [JsonInclude, JsonPropertyName("user_id")]
    public string UserId { get; private set; }

    [JsonInclude, JsonPropertyName("user_login")]
    public string UserLogin { get; private set; }

    [JsonInclude, JsonPropertyName("user_name")]
    public string UserName { get; private set; }

    [JsonInclude, JsonPropertyName("game_id")]
    public string GameId { get; private set; }

    [JsonInclude, JsonPropertyName("game_name")]
    public string GameName { get; private set; }

    [JsonInclude, JsonPropertyName("type")]
    public string Type { get; private set; }

    [JsonInclude, JsonPropertyName("title")]
    public string Title { get; private set; }

    [JsonInclude, JsonPropertyName("tags")]
    public string[] Tags { get; private set; }

    [JsonInclude, JsonPropertyName("viewer_count")]
    public int ViewerCount { get; private set; }

    [JsonInclude, JsonPropertyName("started_at")]
    public DateTime StartedAt { get; private set; }

    [JsonInclude, JsonPropertyName("language")]
    public string Language { get; private set; }

    [JsonInclude, JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; private set; }

    [JsonInclude, JsonPropertyName("tag_ids")]
    public string[] TagIds { get; private set; }

    [JsonInclude, JsonPropertyName("is_mature")]
    public bool IsMature { get; private set; }
}
