namespace VerichatCRM.Web.Models.Support
{
    public class DeliveryStatusUpdateDto
    {
        public string UpdateId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
