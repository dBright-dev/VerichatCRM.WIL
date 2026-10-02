namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateQuotationRequest
    {
        public string DealId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
        public decimal VatRate { get; set; } = 0.15m;
        public string Terms { get; set; } = "Payment due within 30 days.";
        public List<CreateQuotationLineItemRequest> LineItems { get; set; } = new();
    }
}
