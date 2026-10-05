namespace VerichatCRM.API.DTOs.Responses
{
    public class InquiryResponse
    {
        public string InquiryId { get; set; } = string.Empty;
        public string? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Company { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? OwnerUserId { get; set; }
        public string? Notes { get; set; }
        public DateTime? ResponseDueAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        /// <summary>True while the response SLA target has not been met.</summary>
        public bool IsOverdue { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}