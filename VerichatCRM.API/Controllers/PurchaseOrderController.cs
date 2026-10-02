using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.API.DTOs.Requests;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "System Administrator,Procurement Manager")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders()
        {
            var orders = await _purchaseOrderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request)
        {
            var order = await _purchaseOrderService.CreateAsync(request);
            return CreatedAtAction(nameof(GetPurchaseOrders), new { id = order.PurchaseOrderId }, order);
        }

        [HttpPatch("{id}/receive")]
        public async Task<IActionResult> ReceivePurchaseOrder(string id)
        {
            var result = await _purchaseOrderService.ReceiveAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
