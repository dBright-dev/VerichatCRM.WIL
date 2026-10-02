using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    public class TicketActivity
    {
        [Key]
        public string ActivityId { get; set; } = Guid.NewGuid().ToString();
        public string TicketId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;   // Created, Assigned, StatusChanged, Escalated, Commented
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public string? Notes { get; set; }
        public string PerformedByUserId { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    }
}