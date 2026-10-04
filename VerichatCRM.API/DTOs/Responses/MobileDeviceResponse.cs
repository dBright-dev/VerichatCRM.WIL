using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    public class MobileDeviceResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "other";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "connected";

        [JsonPropertyName("lastSyncAt")]
        public DateTime? LastSyncAt { get; set; }
    }
}
