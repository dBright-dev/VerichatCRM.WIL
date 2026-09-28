using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    internal class Order
    {
<<<<<<< Updated upstream
=======
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string ClientId { get; set; } = string.Empty;
        public string? DealID { get; set; }
        public List<OrderLineItem> LineItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public class OrderLineItem
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
>>>>>>> Stashed changes
    }
}
