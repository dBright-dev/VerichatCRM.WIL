using VerichatCRM.Web.Models.Procurement;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class ProductApiService : IProductApiService
    {
        private readonly IApiClient _api;
        private readonly ILogger<ProductApiService> _logger;

        public ProductApiService(IApiClient api, ILogger<ProductApiService> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<List<ProductViewModel>> GetAllAsync()
        {
            try
            {
                var dtos = await _api.GetAsync<List<ProductViewModel>>("/api/product");
                return dtos ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load products from API — returning empty list.");
                return new();
            }
        }
    }
}
