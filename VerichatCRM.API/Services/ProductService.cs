using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IWarehouseAlertService _alertService;

        public ProductService(AppDbContext context, IWarehouseAlertService alertService)
        {
            _context = context;
            _alertService = alertService;
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsAsync(string? category, bool? lowStockOnly)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            if (lowStockOnly == true)
                query = query.Where(p => p.StockQty <= p.LowStockThreshold);

            return await query.Select(p => new ProductResponse
            {
                SKU = p.SKU,
                ProductName = p.ProductName,
                Category = p.Category,
                Price = p.Price,
                StockQty = p.StockQty,
                IsLowStock = p.StockQty <= p.LowStockThreshold
            }).ToListAsync();
        }

        public async Task<ProductResponse?> GetProductBySkuAsync(string sku)
        {
            var p = await _context.Products.FindAsync(sku);
            if (p == null) return null;

            return new ProductResponse
            {
                SKU = p.SKU,
                ProductName = p.ProductName,
                Category = p.Category,
                Price = p.Price,
                StockQty = p.StockQty,
                IsLowStock = p.StockQty <= p.LowStockThreshold
            };
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                SKU = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // Or however you generate SKU
                ProductName = request.ProductName,
                Category = request.Category,
                Price = request.Price,
                StockQty = request.InitialStockQty,
                LowStockThreshold = request.LowStockThreshold
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _alertService.CheckAndCreateAlertsAsync();

            return new ProductResponse
            {
                SKU = product.SKU,
                ProductName = product.ProductName,
                Category = product.Category,
                Price = product.Price,
                StockQty = product.StockQty,
                IsLowStock = product.StockQty <= product.LowStockThreshold
            };
        }

        public async Task<ProductResponse?> UpdateStockAsync(string sku, UpdateStockRequest request)
        {
            var product = await _context.Products.FindAsync(sku);
            if (product == null) return null;

            product.StockQty = request.NewQuantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _alertService.CheckAndCreateAlertsAsync();

            return new ProductResponse
            {
                SKU = product.SKU,
                ProductName = product.ProductName,
                Category = product.Category,
                Price = product.Price,
                StockQty = product.StockQty,
                IsLowStock = product.StockQty <= product.LowStockThreshold
            };
        }
    }
}
