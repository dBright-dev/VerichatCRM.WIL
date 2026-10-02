namespace VerichatCRM.API.DTOs.Requests
{
    public class UpdateDeliveryStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Notes { get; set; }
    }
}
