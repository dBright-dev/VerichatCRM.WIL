using VerichatCRM.Core.Enums;
using VerichatCRM.Web.Models.Quotation;
using VerichatCRM.Web.Models.Support;

namespace VerichatCRM.Web.ViewModels.Portal
{
    /// <summary>
    /// Landing screen for a client-audience account. Everything on it is the
    /// signed-in client's own data; there is no cross-client view here.
    /// </summary>
    public class PortalHomeViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public List<QuotationDto> Quotations { get; set; } = new();
        public List<TicketDto> Tickets { get; set; } = new();
        public List<DeliveryDto> Deliveries { get; set; } = new();

        public int AwaitingResponse => Quotations.Count(q => q.Status == "Sent");
        public int OpenTickets => Tickets.Count(t => t.Status != "Resolved" && t.Status != "Closed");

        /// <summary>A shipment still counts as moving until it is delivered,
        /// failed or returned - the three statuses logistics can close a job on.</summary>
        public int InTransit => Deliveries.Count(d => d.Status is not (
            DeliveryStatuses.Delivered or DeliveryStatuses.Failed or DeliveryStatuses.Returned));

        public int QuoteCount => Quotations.Count;
        public decimal PendingValue => Quotations.Where(q => q.Status == "Sent").Sum(q => q.GrandTotal);

        public List<QuotationDto> LatestQuotations => Quotations
            .OrderByDescending(q => q.DateGenerated)
            .Take(4)
            .ToList();

        public List<TicketDto> LatestTickets => Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Take(4)
            .ToList();
    }
}
