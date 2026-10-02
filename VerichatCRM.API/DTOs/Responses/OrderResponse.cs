namespace VerichatCRM.API.DTOs.Responses
{
    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public List<OrderLineItemResponse> LineItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? TrackingNumber { get; set; }
    }
}
