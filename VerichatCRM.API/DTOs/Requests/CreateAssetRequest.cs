namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateAssetRequest
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
    }

}
