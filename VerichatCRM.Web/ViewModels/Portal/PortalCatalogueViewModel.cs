using VerichatCRM.Web.ViewModels.Procurement;

namespace VerichatCRM.Web.ViewModels.Portal
{
    /// <summary>
    /// The catalogue as a client sees it: prices and stock, with a quantity box
    /// against every product. There is no checkout - the quantities become a quote
    /// request that a sales representative verifies before any price is promised.
    /// </summary>
    public class PortalCatalogueViewModel
    {
        public List<ProductCard> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new() { "All" };
        public string ActiveCategory { get; set; } = "All";
        public string SearchQuery { get; set; } = string.Empty;

        /// <summary>Products with none left; a request for one is still allowed,
        /// it simply cannot be confirmed until procurement restocks.</summary>
        public int OutOfStock => Products.Count(p => p.StockQty == 0);
    }
}
