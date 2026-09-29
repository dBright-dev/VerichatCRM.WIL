using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class Asset
    {
        public string AssetId { get; set; } = Guid.NewGuid().ToString();
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;
        public string? Location { get; set; }

        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextServiceDue { get; set; }
        public string Status { get; set; } = "Active";

        public List<AssetServiceLog> ServiceLogs { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
