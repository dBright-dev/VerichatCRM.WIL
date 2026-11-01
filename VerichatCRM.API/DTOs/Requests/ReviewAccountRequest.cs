using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.API.DTOs.Requests
{
    /// <summary>
    /// The reviewer's answer to an application. A decline has to carry the reason,
    /// because the applicant is told it and cannot act on a blank.
    /// </summary>
    public class ReviewAccountRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; } = string.Empty;
    }
}
