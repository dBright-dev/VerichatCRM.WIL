using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IPublicCatalogService
    {
        /// <summary>
        /// Catalogue rows with the four fields a page without a session may carry.
        /// <paramref name="category"/> narrows the list; null or "All" is everything.
        /// </summary>
        Task<IEnumerable<PublicProductResponse>> GetProductsAsync(string? category);
    }
}
