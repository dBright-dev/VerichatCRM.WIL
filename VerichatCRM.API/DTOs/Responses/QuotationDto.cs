namespace VerichatCRM.API.DTOs.Responses
{
    public class QuotationDto
    {
        public string QuotationId { get; set; } = string.Empty;
        public string QuoteReference { get; set; } = string.Empty;
        public string DealId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime DateGenerated { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Terms { get; set; } = string.Empty;
        public List<QuotationLineItemDto> LineItems { get; set; } = new();
    }
}
