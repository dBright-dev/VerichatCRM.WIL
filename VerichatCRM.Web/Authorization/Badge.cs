namespace VerichatCRM.Web.Authorization;

// The status-to-colour mapping, in one place. Eleven views each carried their own
// switch for it (Support/Dashboard, Support/TicketHub, Support/TicketDetail,
// Logistics/Tracker, Logistics/Assets, Procurement/WarehouseAlerts,
// Portal/Index, Portal/Tickets, Portal/Quotations, Portal/Catalogue and
// Procurement/ProductCatalog) and they had already drifted — the same "In Transit"
// was badge-info in one and badge-warning in another, "Expired" was grey on the
// portal and unhandled on the staff side, and only one of them knew about the
// Medium priority at all.
// Views call them as Badge.For(model.Status) and Badge.Tone(model.Status).
// DESIGN-SYSTEM.md 4.
public static class Badge
{
    public static string For(string? value) => value switch
    {
        // Priorities
        "Critical" or "High risk" => "badge-critical",
        "High" => "badge-warning",
        "Medium" => "badge-info",
        "Low" => "badge-neutral",

        // The alert's own word for a shortage. "Low" was already taken by the
        // priority above, which is why WarehouseAlerts called one "Low" and got a
        // grey chip.
        "Warning" => "badge-warning",

        // Stock, spelled identically on both catalogues. The client portal said
        // "Low stock" and the buyer's own product list said "Low Stock", and only
        // the portal had a word for zero.
        "Out of stock" => "badge-critical",
        "Low stock" => "badge-warning",
        "In stock" => "badge-success",

        // Live work
        "Open" or "New" or "Processing" or "Needs Service" or "Expired" => "badge-neutral",
        "Assigned" or "In Progress" or "Pending" or "Awaiting Parts" => "badge-info",
        "Escalated" or "Overdue" or "Delayed" or "Breached" or "Failed" => "badge-critical",

        // Money and pipeline
        "Won" or "Approved" or "Accepted" => "badge-success",
        "Lost" or "Cancelled" or "Rejected" => "badge-critical",
        "Quoted" or "Negotiating" or "Prospecting" => "badge-info",

        // Finished work
        "Delivered" or "Resolved" or "Closed" or "Completed" or "Installed" or "OK" => "badge-success",
        // On the road: in flight is a fact, not a warning. The tracker drew
        // "In Transit" as info and the ticket queue as warning for the same word.
        "Dispatched" or "In Transit" or "Sent" => "badge-info",
        "On Hold" or "Due Soon" or "Under Review" => "badge-warning",
        "Returned" => "badge-critical",

        // A response was logged but the ticket stays open until the client agrees.
        "Action Taken" => "badge-info",

        _ => "badge-neutral"
    };

    /// <summary>
    /// The same vocabulary as <see cref="For"/> but as coloured text rather than a
    /// chip, for the ticket rows where a second badge beside the priority badge
    /// would be too loud (Support/TicketHub, Support/TicketDetail). Both views used
    /// to carry an identical private copy of this switch.
    /// </summary>
    public static string Tone(string? value) => For(value) switch
    {
        "badge-critical" => "status-critical",
        "badge-warning" => "status-warning",
        "badge-success" => "status-ok",
        // Info and neutral have no text-tone rule in the theme; the label simply
        // stays in the muted .status-text colour it inherits.
        _ => ""
    };

    /// <summary>
    /// The one wording for a catalogue quantity, so the client portal and the
    /// buyer's product list cannot name the same shelf state differently.
    /// </summary>
    public static string StockLabel(int stockQty, bool isLowStock) =>
        stockQty == 0 ? "Out of stock" : isLowStock ? "Low stock" : "In stock";
}
