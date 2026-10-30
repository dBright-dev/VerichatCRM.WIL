using VerichatCRM.Core.Models.Dashboard;

namespace VerichatCRM.Web.Models.Analytics
{
    public class AnalyticsDashboardDto
    {
        public List<SalesTargetPoint> SalesVsTarget { get; set; } = new();
        public FunnelCounts Funnel { get; set; } = new();
        public List<SectorRevenuePoint> SectorRevenue { get; set; } = new();
    }

    public class SalesTargetPoint
    {
        public string Label { get; set; } = "";
        public decimal Sales { get; set; }
        public decimal Target { get; set; }
    }

    public class FunnelCounts
    {
        public int Inquiries { get; set; }
        public int Quotes { get; set; }
        public int Won { get; set; }
    }

    public class SectorRevenuePoint
    {
        public string Sector { get; set; } = "";
        public decimal Revenue { get; set; }
    }

    public class AnalyticsPageViewModel
    {
        public DashboardAnalyticsResponse Overview { get; set; } = new();
        public AnalyticsDashboardDto? Charts { get; set; }
        public string ChartsJson { get; set; } = "null";
        public bool IsDemo { get; set; }
    }
}