namespace VerichatCRM.Web.ViewModels.Support
{
    public class SupportDashboardViewModel
    {
        public int OpenTickets { get; set; }
        public int Escalated { get; set; }
        public int SlaBreaches { get; set; }
        public int AssetsUnderMaintence { get; set; }
        public int ActiveShipments { get; set; }
        public List<RecentTicketItem> RecentTickets { get; set; } = new();
    }
}
