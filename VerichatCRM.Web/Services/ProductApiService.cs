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

        public async Task<ProductViewModel?> CreateAsync(ProductViewModel product)
        {
            try
            {
                // The API assigns the SKU, so it is not sent on create.
                var payload = new
                {
                    product.ProductName,
                    product.Category,
                    product.Price,
                    InitialStockQty = product.StockQty,
                    product.LowStockThreshold,
                    product.ImageUrl,
                };

                return await _api.PostAsync<object, ProductViewModel>("/api/product", payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product {Name}.", product.ProductName);
                return null;
            }
        }

        public async Task<ProductViewModel?> UpdateAsync(string sku, ProductViewModel product)
        {
            try
            {
                var payload = new
                {
                    product.ProductName,
                    product.Category,
                    product.Price,
                    product.LowStockThreshold,
                    product.ImageUrl,
                };

                return await _api.PutAsync<object, ProductViewModel>($"/api/product/{sku}", payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product {Sku}.", sku);
                return null;
            }
        }
    }
}
