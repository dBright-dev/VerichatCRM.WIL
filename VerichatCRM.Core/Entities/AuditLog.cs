using System;
using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>
    /// Append-only record of a sensitive action, written by AuditLogInterceptor.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public string AuditId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Table name of the changed entity, e.g. "Tickets".</summary>
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;   // Added | Modified | Deleted
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
