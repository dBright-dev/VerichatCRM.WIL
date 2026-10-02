using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class Quotation
    {
        [Key]
        public string QuotationId { get; set; } = Guid.NewGuid().ToString();
        public string QuoteReference { get; set; } = string.Empty;   // e.g. VC-QT-2026-0042
        public string DealId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;

        public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);
        public decimal Subtotal { get; set; }
        public decimal VatRate { get; set; } = 0.15m;               // 15% VAT (RSA)
        public decimal VatAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string Terms { get; set; } = "Payment due within 30 days.";
        public string Status { get; set; } = "Draft";               // Draft | Sent | Accepted | Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Deal? Deal { get; set; }
        public Client? Client { get; set; }
        public List<QuotationLineItem> LineItems { get; set; } = new();
    }
}
