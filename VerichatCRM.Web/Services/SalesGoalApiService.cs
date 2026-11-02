using VerichatCRM.Web.Models.Goals;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class SalesGoalApiService : ISalesGoalApiService
    {
        private readonly IApiClient _api;
        private readonly ILogger<SalesGoalApiService> _logger;

        public SalesGoalApiService(IApiClient api, ILogger<SalesGoalApiService> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<GoalProgressViewModel> GetDashboardProgressAsync()
        {
            try
            {
                return await _api.GetAsync<GoalProgressViewModel>("/api/salesgoals/dashboard-progress")
                       ?? new GoalProgressViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Sales goal progress unavailable.");
                return new GoalProgressViewModel();
            }
        }
    }
}