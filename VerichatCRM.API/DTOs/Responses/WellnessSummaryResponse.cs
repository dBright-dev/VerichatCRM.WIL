using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>Vitals summary payload for the wellness screen.</summary>
    public class WellnessSummaryResponse
    {
        [JsonPropertyName("healthScore")]
        public int? HealthScore { get; set; }

        [JsonPropertyName("metrics")]
        public List<HealthMetricResponse> Metrics { get; set; } = new();
    }

    public class HealthMetricResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("statusLabel")]
        public string StatusLabel { get; set; } = "Normal";

        [JsonPropertyName("inRange")]
        public bool InRange { get; set; } = true;
    }
}
