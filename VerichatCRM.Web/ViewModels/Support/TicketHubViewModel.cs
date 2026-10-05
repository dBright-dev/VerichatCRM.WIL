using VerichatCRM.Web.Models.Support;

namespace VerichatCRM.Web.ViewModels.Support
{
    public class TicketHubViewModel
    {
        public List<TicketDto> Tickets { get; set; } = new();
        public string ActiveStatus { get; set; } = "All";
        public List<string> StatusFilters { get; set; } = new()
    {
        "All", "Received", "Under Review", "Action Taken", "Escalated", "Resolved", "Closed"
    };

        public int TotalOpen => Tickets.Count(t => t.Status != "Resolved" && t.Status != "Closed");
        public int EscalatedCount => Tickets.Count(t => t.EscalationLevel > 0);
        public int SlaBreaches => Tickets.Count(t => t.IsSlaBreached);
        public int ResolvedToday => Tickets.Count(t =>
            t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == DateTime.UtcNow.Date);
    }
}
