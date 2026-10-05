namespace VerichatCRM.Web.Models.Support
{
    public class TicketActivityDto
    {
        public string ActivityId { get; set; } = "";
        public string Action { get; set; } = "";
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public string? Notes { get; set; }
        public DateTime PerformedAt { get; set; }
    }
}
