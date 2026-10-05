using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Where a prospective enquiry arrived from.</summary>
    public static class InquirySources
    {
        public const string Phone = "Phone";
        public const string Email = "Email";
        public const string Website = "Website";
        public const string WalkIn = "Walk-in";
        public const string MobileApp = "Mobile App";
        public const string Referral = "Referral";

        public static readonly string[] All =
        {
            Phone, Email, Website, WalkIn, MobileApp, Referral
        };
    }

    /// <summary>Lifecycle of an enquiry as it moves toward a quotation.</summary>
    public static class InquiryStatuses
    {
        public const string New = "New";
        public const string Contacted = "Contacted";
        public const string Qualified = "Qualified";
        public const string Quoted = "Quoted";
        public const string Won = "Won";
        public const string Lost = "Lost";

        /// <summary>Statuses that are still open and need attention.</summary>
        public static readonly string[] Open = { New, Contacted, Qualified, Quoted };

        public static readonly string[] All = { New, Contacted, Qualified, Quoted, Won, Lost };
    }

    /// <summary>
    /// An inbound sales enquiry. Sits in front of the pipeline: an enquiry is
    /// qualified and scored first, then converted into a <see cref="Deal"/>.
    /// </summary>
    public class Inquiry
    {
        [Key]
        public string InquiryId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Optional - an enquiry can arrive before a client record exists.</summary>
        public string? ClientId { get; set; }

        public string ContactName { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Company { get; set; }

        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Source { get; set; } = InquirySources.Website;
        public string Status { get; set; } = InquiryStatuses.New;

        /// <summary>Qualification score, 0-100. Higher sorts to the top of the queue.</summary>
        public int Score { get; set; }

        public string? OwnerUserId { get; set; }
        public string? Notes { get; set; }

        /// <summary>SLA target for first response.</summary>
        public DateTime? ResponseDueAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Client? Client { get; set; }
    }
}