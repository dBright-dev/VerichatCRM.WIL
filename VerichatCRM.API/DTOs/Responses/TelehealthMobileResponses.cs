using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    public class TelehealthAppointmentResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("doctorName")]
        public string DoctorName { get; set; } = string.Empty;

        [JsonPropertyName("specialty")]
        public string Specialty { get; set; } = string.Empty;

        [JsonPropertyName("facility")]
        public string Facility { get; set; } = string.Empty;

        [JsonPropertyName("startsAt")]
        public DateTime StartsAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";
    }

    public class LabResultMobileResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("testName")]
        public string TestName { get; set; } = string.Empty;

        [JsonPropertyName("laboratory")]
        public string Laboratory { get; set; } = string.Empty;

        [JsonPropertyName("reportedAt")]
        public DateTime ReportedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "processing";

        [JsonPropertyName("fileSizeKb")]
        public int? FileSizeKb { get; set; }

        [JsonPropertyName("isNew")]
        public bool IsNew { get; set; }

        [JsonPropertyName("pdfUrl")]
        public string? PdfUrl { get; set; }
    }

    public class LabResultsMobileResponse
    {
        [JsonPropertyName("limsConnected")]
        public bool LimsConnected { get; set; }

        [JsonPropertyName("connectedLabs")]
        public List<string> ConnectedLabs { get; set; } = new();

        [JsonPropertyName("results")]
        public List<LabResultMobileResponse> Results { get; set; } = new();
    }

    public class MobilePrescriptionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("medication")]
        public string Medication { get; set; } = string.Empty;

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = string.Empty;

        [JsonPropertyName("prescriber")]
        public string Prescriber { get; set; } = string.Empty;

        [JsonPropertyName("issuedAt")]
        public DateTime IssuedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "active";

        [JsonPropertyName("qrPayload")]
        public string QrPayload { get; set; } = string.Empty;
    }
}
