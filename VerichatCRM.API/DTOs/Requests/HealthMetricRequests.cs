using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Requests
{
    /// <summary>Body of POST /api/healthmetrics — a reading captured from a paired IoT device.</summary>
    public class LogHealthMetricRequest
    {
        [Required]
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("metricType")]
        public string MetricType { get; set; } = string.Empty; // bloodPressure | heartRate | bloodGlucose | temperature | bodyWeight

        [Required]
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty; // e.g. "75.5" or "118/76"

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty; // e.g. "kg", "bpm", "mmHg"

        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }
    }
}
