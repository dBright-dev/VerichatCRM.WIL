using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.LineItems)
                .Select(o => MapToResponse(o))
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersByClientAsync(string clientId)
        {
            return await _context.Orders
                .Include(o => o.LineItems)
                .Where(o => o.ClientId == clientId)
                .Select(o => MapToResponse(o))
                .ToListAsync();
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var order = new Order
            {
                ClientId = request.ClientId,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                LineItems = new List<OrderLineItem>()
            };

            decimal total = 0;

            foreach (var item in request.LineItems)
            {
                var product = await _context.Products.FindAsync(item.SKU);
                if (product == null) continue;

                var lineItem = new OrderLineItem
                {
                    SKU = product.SKU,
                    ProductName = product.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                total += lineItem.LineTotal;
                order.LineItems.Add(lineItem);

                // Deduct stock
                product.StockQty -= item.Quantity;
            }

            order.TotalAmount = total;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return MapToResponse(order);
        }

        public async Task<OrderResponse?> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var order = await _context.Orders.Include(o => o.LineItems).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return MapToResponse(order);
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                ClientId = order.ClientId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.OrderDate,
                ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                TrackingNumber = order.TrackingNumber,
                LineItems = order.LineItems.Select(li => new OrderLineItemResponse
                {
                    SKU = li.SKU,
                    ProductName = li.ProductName,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    LineTotal = li.LineTotal
                }).ToList()
            };
        }
    }
}
