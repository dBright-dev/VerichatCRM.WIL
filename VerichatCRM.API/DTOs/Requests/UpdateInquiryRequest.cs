using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.API.DTOs.Requests
{
    /// <summary>Partial update. Only supplied fields are changed.</summary>
    public class UpdateInquiryRequest
    {
        [MaxLength(200)]
        public string? Subject { get; set; }

        [MaxLength(120)]
        public string? ContactName { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? ContactEmail { get; set; }

        [MaxLength(40)]
        public string? ContactPhone { get; set; }

        [MaxLength(160)]
        public string? Company { get; set; }

        public string? ClientId { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }

        public string? Status { get; set; }

        [Range(0, 100)]
        public int? Score { get; set; }

        public string? OwnerUserId { get; set; }
    }
}