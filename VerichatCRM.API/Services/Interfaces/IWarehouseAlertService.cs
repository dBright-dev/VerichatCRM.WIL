using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IWarehouseAlertService
    {
        Task<IEnumerable<WarehouseAlertResponse>> GetActiveAlertsAsync();
        Task<WarehouseAlertResponse?> ResolveAlertAsync(string alertId);
        Task CheckAndCreateAlertsAsync(); // Called after stock updates
    }
}
