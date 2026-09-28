using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class WarehouseAlertService : IWarehouseAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WarehouseAlertService> _logger;

        public WarehouseAlertService(ApplicationDbContext context, ILogger<WarehouseAlertService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CheckAndCreateAlertAsync()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQty < p.LowStockThreshold)
                .ToListAsync();

            foreach (var product in lowStockProducts)
            {
                var hasExistingAlert = await _context.WarehouseAlerts
                    .AnyAsync(a => a.SKU == product.SKU && !a.IsResolved);
                if (!hasExistingAlert)
                {
                    var newAlert = new WarehouseAlert
                    {
                        SKU = product.SKU,
                        ProductName = product.ProductName,
                        CurrentStock = product.StockQty,
                        Threshold = product.LowStockThreshold,
                        Severity = product.StockQty == 0 ? "Critical" : "Warning",
                        CreatedAt = DateTime.UtcNow,
                        IsResolved = false
                    };
                    _context.WarehouseAlerts.Add(newAlert);
                    _logger.LogInformation($"Created new warehouse alert for product {product.ProductName} (SKU: {product.SKU}).");
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WarehouseAlertResponse>> GetActiveAlertsAsync()
        {
            return await _context.WarehouseAlerts
           .Where(a => !a.IsResolved)
           .OrderByDescending(a => a.CreatedAt)
           .Select(a => new WarehouseAlertResponse
           {
               AlertId = a.AlertId,
               SKU = a.SKU,
               ProductName = a.ProductName,
               CurrentStock = a.CurrentStock,
               Threshold = a.Threshold,
               Severity = a.Severity,
               AlertDate = a.CreatedAt,
               isResolved = a.IsResolved
           })
           .ToListAsync();
        }

        public async Task<WarehouseAlertResponse?> ResolveAlertAsync(string alertId)
        {
            var alert = await _context.WarehouseAlerts.FirstOrDefaultAsync(a => a.AlertId == alertId);
            if (alert == null) return null;

            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new WarehouseAlertResponse
            {
                AlertId = alert.AlertId,
                SKU = alert.SKU,
                ProductName = alert.ProductName,
                CurrentStock = alert.CurrentStock,
                Threshold = alert.Threshold,
                Severity = alert.Severity,
                AlertDate = alert.CreatedAt,
                isResolved = alert.IsResolved
            };
        }
    }
}
