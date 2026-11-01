namespace VerichatCRM.Web.ViewModels.Support
{
    public class SupportDashboardViewModel
    {
        public int OpenTickets { get; set; }
        public int Escalated { get; set; }
        public int SlaBreaches { get; set; }
        // Was AssetsUnderMaintence; nothing read it until this board did, so the
        // property is now spelled the way the rest of the model is.
        public int AssetsUnderMaintenance { get; set; }
        public int ActiveShipments { get; set; }
        public List<RecentTicketItem> RecentTickets { get; set; } = new();
    }
}
