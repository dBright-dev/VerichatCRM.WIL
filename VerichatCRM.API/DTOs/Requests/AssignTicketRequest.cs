namespace VerichatCRM.API.DTOs.Requests
{
    public class AssignTicketRequest
    {
        public string AssignedToUserId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
