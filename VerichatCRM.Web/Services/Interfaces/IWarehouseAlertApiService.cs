using VerichatCRM.Web.Models.Procurement;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IWarehouseAlertApiService
    {
        Task<List<WarehouseAlertViewModel>> GetActiveAsync();
    }
}
