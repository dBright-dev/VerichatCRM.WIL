namespace VerichatCRM.API.DTOs.Responses
{
    public class PurchaseOrderLineItemResponse
    {
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}