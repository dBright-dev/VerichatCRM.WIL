using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.Models.Support;

namespace VerichatCRM.Web.Services
{
    public class SupportApiService : ISupportApiService
    {
        private readonly IApiClient _api;

        public SupportApiService(IApiClient api) => _api = api;

        public async Task<List<TicketDto>> GetTicketsAsync() =>
            await _api.GetAsync<List<TicketDto>>("/api/supportticket") ?? new();

        public async Task<TicketDto?> GetTicketAsync(string ticketId) =>
            await _api.GetAsync<TicketDto>($"/api/supportticket/{ticketId}");

        public async Task<TicketDto?> CreateTicketAsync(CreateTicketDto dto) =>
            await _api.PostAsync<CreateTicketDto, TicketDto>("/api/supportticket", dto);

        public async Task<TicketDto?> UpdateTicketStatusAsync(string ticketId, string newStatus, string? notes) =>
            await _api.PatchAsync<object, TicketDto>(
                $"/api/supportticket/{ticketId}/status",
                new { NewStatus = newStatus, Notes = notes });

        public async Task<List<AssetDto>> GetAssetsAsync() =>
            await _api.GetAsync<List<AssetDto>>("/api/asset") ?? new();

        public async Task<List<DeliveryDto>> GetDeliveriesAsync() =>
            await _api.GetAsync<List<DeliveryDto>>("/api/delivery") ?? new();
    }
}
