namespace VerichatCRM.Core.Entities
{
    public class PurchaseOrderLineItem
    {
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}