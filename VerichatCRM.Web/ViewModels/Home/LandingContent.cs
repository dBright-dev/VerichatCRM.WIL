namespace VerichatCRM.Web.ViewModels.Home;

public sealed record ShowcaseItem(string Name, string Variant, string Category, string Detail, string Image, string Alt);

public sealed record WorkflowStep(string Number, string Label, string Heading, string Body, string Image, string Alt, string[] Roles);

public sealed record SectorCard(string Name, string Body, string Image, string Alt);

public sealed record TeamMember(string Initial, string Name, string Role, string Focus);

/// <summary>
/// Static marketing copy for the public landing page. Deliberately not read from
/// the database: the page is the entry point for anonymous visitors and must
/// render even when the API or SQL Server is unreachable.
/// </summary>
public static class LandingContent
{
    public static IReadOnlyList<string> Categories { get; } = new[]
    {
        "Clinical consumables", "Operating theatre", "Rehab & mobility", "Mine rescue",
        "Teaching laboratories", "Protective wear", "Calibration & service"
    };

    public static IReadOnlyList<ShowcaseItem> Showcase { get; } = new[]
    {
        new ShowcaseItem(
            "Surgical Mask", "Type IIR · box of 50", "Clinical",
            "Fluid-resistant Type IIR, batch-traceable to the carton and held above its reorder point by the warehouse alerts.",
            "/images/landing/product-surgical-mask.jpg",
            "A neat stack of folded surgical face masks beside a plain carton on a dark navy backdrop."),

        new ShowcaseItem(
            "Wheelchair", "Lightweight aluminium frame", "Rehab & mobility",
            "Folding aluminium frame, delivered assembled, fitted on site and logged against the client's asset register.",
            "/images/landing/product-wheelchair.jpg",
            "A lightweight aluminium manual wheelchair photographed against a dark navy backdrop."),

        new ShowcaseItem(
            "IV Giving Set", "Sterile, single use", "Clinical",
            "Lot-coded sterile sets issued against a tracked consignment, so a ward can see what is coming before it arrives.",
            "/images/landing/product-iv-set.jpg",
            "Coiled clear intravenous giving-set tubing with a drip chamber on a dark navy backdrop."),

        new ShowcaseItem(
            "Lab Coat", "White unisex, institutional cut", "Education",
            "Stocked by size for teaching laboratories, reordered from the same alert that flags the consumables beside it.",
            "/images/landing/product-lab-coat.jpg",
            "A white lab coat presented on an invisible mannequin against a dark navy backdrop.")
    };

    public static IReadOnlyList<WorkflowStep> Workflow { get; } = new[]
    {
        new WorkflowStep(
            "01", "Enquiry", "A ranked queue, not a shared inbox.",
            "A clinic sends an enquiry. The platform scores it on sector, contract status, estimated value and urgency, so the "
            + "next call to make is already at the top of the list — and the quote that follows leaves as a branded PDF.",
            "/images/landing/hero-supply-suite.jpg",
            "A hospital supply store with stainless shelving of sealed cartons lit by cool blue light.",
            new[] { "Sales representative", "Client user" }),

        new WorkflowStep(
            "02", "Supply", "From purchase order to ward door.",
            "Stock below its threshold raises an alert, and accepting that alert writes a real purchase order against a named "
            + "supplier. Every consignment carries a tracking number the client can follow themselves.",
            "/images/landing/dispatch-cold-chain.jpg",
            "A navy transit case and sealed sterile packs on a loading dock at dusk.",
            new[] { "Procurement manager", "Logistics" }),

        new WorkflowStep(
            "03", "Care", "Installed, calibrated, serviced.",
            "Faults become tickets routed by SLA to the engineer who can actually close them. Warranty and calibration dates "
            + "belong to the asset, so a unit that is out of service shows up before the clinic notices.",
            "/images/landing/sector-clinical.jpg",
            "An empty operating theatre with an instrument tray in the foreground.",
            new[] { "Support engineer", "Client user" })
    };

    public static IReadOnlyList<SectorCard> Sectors { get; } = new[]
    {
        new SectorCard(
            "Clinical & hospital",
            "Theatres, wards and outpatient departments — consumables on a reorder cycle, equipment on a calibration cycle.",
            "/images/landing/sector-clinical.jpg",
            "An operating theatre prepared for a procedure, lit in cool clinical tones."),

        new SectorCard(
            "Mining & industrial",
            "Rescue stations and site clinics where a late delivery is a safety problem, so stock levels and ETAs are visible together.",
            "/images/landing/sector-mining.jpg",
            "An underground mine rescue station with a stretcher and response bags on a rack."),

        new SectorCard(
            "Education & research",
            "Teaching laboratories and student placements, where the same catalogue serves procurement and the department office.",
            "/images/landing/sector-education.jpg",
            "A nursing teaching laboratory with training mannequins on trolleys.")
    };

    // This page is published, so it names the functions that own the platform rather than
    // the people who committed the code. Each entry is a company role and what that role is
    // accountable for once the system is live.
    public static IReadOnlyList<TeamMember> Team { get; } = new[]
    {
        new TeamMember("E", "Chief Executive Officer", "Executive sponsor",
            "Chooses the sectors the company serves, signs the client contracts and owns the "
            + "roadmap the procurement platform is built to support."),
        new TeamMember("O", "Procurement & Logistics Manager", "Supply chain",
            "Owns the catalogue, the supplier records and the path from a warehouse alert to a "
            + "purchase order the client can track."),
        new TeamMember("C", "Client Support Manager", "Client services",
            "Owns the ticket queue, the SLA commitments and the calibration calendar behind every "
            + "asset installed on site.")
    };
}
