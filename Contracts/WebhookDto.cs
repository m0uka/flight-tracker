using System.Text.Json.Serialization;

namespace FlightTracker.Contracts;

public class WebhookDto
{
    public string Content { get; set; }
    public string Username { get; set; }
    
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
}