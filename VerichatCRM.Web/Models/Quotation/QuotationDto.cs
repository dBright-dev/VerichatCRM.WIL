namespace VerichatCRM.Web.Models.Quotation
{
    public class QuotationDto
    {
        public string QuotationId { get; set; } = "";
        public string QuoteReference { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public DateTime DateGenerated { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = "";
        public string Terms { get; set; } = "";
        public List<QuotationLineItemDto> LineItems { get; set; } = new();
    }
}
