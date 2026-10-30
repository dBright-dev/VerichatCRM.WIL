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

        //Records a contact today. currentStatus is only used by the temporary fallback.
        Task<bool> MarkContactedAsync(string inquiryId, string currentStatus);
        Task<bool> AddFollowUpAsync(string inquiryId, DateTime nextFollowUpDate, string? note);
    }
}