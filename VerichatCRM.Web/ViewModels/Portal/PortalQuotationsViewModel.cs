using VerichatCRM.Web.Models.Quotation;

namespace VerichatCRM.Web.ViewModels.Portal
{
    public class PortalQuotationsViewModel
    {
        public List<QuotationDto> Quotations { get; set; } = new();
        public string ActiveStatus { get; set; } = "All";
        public List<string> StatusFilters { get; set; } = new()
        {
            "All", "Draft", "Reviewed", "Sent", "Accepted", "Rejected", "Expired"
        };

        /// <summary>
        /// Whether accept/decline buttons are rendered. The server enforces the
        /// same permission on the POST, so a student who crafts the request still
        /// gets refused - this only keeps the dead button off the screen.
        /// </summary>
        public bool CanRespond { get; set; }

        public int AwaitingResponse => Quotations.Count(q => q.Status == "Sent");
    }
}
