using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>Dashboard payload for the mobile home screen.</summary>
    public class ClientDashboardResponse
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("activeCount")]
        public int ActiveCount { get; set; }

        [JsonPropertyName("pendingCount")]
        public int PendingCount { get; set; }

        [JsonPropertyName("recurringCount")]
        public int RecurringCount { get; set; }

        [JsonPropertyName("vitals")]
        public ClientVitalsDto? Vitals { get; set; }

        [JsonPropertyName("nextRecurringOrder")]
        public ClientRecurringOrderDto? NextRecurringOrder { get; set; }

        [JsonPropertyName("trackingOrders")]
        public List<ClientTrackingOrderDto> TrackingOrders { get; set; } = new();
    }

    public class ClientVitalsDto
    {
        [JsonPropertyName("heartRate")]
        public int? HeartRate { get; set; }

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("systolic")]
        public int? Systolic { get; set; }

        [JsonPropertyName("diastolic")]
        public int? Diastolic { get; set; }
    }

    public class ClientRecurringOrderDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }
    }

    public class ClientTrackingOrderDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
