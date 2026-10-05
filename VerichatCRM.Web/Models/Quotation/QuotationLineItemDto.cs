namespace VerichatCRM.Web.Models.Quotation
{
    public class QuotationLineItemDto
    {
        public string SKU { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
