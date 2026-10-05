using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IInquiryService
    {
        Task<IEnumerable<InquiryResponse>> GetInquiriesAsync(string? status, string? source, int? minScore);
        Task<InquiryResponse?> GetByIdAsync(string inquiryId);
        Task<InquiryResponse> CreateAsync(CreateInquiryRequest request);
        Task<InquiryResponse?> UpdateAsync(string inquiryId, UpdateInquiryRequest request);
        Task<bool> DeleteAsync(string inquiryId);

        /// <summary>Headline counts used by the role dashboards.</summary>
        Task<InquirySummary> GetSummaryAsync();
    }

    /// <summary>Counts scoped to the enquiry queue.</summary>
    public class InquirySummary
    {
        public int Total { get; set; }
        public int Open { get; set; }
        public int New { get; set; }
        public int Overdue { get; set; }
        public int Won { get; set; }
        public decimal AverageScore { get; set; }
    }
}