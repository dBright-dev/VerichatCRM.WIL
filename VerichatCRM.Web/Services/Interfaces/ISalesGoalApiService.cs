using VerichatCRM.Web.Models.Goals;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface ISalesGoalApiService
    {
        // Never null: returns an empty (no goal) model if the API is unavailable.
        Task<GoalProgressViewModel> GetDashboardProgressAsync();
    }
}
