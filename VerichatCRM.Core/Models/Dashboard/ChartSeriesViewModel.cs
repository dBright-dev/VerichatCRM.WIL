namespace VerichatCRM.Core.Models.Dashboard;

public class ChartSeriesViewModel
{
    public string Name { get; set; } = string.Empty;
    public List<ChartPointViewModel> Points { get; set; } = new();
}
