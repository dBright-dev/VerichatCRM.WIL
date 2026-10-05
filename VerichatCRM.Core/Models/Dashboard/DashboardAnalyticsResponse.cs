namespace VerichatCRM.Core.Models.Dashboard;

public class DashboardAnalyticsResponse
{
    public string RoleLabel { get; set; } = string.Empty;
    public List<KpiCardViewModel> Kpis { get; set; } = new();
    public List<ChartPointViewModel> RevenueTrend { get; set; } = new();
    public List<ActivityItemViewModel> RecentActivity { get; set; } = new();
}
