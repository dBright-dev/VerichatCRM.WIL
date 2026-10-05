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

            var products = await query.ToListAsync();
            return products.Select(Map).ToList();
        }

        public async Task<ProductResponse?> GetProductBySkuAsync(string sku)
        {
            var p = await _context.Products.FindAsync(sku);
            return p is null ? null : Map(p);
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                SKU = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                ProductName = request.ProductName,
                Category = request.Category,
                Price = request.Price,
                StockQty = request.InitialStockQty,
                LowStockThreshold = request.LowStockThreshold,
                ImageUrl = NormaliseImageUrl(request.ImageUrl),
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _alertService.CheckAndCreateAlertsAsync();

            return Map(product);
        }

        public async Task<ProductResponse?> UpdateStockAsync(string sku, UpdateStockRequest request)
        {
            var product = await _context.Products.FindAsync(sku);
            if (product == null) return null;

            product.StockQty = request.NewQuantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _alertService.CheckAndCreateAlertsAsync();

            return Map(product);
        }

        public async Task<ProductResponse?> UpdateProductAsync(string sku, UpdateProductRequest request)
        {
            var product = await _context.Products.FindAsync(sku);
            if (product == null) return null;

            if (!string.IsNullOrWhiteSpace(request.ProductName))
                product.ProductName = request.ProductName.Trim();
            if (!string.IsNullOrWhiteSpace(request.Category))
                product.Category = request.Category.Trim();
            if (request.Price.HasValue)
                product.Price = request.Price.Value;
            if (request.LowStockThreshold.HasValue)
                product.LowStockThreshold = request.LowStockThreshold.Value;
            if (request.ImageUrl != null)
                product.ImageUrl = NormaliseImageUrl(request.ImageUrl);

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _alertService.CheckAndCreateAlertsAsync();

            return Map(product);
        }

        /// <summary>Single mapping point so every endpoint returns the same shape.</summary>
        private static ProductResponse Map(Product p) => new()
        {
            SKU = p.SKU,
            ProductName = p.ProductName,
            Category = p.Category,
            Price = p.Price,
            StockQty = p.StockQty,
            IsLowStock = p.StockQty <= p.LowStockThreshold,
            ImageUrl = p.ImageUrl,
        };

        /// <summary>
        /// Stores blank input as null and rejects anything that is not an absolute
        /// http(s) URL, so a malformed value can never reach the database or leak
        /// into an image tag on a client.
        /// </summary>
        private static string? NormaliseImageUrl(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var trimmed = raw.Trim();
            return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ? trimmed
                : null;
        }
    }
}
