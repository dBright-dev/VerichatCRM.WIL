namespace VerichatCRM.API.DTOs.Responses
{
    public class PurchaseOrderResponse
    {
        public string PurchaseOrderId { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public List<PurchaseOrderLineItemResponse> LineItems { get; set; } = new();
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedArrivalDate { get; set; }
    }
}
