using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateInquiryRequest
    {
        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string ContactName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? ContactEmail { get; set; }

        [MaxLength(40)]
        public string? ContactPhone { get; set; }

        [MaxLength(160)]
        public string? Company { get; set; }

        public string? ClientId { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public string? Source { get; set; }

        /// <summary>Immediate | High | Standard | Flexible.</summary>
        [MaxLength(30)]
        public string? Urgency { get; set; }

        /// <summary>What the enquiry is worth if it closes, in rand.</summary>
        public decimal? EstimatedValue { get; set; }

        /// <summary>
        /// 0-100. Left out, the enquiry is scored from its sector, contract
        /// status, value and urgency; supplied, that number is stored as entered.
        /// </summary>
        [Range(0, 100)]
        public int? Score { get; set; }
    }
}