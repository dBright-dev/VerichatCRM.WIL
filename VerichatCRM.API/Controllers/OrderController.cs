using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get all orders (Admin/Sales view).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Get orders for a specific client (Procurement Manager view).
        /// </summary>
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager")]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetClientOrders(string clientId)
        {
            var orders = await _orderService.GetOrdersByClientAsync(clientId);
            return Ok(orders);
        }

        /// <summary>
        /// Create a new order.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Sales Representative,Procurement Manager")]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrders), new { id = order.OrderId }, order);
        }

        /// <summary>
        /// Update order status (e.g., Confirmed → Shipped → Delivered).
        /// </summary>
        [HttpPatch("{orderId}/status")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(
            string orderId,
            [FromBody] string newStatus)
        {
            var order = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            if (order == null) return NotFound();
            return Ok(order);
        }
    }
}
