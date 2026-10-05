using VerichatCRM.Web.Models.Inquiry;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IInquiryApiService
    {
        Task<List<InquiryViewModel>> GetAllAsync(string? status = null);
        Task<InquirySummaryViewModel> GetSummaryAsync();
        Task<InquiryViewModel?> CreateAsync(InquiryFormViewModel model);
        Task<InquiryViewModel?> UpdateAsync(string inquiryId, InquiryFormViewModel model);
        Task<bool> DeleteAsync(string inquiryId);
    }
}