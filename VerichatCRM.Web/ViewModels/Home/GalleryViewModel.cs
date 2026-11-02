using VerichatCRM.Web.Models.Public;
using VerichatCRM.Web.Services;

namespace VerichatCRM.Web.ViewModels.Home;

/// <summary>
/// One tile. <paramref name="Detail"/> is the line under the name: the SKU for a
/// catalogue row, the variant for a showcase item, because a gallery needs something to
/// read as fact beside the photograph.
/// </summary>
public sealed record GalleryItem(string Name, string Detail, string Category, string? Image, string Alt);

/// <summary>One sector: the copy that frames it and everything stocked for it.</summary>
public sealed record GalleryGroup(
    string Name,
    string Body,
    string Image,
    string Alt,
    IReadOnlyList<GalleryItem> Items);

/// <summary>
/// The public gallery's model: catalogue rows from the API, bucketed into the three
/// sectors the company says it serves.
/// <para>
/// Two things decide the shape. The first is that a product's category is free text, so
/// grouping is by keyword rather than by a key, and a row that matches no sector lands in
/// a fourth honest group instead of being quietly dropped - a visitor who cannot find a
/// line they know we stock will conclude we do not stock it.
/// </para>
/// <para>
/// The second is that this is the entry point for people with no account. If the API is
/// unreachable, or the catalogue is still empty, a blank grid reads as a company with
/// nothing to sell, so the page falls back to the standing showcase already published on
/// the landing page. The screen says which of the two it is showing; a fallback is not
/// passed off as the warehouse list.
/// </para>
/// </summary>
public class GalleryViewModel
{
    public const string OtherGroupName = "Also stocked";

    /// <summary>
    /// Keyword stems against the index into <see cref="LandingContent.Sectors"/> that they
    /// belong to. Anything that matches none is clinical, which is the sector most of the
    /// catalogue is filed under today.
    /// </summary>
    private static readonly (string[] Keywords, int Sector)[] BucketBy =
    {
        (new[] { "mine", "mining", "industrial", "rescue" }, 1),
        (new[] { "educat", "teach", "laborator", "research", "student", "training" }, 2),
    };

    private static int SectorFor(string? category)
    {
        var value = (category ?? string.Empty).Trim().ToLowerInvariant();
        if (value.Length == 0) return -1;

        foreach (var (keywords, sector) in BucketBy)
        {
            if (keywords.Any(value.Contains)) return sector;
        }

        return 0;
    }

    public IReadOnlyList<GalleryGroup> Groups { get; private init; } = Array.Empty<GalleryGroup>();

    /// <summary>False when the page is showing the standing showcase rather than the catalogue.</summary>
    public bool FromDatabase { get; private init; }

    public int ItemCount => Groups.Sum(g => g.Items.Count);

    public static GalleryViewModel From(IReadOnlyList<PublicProductItem> products)
    {
        if (products.Count == 0) return FromShowcase();

        // A category is not a sector, so a row like "Home Care & Rehabilitation" reaches
        // clinical rather than a fourth group, and only a genuinely blank category falls
        // through to the honest "Also stocked" set.
        //
        // The photograph is tried per item first, then per category: an uploaded ImageUrl
        // always wins, a catalogue photograph named after the product comes next, and a
        // category image is what an item with no photo of its own is filed under.
        var tiles = products
            .Select(p => (Sector: SectorFor(p.Category), Item: Tile(p.Sku, p.Name, p.Category,
                string.IsNullOrWhiteSpace(p.ImageUrl)
                    ? CatalogueArt.ProductImageFor(p.Name) ?? CatalogueArt.ImageFor(p.Category)
                    : p.ImageUrl)))
            .ToList();

        var groups = LandingContent.Sectors
            .Select((sector, index) => Group(sector, tiles.Where(t => t.Sector == index).Select(t => t.Item)))
            .ToList();

        var leftovers = tiles.Where(t => t.Sector == -1).Select(t => t.Item).ToList();
        if (leftovers.Count > 0)
        {
            groups.Add(new GalleryGroup(
                OtherGroupName,
                "In the catalogue, and not yet filed under a sector by whoever maintains it. " +
                "Ask for it the same way as anything else on this page.",
                LandingContent.Sectors[0].Image,
                LandingContent.Sectors[0].Alt,
                leftovers));
        }

        return new GalleryViewModel { Groups = groups, FromDatabase = true };
    }

    private static GalleryViewModel FromShowcase()
    {
        var tiles = LandingContent.Showcase
            .Select(item => (Sector: SectorFor(item.Category), Item: new GalleryItem(
                item.Name, item.Variant, item.Category, item.Image, item.Alt)))
            .ToList();

        var groups = LandingContent.Sectors
            .Select((sector, index) => Group(sector, tiles.Where(t => t.Sector == index).Select(t => t.Item)))
            .ToList();

        var leftovers = tiles.Where(t => t.Sector == -1).Select(t => t.Item).ToList();
        if (leftovers.Count > 0)
        {
            groups.Add(new GalleryGroup(
                OtherGroupName,
                "In the catalogue, and not yet filed under a sector by whoever maintains it.",
                LandingContent.Sectors[0].Image,
                LandingContent.Sectors[0].Alt,
                leftovers));
        }

        return new GalleryViewModel { Groups = groups, FromDatabase = false };
    }

    private static GalleryGroup Group(
        SectorCard sector,
        IEnumerable<GalleryItem> items) =>
        new(sector.Name, sector.Body, sector.Image, sector.Alt, items.ToList());

    private static GalleryItem Tile(string sku, string name, string category, string? image) =>
        new(name, "SKU " + sku + " · " + category, category, image, name + " — " + category);
}
