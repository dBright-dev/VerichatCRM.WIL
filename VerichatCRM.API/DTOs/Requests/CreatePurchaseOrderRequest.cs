namespace VerichatCRM.API.DTOs.Requests
{
    public class CreatePurchaseOrderRequest
    {
        public string SupplierName { get; set; } = string.Empty;
        public List<PurchaseOrderLineItemRequest> LineItems { get; set; } = new();
    }
}
