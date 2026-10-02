using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VerichatCRM.Core.Enums;

namespace VerichatCRM.Core.Entities
{
    public class Ticket
    {
        [Key]
        public string TicketId { get; set; } = Guid.NewGuid().ToString();
        public string TrackingNumber { get; set; } = string.Empty; // e.g. VC-TCK-10432

        // Links
        public string ClientId { get; set; } = string.Empty;
        public string? EquipmentId { get; set; }        // references Asset
        public string? AssignedToUserId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        // Content
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PhotoUrls { get; set; } = new();

        // Classification
        public string Priority { get; set; } = TicketPriorities.Medium;
        public string Status { get; set; } = TicketStatuses.Received;
        public string Category { get; set; } = "General"; // Technical, Logistics, Billing, General

        // SLA
        public DateTime SlaDeadline { get; set; }
        public bool IsSlaBreached { get; set; }
        public int EscalationLevel { get; set; } = 0;
        public DateTime? EscalatedAt { get; set; }

        // Lifecycle
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }

        // Navigation
        public Asset? Equipment { get; set; }
        public List<TicketActivity> Activities { get; set; } = new();
    }
}
