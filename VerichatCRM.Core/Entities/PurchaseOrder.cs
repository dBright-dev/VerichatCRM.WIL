using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class PurchaseOrder
    {
        [Key]
        public string PurchaseOrderId { get; set; } = Guid.NewGuid().ToString();
        public string SupplierName { get; set; } = string.Empty;
        public List<PurchaseOrderLineItem> LineItems { get; set; } = new();
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Received, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedArrivalDate { get; set; }
    }
}
