namespace VerichatCRM.API.DTOs.Responses
{
    public class DeliveryStatusUpdateResponse
    {
        public string UpdateId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
