using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync();
        Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request);
        Task<PurchaseOrderResponse?> ReceiveAsync(string id, string v);
    }
}
