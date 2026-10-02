using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace VerichatCRM.API.Services
{
    public class WarehouseAlertService : IWarehouseAlertService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WarehouseAlertService> _logger;

        public WarehouseAlertService(AppDbContext context, ILogger<WarehouseAlertService> logger)
        {
            _context = context;
            _logger = logger;
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
                    CreatedAt = a.CreatedAt,
                    IsResolved = a.IsResolved
                })
                .ToListAsync();
        }

        public async Task<WarehouseAlertResponse?> ResolveAlertAsync(string alertId)
        {
            var alert = await _context.WarehouseAlerts.FindAsync(alertId);
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
                CreatedAt = alert.CreatedAt,
                IsResolved = alert.IsResolved
            };
        }

        public async Task CheckAndCreateAlertsAsync()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQty <= p.LowStockThreshold)
                .ToListAsync();

            foreach (var product in lowStockProducts)
            {
                var existingAlert = await _context.WarehouseAlerts
                    .AnyAsync(a => a.SKU == product.SKU && !a.IsResolved);

                if (!existingAlert)
                {
                    var alert = new WarehouseAlert
                    {
                        SKU = product.SKU,
                        ProductName = product.ProductName,
                        CurrentStock = product.StockQty,
                        Threshold = product.LowStockThreshold,
                        Severity = product.StockQty == 0 ? "Critical" : "Warning"
                    };

                    _context.WarehouseAlerts.Add(alert);
                    _logger.LogWarning("Low stock alert created for {SKU}: {StockQty} remaining", product.SKU, product.StockQty);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
