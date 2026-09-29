namespace VerichatCRM.API.DTOs.Responses
{
    public class TicketActivityResponse
    {
        public string ActivityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public string? Notes { get; set; }
        public DateTime PerformedAt { get; set; }
    }

}
