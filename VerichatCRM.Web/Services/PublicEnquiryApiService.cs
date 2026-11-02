using VerichatCRM.Web.Models.Public;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Home;

namespace VerichatCRM.Web.Services
{
    /// <summary>
    /// Carries a contact-page enquiry to the API's public door. The web tier is the
    /// only place a session cookie exists, and this call has no session to send -
    /// <see cref="ApiClient"/> omits the header when there is nothing in the session,
    /// so an anonymous visitor posts here exactly as they do to the sign-in form.
    /// </summary>
    public class PublicEnquiryApiService : IPublicEnquiryApiService
    {
        private readonly IApiClient _api;

        public PublicEnquiryApiService(IApiClient api)
        {
            _api = api;
        }

        public async Task<PublicEnquiryResult> SubmitAsync(PublicEnquiryForm form)
        {
            // The honeypot never travels: the controller drops a submission that
            // filled it before this runs, so anything reaching the API is a form a
            // person could see. Channel is stated rather than guessed from a user
            // agent, because this page is served to browsers and to the mobile app's
            // web view alike.
            var payload = new
            {
                name = form.Name,
                email = form.Email,
                phone = form.Phone,
                organisation = form.Organisation,
                sector = form.Sector,
                subject = form.Subject,
                message = form.Message,
                urgency = form.Urgency,
                channel = "Web"
            };

            // Parsed whatever the status code says: a 400 from this endpoint carries
            // the sentence the desk would want the visitor to read, not a transport
            // failure.
            return await _api.SendForBodyAsync<object, PublicEnquiryResult>(
                       HttpMethod.Post, "/api/public/enquiry", payload)
                   ?? new PublicEnquiryResult
                   {
                       Success = false,
                       Message = _api.LastError ??
                           "The enquiry could not be filed just now. Send it to " +
                           VerichatContact.Email +
                           " or telephone the office and we will take it from there."
                   };
        }
    }
}
