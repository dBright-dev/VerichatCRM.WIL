using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IDeliveryLogService
    {
        Task<IEnumerable<DeliveryLogResponse>> GetAllAsync();
        Task<IEnumerable<DeliveryLogResponse>> GetByClientAsync(string clientId);
        Task<DeliveryLogResponse?> GetByTrackingAsync(string trackingNumber);
        Task<DeliveryLogResponse?> UpdateStatusAsync(string deliveryId, UpdateDeliveryStatusRequest request, string userId);
    }
}
