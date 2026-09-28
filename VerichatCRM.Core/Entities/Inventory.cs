using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class Inventory
    {
        public string InventoryId { get; set; } = Guid.NewGuid().ToString();
        public string SKU { get; set; } = string.Empty;
        public string WarehouseLocation { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime LastCountedAt { get; set; }
        public DateTime? LastRestockedAt { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
    }
}
