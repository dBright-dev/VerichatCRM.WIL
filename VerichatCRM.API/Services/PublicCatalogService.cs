using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// The catalogue as an anonymous visitor sees it.
    /// <para>
    /// Separate from the staff catalogue service rather than a filter on top of it,
    /// because that service hands back prices and stock levels and the projection has to
    /// be provable by reading one short method. The query selects the four columns and no
    /// others, so a change to the entity cannot widen what the public page leaks.
    /// </para>
    /// </summary>
    public class PublicCatalogService : IPublicCatalogService
    {
        private readonly AppDbContext _context;

        public PublicCatalogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PublicProductResponse>> GetProductsAsync(string? category)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
                query = query.Where(p => p.Category == category);

            // Alphabetical by name, then category: a browsed catalogue is read like a
            // list, and the staff screen - which sorts by SKU for re-ordering - is not
            // this page's concern.
            var items = await query
                .OrderBy(p => p.ProductName)
                .ThenBy(p => p.Category)
                .Select(p => new PublicProductResponse
                {
                    Sku = p.SKU,
                    Name = p.ProductName,
                    Category = p.Category,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return items;
        }
    }
}
