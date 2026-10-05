using VerichatCRM.Web.Models.Procurement;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IProductApiService
    {
        Task<List<ProductViewModel>> GetAllAsync();
        Task<ProductViewModel?> CreateAsync(ProductViewModel product);
        Task<ProductViewModel?> UpdateAsync(string sku, ProductViewModel product);
    }
}
