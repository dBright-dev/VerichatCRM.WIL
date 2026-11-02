using System.Text;

namespace VerichatCRM.Web.Services;

/// <summary>
/// Chooses the photograph shown for a catalogue product.
/// <para>
/// Two lookups, both by name. A product's own photograph is found by slugging its name and
/// checking that the file is actually on disk, so a tile never points at a 404; when there
/// is none, the category's photograph stands in. Categories are validated against
/// <c>ProductCategories</c> at the API, but rows written before that rule existed still
/// carry values like "Clinical", so the alias table below maps the vocabulary the
/// procurement team actually uses onto the files that exist.
/// </para>
/// <para>
/// An unmapped category yields a path to a file that does not exist - the three catalogue
/// views each hide the image on error and fall back to the placeholder they already used.
/// </para>
/// </summary>
public static class CatalogueArt
{
    private static string? _webRoot;

    // The product photographs are read once per process. A file added to wwwroot after
    // startup is not picked up until the app restarts, which is fine: the photographs are
    // deployed with the site, and anything an administrator uploads goes through
    // Product.ImageUrl instead, which is checked before this cache is consulted.
    private static readonly Lazy<HashSet<string>> ProductPhotos = new(() =>
        string.IsNullOrEmpty(_webRoot) || !Directory.Exists(ProductsDirectory)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(
                Directory.EnumerateFiles(ProductsDirectory).Select(name => Path.GetFileName(name)!),
                StringComparer.OrdinalIgnoreCase));

    private static string ProductsDirectory =>
        Path.Combine(_webRoot!, "images", "catalogue", "products");

    /// <summary>
    /// Points the resolver at the served content root. Called once from Program.cs before
    /// the first request; without it every product simply uses its category photograph.
    /// </summary>
    public static void UseWebRoot(string? webRoot) => _webRoot = webRoot;

    // Categories against the photography from the Verichat company profile that best
    // represents them. The files predate the validated category list and are named after
    // what a sector was called in the profile ("hospital-equipment", "laboratory-equipment"),
    // so an alias is cheaper than renaming a deployed asset. The four short values are the
    // free-text labels the old DataSeeder wrote, kept working for rows that were never
    // re-filed. A category with no entry and no matching file falls through to the caller's
    // on-error placeholder.
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Clinical"] = "hospital-equipment",
        ["Rehab"] = "home-care-and-rehabilitation",
        ["Education"] = "laboratory-equipment",
        ["Consumables"] = "hospital-equipment",
        ["Laboratory"] = "laboratory-equipment",
        ["Home Care"] = "home-care-and-rehabilitation",
        ["Hospital Infrastructure"] = "hospital-equipment",
    };

    public static string? ImageFor(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return null;

        var name = Aliases.TryGetValue(category.Trim(), out var alias) ? alias : Slug(category);
        return string.IsNullOrEmpty(name) ? null : $"/images/catalogue/{name}.jpg";
    }

    /// <summary>
    /// The photograph of this specific item, when the catalogue holds one: a file named
    /// after the product under <c>/images/catalogue/products/</c>. Null when no such file
    /// is served, so the caller falls through to the category photograph.
    /// <para>
    /// This is a convention, not a column. <c>Product.ImageUrl</c> stays the override for
    /// a photograph an administrator uploads, and it has to be an absolute http(s) URL
    /// because the mobile app loads it directly; a relative path baked into the database
    /// would be a lie on whichever host did not serve it. The gallery, the client portal
    /// and the procurement list all reach the same files by deriving the name here, so
    /// adding photography is a drop-in with no database write behind it.
    /// </para>
    /// </summary>
    public static string? ProductImageFor(string? productName)
    {
        var slug = Slug(productName);
        if (slug.Length == 0) return null;

        var file = slug + ".jpg";
        return ProductPhotos.Value.Contains(file) ? "/images/catalogue/products/" + file : null;
    }

    private static string Slug(string? text)
    {
        var source = (text ?? string.Empty).Trim().ToLowerInvariant();
        var sb = new StringBuilder(source.Length);
        var dash = false;
        foreach (var c in source)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                dash = false;
            }
            else if (sb.Length > 0 && !dash)
            {
                sb.Append('-');
                dash = true;
            }
        }
        return sb.ToString().TrimEnd('-');
    }
}
