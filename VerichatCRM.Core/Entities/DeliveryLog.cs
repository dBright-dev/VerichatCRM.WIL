using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    public class DeliveryLog
    {
        [Key]
        public string DeliveryId { get; set; } = Guid.NewGuid().ToString();
        public string TrackingNumber { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public string? Carrier { get; set; }

        public string Status { get; set; } = "Processing";
        public DateTime EstimatedArrival { get; set; }
        public DateTime? ActualArrival { get; set; }

        public List<DeliveryStatusUpdate> StatusHistory { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
