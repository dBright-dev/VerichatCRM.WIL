using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    public class QuotationLineItem
    {
        [Key]
        public string LineItemId { get; set; } = Guid.NewGuid().ToString();
        public string QuotationId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}