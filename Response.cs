using System.Text.Json.Serialization;

namespace TwitchDotNet;
public class Response<T>
{
    [JsonInclude]
    public List<T> Data { get; private set; }
}
