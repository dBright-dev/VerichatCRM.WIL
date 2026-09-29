namespace VerichatCRM.Core.Entities
{
    public class DeliveryStatusUpdate
    {
        public string UpdateId { get; set; } = Guid.NewGuid().ToString();
        public string DeliveryId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public string UpdatedByUserId { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}