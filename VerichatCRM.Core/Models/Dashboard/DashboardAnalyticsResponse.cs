namespace VerichatCRM.Core.Models.Dashboard;

public class DashboardAnalyticsResponse
{
    public string RoleLabel { get; set; } = string.Empty;
    public List<KpiCardViewModel> Kpis { get; set; } = new();
    public List<ChartPointViewModel> RevenueTrend { get; set; } = new();

    /// <summary>Extra graph series accumulated from the DB (tickets, stock, deliveries...).</summary>
    public List<ChartSeriesViewModel> Series { get; set; } = new();

    /// <summary>Donut / breakdown (e.g. order status share).</summary>
    public List<ChartPointViewModel> Breakdown { get; set; } = new();

    public List<ActivityItemViewModel> RecentActivity { get; set; } = new();

    /// <summary>Chronological timeline across orders / tickets / clients / deliveries.</summary>
    public List<TimelineEventViewModel> Timeline { get; set; } = new();
}
