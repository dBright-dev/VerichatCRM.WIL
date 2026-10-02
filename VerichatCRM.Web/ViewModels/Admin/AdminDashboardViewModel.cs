namespace VerichatCRM.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public string UserName { get; set; } = "System Administrator";
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public decimal TotalRevenue { get; set; }
        public decimal RevenueTarget { get; set; } = 2600000m;
        public int ActiveClients { get; set; }
        public int RenewalsDue { get; set; }
        public int OpenTickets { get; set; }
        public int CriticalTickets { get; set; }
        public int LowStockItems { get; set; }
        public List<AdminRevenuePoint> RevenueVsTarget { get; set; } = new();
        public List<AdminActivityItem> RecentActivity { get; set; } = new();
    }

    public class AdminRevenuePoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Target { get; set; }
    }

    public class AdminActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";
    }
}
