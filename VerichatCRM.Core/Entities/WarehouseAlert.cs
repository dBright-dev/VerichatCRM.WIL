using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class WarehouseAlert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string Severity { get; set; } = "Warning";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
    }
}
