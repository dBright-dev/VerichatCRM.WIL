using VerichatCRM.Web.Models.Support;

namespace VerichatCRM.Web.ViewModels.Logistics;

// The dispatch board reads the same two endpoints the tracker and the asset list
// already use; what it adds is the grouping a dispatcher works from — lanes by
// status, what is late, and what is due for service — rather than four counts.
public class DispatchDashboardViewModel
{
    public List<string> Lanes { get; init; } = new();
    public Dictionary<string, List<DeliveryDto>> ByLane { get; init; } = new();
    public List<DeliveryDto> Late { get; init; } = new();
    public int DeliveredThisWeek { get; init; }
    public List<AssetDto> ServiceDue { get; init; } = new();
    public IReadOnlyDictionary<string, int> StatusCounts { get; init; } = new Dictionary<string, int>();
}
