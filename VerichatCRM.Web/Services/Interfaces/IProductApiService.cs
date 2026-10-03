using VerichatCRM.Web.Models.Procurement;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IProductApiService
    {
        Task<List<ProductViewModel>> GetAllAsync();
    }
}
