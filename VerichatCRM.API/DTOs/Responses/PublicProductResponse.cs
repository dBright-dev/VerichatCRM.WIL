namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>
    /// One catalogue row as the public gallery may show it.
    /// <para>
    /// This is a deliberate subset of <see cref="ProductResponse"/>, and the columns left
    /// out are the reason it exists. Price, stock on hand and the reorder threshold tell a
    /// competitor where to undercut and when to call; a page reachable without signing in
    /// has no business carrying them. Nothing is projected here by accident: a new field
    /// on <c>Product</c> does not appear in the gallery until somebody adds it to this
    /// shape, which is the review point.
    /// </para>
    /// </summary>
    public class PublicProductResponse
    {
        /// <summary>The stock-keeping unit, so a visitor can quote it down the telephone.</summary>
        public string Sku { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>The free-text category, used by the gallery to group the page.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Administrator-supplied photograph. Null falls back to the category artwork.</summary>
        public string? ImageUrl { get; set; }
    }
}
