using VerichatCRM.Web.Models.Public;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IPublicCatalogApiService
    {
        /// <summary>
        /// Catalogue rows the public gallery may show. An empty list rather than an
        /// exception when the API is unreachable: the gallery is a page an anonymous
        /// visitor reads, and a database having a bad morning is not a reason to hand
        /// them a stack trace.
        /// </summary>
        Task<List<PublicProductItem>> GetProductsAsync();
    }
}
