using VerichatCRM.Web.Models.Procurement;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class WarehouseAlertApiService : IWarehouseAlertApiService
    {
        private readonly IApiClient _api;
        private readonly ILogger<WarehouseAlertApiService> _logger;

        public WarehouseAlertApiService(IApiClient api, ILogger<WarehouseAlertApiService> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<List<WarehouseAlertViewModel>> GetActiveAsync()
        {
            try
            {
                var dtos = await _api.GetAsync<List<WarehouseAlertViewModel>>("/api/warehousealert");
                return dtos ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load alerts — returning empty list.");
                return new();
            }
        }
    }
}
