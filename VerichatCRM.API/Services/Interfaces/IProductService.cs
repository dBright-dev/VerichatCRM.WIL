using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetProductsAsync(string? category, bool? lowStockOnly);
        Task<ProductResponse?> GetProductBySkuAsync(string sku);
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
        Task<ProductResponse?> UpdateStockAsync(string sku, UpdateStockRequest request);
    }
}
