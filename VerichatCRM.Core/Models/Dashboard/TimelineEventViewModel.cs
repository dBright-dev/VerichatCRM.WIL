namespace VerichatCRM.Core.Models.Dashboard;

public class TimelineEventViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Kind { get; set; } = "info"; // order | ticket | client | delivery | stock
    public string BadgeVariant { get; set; } = "accent";
}