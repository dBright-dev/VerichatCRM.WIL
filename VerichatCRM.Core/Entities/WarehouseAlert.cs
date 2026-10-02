using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class WarehouseAlert
    {
        [Key]
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string Severity { get; set; } = "Warning"; // Warning, Critical
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public string? ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
