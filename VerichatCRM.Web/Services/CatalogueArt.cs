using System.Text;

namespace VerichatCRM.Web.Services;

/// <summary>
/// Chooses the photograph shown behind a catalogue product that has no image of its
/// own. Products arrive with a free-text category, so the mapping is by name rather
/// than by a foreign key, and an unmapped category simply yields a file that does not
/// exist - both catalogue views hide the image on error and fall back to the
/// placeholder they already used.
/// </summary>
public static class CatalogueArt
{
    // Categories the procurement team actually uses today, against the photography
    // from the Verichat company profile that best represents them.
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Clinical"] = "hospital-equipment",
        ["Rehab"] = "home-care-and-rehabilitation",
        ["Education"] = "laboratory-equipment",
        ["Consumables"] = "hospital-equipment",
    };

    public static string? ImageFor(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return null;

        var name = Aliases.TryGetValue(category.Trim(), out var alias) ? alias : Slug(category);
        return string.IsNullOrEmpty(name) ? null : $"/images/catalogue/{name}.jpg";
    }

    private static string Slug(string category)
    {
        var sb = new StringBuilder(category.Length);
        var dash = false;
        foreach (var c in category.Trim().ToLowerInvariant())
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
