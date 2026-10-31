using VerichatCRM.Web.Models.Support;

namespace VerichatCRM.Web.ViewModels.Portal
{
    public class PortalTicketsViewModel
    {
        public List<TicketDto> Tickets { get; set; } = new();
        public string ActiveStatus { get; set; } = "All";
        public int OpenCount => Tickets.Count(t => t.Status != "Resolved" && t.Status != "Closed");
        public int BreachedCount => Tickets.Count(t => t.IsSlaBreached);
    }
}
