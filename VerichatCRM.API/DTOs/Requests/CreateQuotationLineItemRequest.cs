namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateQuotationLineItemRequest
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}