namespace VerichatCRM.API.DTOs.Responses
{
    public class AssetResponse
    {
        public string AssetId { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextServiceDue { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AssetServiceLogResponse> ServiceLogs { get; set; } = new();
    }
}
