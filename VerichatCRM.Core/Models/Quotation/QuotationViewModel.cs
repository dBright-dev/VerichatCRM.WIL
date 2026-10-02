using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Quotation
{
    public class QuotationViewModel
    {
        public string QuotationId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientSector { get; set; } = string.Empty;
        public string QuoteReference { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; } = DateTime.Today.AddDays(30);
        public string Terms { get; set; } = "50% deposit on approval, balance on delivery.";
        public decimal VatRate { get; set; } = 0.15m;
        public List<QuotationLineItemViewModel> LineItems { get; set; } = new();
        public decimal Subtotal => LineItems.Sum(i => i.LineTotal);
        public decimal VatAmount => Subtotal * VatRate;
        public decimal GrandTotal => Subtotal + VatAmount;
    }
}
