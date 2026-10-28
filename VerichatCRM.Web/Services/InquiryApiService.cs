using VerichatCRM.Web.Models.Inquiry;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class InquiryApiService : IInquiryApiService
    {
        private readonly IApiClient _api;
        private readonly ILogger<InquiryApiService> _logger;

        public InquiryApiService(IApiClient api, ILogger<InquiryApiService> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<List<InquiryViewModel>> GetAllAsync(string? status = null)
        {
            try
            {
                var endpoint = string.IsNullOrWhiteSpace(status) || status == "All"
                    ? "/api/inquiry"
                    : $"/api/inquiry?status={Uri.EscapeDataString(status)}";

                return await _api.GetAsync<List<InquiryViewModel>>(endpoint) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load inquiries from API.");
                return new();
            }
        }

        public async Task<InquirySummaryViewModel> GetSummaryAsync()
        {
            try
            {
                return await _api.GetAsync<InquirySummaryViewModel>("/api/inquiry/summary")
                       ?? new InquirySummaryViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load inquiry summary from API.");
                return new InquirySummaryViewModel();
            }
        }

        public async Task<InquiryViewModel?> CreateAsync(InquiryFormViewModel model)
        {
            try
            {
                return await _api.PostAsync<object, InquiryViewModel>("/api/inquiry", new
                {
                    model.Subject,
                    model.ContactName,
                    model.ContactEmail,
                    model.ContactPhone,
                    model.Company,
                    model.ClientId,
                    model.Description,
                    model.Source,
                    model.Score,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create inquiry {Subject}.", model.Subject);
                return null;
            }
        }

        public async Task<InquiryViewModel?> UpdateAsync(string inquiryId, InquiryFormViewModel model)
        {
            try
            {
                return await _api.PutAsync<object, InquiryViewModel>($"/api/inquiry/{inquiryId}", new
                {
                    model.Subject,
                    model.ContactName,
                    model.ContactEmail,
                    model.ContactPhone,
                    model.Company,
                    model.ClientId,
                    model.Notes,
                    model.Status,
                    model.Score,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update inquiry {Id}.", inquiryId);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string inquiryId)
        {
            try
            {
                // A successful delete returns 204 with no body.
                await _api.DeleteAsync<object>($"/api/inquiry/{inquiryId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete inquiry {Id}.", inquiryId);
                return false;
            }
        }

        public async Task<bool> MarkContactedAsync(string inquiryId, string currentStatus)
        {
            try
            {
                var result = await _api.PostAsync<object, InquiryViewModel>(
                    $"/api/inquiry/{inquiryId}/contact", new { });
                if (result is not null) return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Contact endpoint unavailable, using fallback.");
            }

            // Temporary fallback until Section 2's endpoint ships. It can only
            // advance a New lead to Contacted; other stages need the real endpoint.
            if (currentStatus != "New") return false;

            try
            {
                var updated = await _api.PutAsync<object, InquiryViewModel>(
                    $"/api/inquiry/{inquiryId}", new { Status = "Contacted" });
                return updated is not null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to mark inquiry {Id} as contacted.", inquiryId);
                return false;
            }
        }

        public async Task<bool> AddFollowUpAsync(string inquiryId, DateTime nextFollowUpDate, string? note)
        {
            try
            {
                var result = await _api.PostAsync<object, InquiryViewModel>(
                    $"/api/inquiry/{inquiryId}/follow-up",
                    new { NextFollowUpDate = nextFollowUpDate, Note = note });
                return result is not null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add follow-up for inquiry {Id}.", inquiryId);
                return false;
            }
        }
    }
}