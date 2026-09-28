namespace VerichatCRM.Web.ViewModels.Procurement
{
    public class ProcurementDashboardViewModel
    {
        public string ManagerName { get; set; } = "Procurement Manager";
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public int ActiveOrders { get; set; }
        public int PendingQuotes { get; set; }
        public int CatalogItems { get; set; }
        public int LowStockItems { get; set; }
        public List<RevenuePoint> RevenueVsTarget { get; set; } = new();
        public List<ActivityItem> RecentActivity { get; set; } = new();
    }
    public class RevenuePoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Target { get; set; }
    }

    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Severity { get; set; } = "info"; // info, warning, success, critical
    }
}
