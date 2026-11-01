using VerichatCRM.Web.Models.Public;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IPublicEnquiryApiService
    {
        Task<PublicEnquiryResult> SubmitAsync(PublicEnquiryForm form);
    }
}
