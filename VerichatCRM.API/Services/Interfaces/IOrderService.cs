using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetOrdersAsync();
        Task<IEnumerable<OrderResponse>> GetOrdersByClientAsync(string clientId);
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse?> UpdateOrderStatusAsync(string orderId, string newStatus);
    }
}
