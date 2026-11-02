using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly AppDbContext _context;

        public PurchaseOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.LineItems)
                .Select(po => MapToResponse(po))
                .ToListAsync();
        }

        public async Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request)
        {
            var po = new PurchaseOrder
            {
                SupplierName = request.SupplierName,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                LineItems = request.LineItems.Select(li => new PurchaseOrderLineItem
                {
                    SKU = li.SKU,
                    Quantity = li.Quantity,
                    UnitCost = li.UnitCost
                }).ToList()
            };

            po.TotalCost = po.LineItems.Sum(li => li.Quantity * li.UnitCost);
            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            return MapToResponse(po);
        }

        public async Task<PurchaseOrderResponse?> ReceiveAsync(string id)
        {
            var po = await _context.PurchaseOrders.Include(p => p.LineItems).FirstOrDefaultAsync(p => p.PurchaseOrderId == id);
            if (po == null) return null;

            po.Status = "Received";

            foreach (var item in po.LineItems)
            {
                var product = await _context.Products.FindAsync(item.SKU);
                if (product != null)
                {
                    product.StockQty += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            return MapToResponse(po);
        }

        private static PurchaseOrderResponse MapToResponse(PurchaseOrder po)
        {
            return new PurchaseOrderResponse
            {
                PurchaseOrderId = po.PurchaseOrderId,
                SupplierName = po.SupplierName,
                TotalCost = po.TotalCost,
                Status = po.Status,
                CreatedAt = po.CreatedAt,
                ExpectedArrivalDate = po.ExpectedArrivalDate,
                LineItems = po.LineItems.Select(li => new PurchaseOrderLineItemResponse
                {
                    SKU = li.SKU,
                    Quantity = li.Quantity,
                    UnitCost = li.UnitCost,
                    LineTotal = li.Quantity * li.UnitCost
                }).ToList()
            };
        }
    }
}
