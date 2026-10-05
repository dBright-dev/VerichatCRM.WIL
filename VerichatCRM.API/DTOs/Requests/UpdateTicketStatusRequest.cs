namespace VerichatCRM.API.DTOs.Requests
{
    public class UpdateTicketStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}
