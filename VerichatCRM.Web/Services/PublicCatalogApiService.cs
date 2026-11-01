using VerichatCRM.Web.Models.Public;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    /// <summary>
    /// Reads the projection the public gallery is allowed to show. The staff catalogue
    /// screen has its own service against the authenticated endpoints; this one exists
    /// so an anonymous page never asks for a route it cannot have.
    /// </summary>
    public class PublicCatalogApiService : IPublicCatalogApiService
    {
        private readonly IApiClient _api;
        private readonly ILogger<PublicCatalogApiService> _logger;

        public PublicCatalogApiService(IApiClient api, ILogger<PublicCatalogApiService> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<List<PublicProductItem>> GetProductsAsync()
        {
            try
            {
                var items = await _api.GetAsync<List<PublicProductItem>>("/api/public/products");
                return items ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Public catalogue read failed - showing an empty gallery.");
                return new();
            }
        }
    }
}
