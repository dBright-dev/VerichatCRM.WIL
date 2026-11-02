using VerichatCRM.Web.Models.Inquiry;

namespace VerichatCRM.Web.Models.Sales
{
    public class LeadsViewModel
    {
        public List<InquiryViewModel> Leads { get; set; } = new();
        public List<string> Filters { get; set; } = new();
        public string ActiveFilter { get; set; } = "All";
        public int OpenCount { get; set; }
        public int DueTodayCount { get; set; }
        public int OverdueCount { get; set; }
    }
}