namespace VerichatCRM.Web.Models.Support
{
    public class DeliveryDto
    {
        public string DeliveryId { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public string? Carrier { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EstimatedArrival { get; set; }
        public DateTime? ActualArrival { get; set; }
        public List<DeliveryStatusUpdateDto> StatusHistory { get; set; } = new();
    }
}
