namespace VerichatCRM.API.DTOs.Responses
{
    public class QuotationLineItemDto
    {
        public string LineItemId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}