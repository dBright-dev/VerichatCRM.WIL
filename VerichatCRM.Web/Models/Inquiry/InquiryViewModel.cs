using VerichatCRM.Core.Models.Clients;

namespace VerichatCRM.Web.Models.Inquiry
{
    public class InquiryViewModel
    {
        public string InquiryId { get; set; } = "";
        public string? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string ContactName { get; set; } = "";
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Company { get; set; }
        public string Subject { get; set; } = "";
        public string? Description { get; set; }
        public string Source { get; set; } = "";
        public string Status { get; set; } = "";
        public int Score { get; set; }
        public string? Notes { get; set; }
        public DateTime? ResponseDueAt { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        // Supplied by the API once the follow-up tracking endpoints ship.
        public DateTime? LastContactDate { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public string? FollowUpStatus { get; set; }

        // Falls back to the SLA response time until LastContactDate exists.
        public DateTime? EffectiveLastContact => LastContactDate ?? RespondedAt;
    }

    public class InquirySummaryViewModel
    {
        public int Total { get; set; }
        public int Open { get; set; }
        public int New { get; set; }
        public int Overdue { get; set; }
        public int Won { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class InquiryFormViewModel
    {
        public string? InquiryId { get; set; }
        public string Subject { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Company { get; set; }
        public string? ClientId { get; set; }
        public string? Description { get; set; }
        public string Source { get; set; } = "Website";
        public string Status { get; set; } = "New";
        public int Score { get; set; } = 50;
        public string? Notes { get; set; }

        public List<ClientViewModel> Clients { get; set; } = new();
    }

    public class InquiryQueueViewModel
    {
        public List<InquiryViewModel> Inquiries { get; set; } = new();
        public InquirySummaryViewModel Summary { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
        public List<string> Sources { get; set; } = new();
        public string ActiveStatus { get; set; } = "All";
    }
}