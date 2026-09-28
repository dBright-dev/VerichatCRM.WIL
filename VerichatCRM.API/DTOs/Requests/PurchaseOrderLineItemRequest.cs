namespace VerichatCRM.API.DTOs.Requests
{
    public class PurchaseOrderLineItemRequest
    {
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}