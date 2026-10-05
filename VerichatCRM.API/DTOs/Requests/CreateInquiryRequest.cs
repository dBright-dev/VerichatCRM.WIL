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

        /// <summary>0-100. Defaults to 50 when the caller does not score it.</summary>
        [Range(0, 100)]
        public int Score { get; set; } = 50;
    }
}