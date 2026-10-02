namespace VerichatCRM.Web.Models.Support
{
    public class TicketDto
    {
        public string TicketId { get; set; } = "";
        public string TrackingNumber { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string? EquipmentId { get; set; }
        public string? AssignedToUserId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> PhotoUrls { get; set; } = new();
        public string Priority { get; set; } = "";
        public string Status { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime SlaDeadline { get; set; }
        public bool IsSlaBreached { get; set; }
        public int EscalationLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
        public List<TicketActivityDto> Activities { get; set; } = new();
    }
}
