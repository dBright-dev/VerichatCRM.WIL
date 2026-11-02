using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>Response for a persisted health metric reading (POST /api/healthmetrics).</summary>
    public class LogHealthMetricResponse
    {
        [JsonPropertyName("metricId")]
        public string MetricId { get; set; } = string.Empty;

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("metricType")]
        public string MetricType { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
