namespace VerichatCRM.API.DTOs.Responses
{
    public class TicketResponse
    {
        public string TicketId { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? EquipmentId { get; set; }
        public string? AssignedToUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PhotoUrls { get; set; } = new();
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime SlaDeadline { get; set; }
        public bool IsSlaBreached { get; set; }
        public int EscalationLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
        public List<TicketActivityResponse> Activities { get; set; } = new();
    }
}
