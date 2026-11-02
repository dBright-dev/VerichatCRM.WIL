using VerichatCRM.API.DTOs.Requests;

namespace VerichatCRM.API.Services.Interfaces
{
    /// <summary>
    /// Turns a note from the public contact page into an enquiry the sales desk can work.
    /// </summary>
    public interface IPublicEnquiryService
    {
        Task<PublicEnquiryOutcome> SubmitAsync(PublicEnquiryRequest request);
    }

    /// <summary>
    /// What the form is told back. <see cref="Reference"/> is the short form of the
    /// enquiry's own id, printed so a visitor can quote it when they telephone - the desk
    /// can find the row from it, because the id it matches is the primary key.
    /// </summary>
    public record PublicEnquiryOutcome(bool Success, string Message, string? Reference = null);
}
