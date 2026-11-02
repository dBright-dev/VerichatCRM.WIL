namespace VerichatCRM.Web.ViewModels.Home;

/// <summary>
/// The contact facts the public pages publish, in one place.
/// <para>
/// Four pages now show this block - the privacy notice, the company profile, the
/// services page and contact us - and a phone number that is right on one of them and
/// stale on the others is worse than no number at all. Every page renders it from here.
/// </para>
/// <para>
/// The hrefs are built rather than written out because the markup cannot hold them: a
/// literal <c>@</c> in a Razor view has to be authored as <c>&amp;#64;</c>, which is
/// correct in the browser but wrong as soon as C# compares the string to something.
/// </para>
/// </summary>
public static class VerichatContact
{
    public const string Company = "Verichat Quality Projects";

    public const string Street = "11 Lincoln Road";
    public const string Suburb = "Avondale";
    public const string City = "Harare";
    public const string Country = "Zimbabwe";

    /// <summary>What a map, a courier or a form field needs: one line, commas included.</summary>
    public static string MapsQuery { get; } = $"{Street}, {Suburb}, {City}, {Country}";

    public const string Phone = "+263 771 180 001";
    public const string PhoneAlternate = "+263 774 697 662";
    public const string Email = "sales@verichat.co.zw";

    public static string EmailHref { get; } = "mailto:" + Email;

    /// <summary>
    /// A <c>mailto:</c> that already carries a subject, for the second route the receipt
    /// screen offers. The body is not filled from what the visitor typed: a page that
    /// echoes back a submission in a URL is one more place their words can be read, and the
    /// reference in the subject line is enough for the desk to find the enquiry again.
    /// </summary>
    public static string BackupHref(string subject) =>
        EmailHref + "?subject=" + Uri.EscapeDataString(subject);

    /// <summary>
    /// A <c>tel:</c> URI keeps the <c>+</c>, because that is what makes the number
    /// international; everything else that is not a digit - the spaces a human reads by
    /// - is dropped.
    /// </summary>
    public static string Tel(string displayed) =>
        "tel:" + new string((displayed ?? string.Empty).Where(c => char.IsDigit(c) || c == '+').ToArray());

    public static string PhoneHref { get; } = Tel(Phone);
    public static string PhoneAlternateHref { get; } = Tel(PhoneAlternate);

    /// <summary>
    /// Directions offered as a link the visitor chooses to follow, never as an embedded
    /// map. An iframe from a mapping provider loads that provider's scripts on our page
    /// and sets their cookies under our domain's appearance, which is exactly what the
    /// published privacy notice rules out ("No trackers"). A link costs the visitor
    /// nothing until they click it, and then they are on the map site knowing they went
    /// there.
    /// </summary>
    public static string GoogleMapsHref { get; } =
        "https://www.google.com/maps/search/?api=1&query=" + Uri.EscapeDataString(MapsQuery);

    public static string OpenStreetMapHref { get; } =
        "https://www.openstreetmap.org/search?query=" + Uri.EscapeDataString(MapsQuery);
}
