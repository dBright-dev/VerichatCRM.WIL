using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class Product
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // PPE, Laboratory, Hospital Infrastructure, Home Care, Infant Care
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public int LowStockThreshold { get; set; } = 10; // For automated low-stock trigger
        /// <summary>
        /// Absolute URL of the product photograph uploaded by an administrator.
        /// Null when no image has been supplied; clients fall back to an icon.
        /// </summary>
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
