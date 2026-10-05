using VerichatCRM.Core.Models.Clients;
using VerichatCRM.Web.Models.Client;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class ClientApiService : IClientApiService
    {
        private readonly IApiClient _api;

        public ClientApiService(IApiClient api) => _api = api;

        public async Task<List<ClientViewModel>> GetClientsAsync(string? search, string? sector)
        {
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(sector)) qs.Add($"sector={Uri.EscapeDataString(sector)}");
            var suffix = qs.Any() ? "?" + string.Join("&", qs) : "";

            var dtos = await _api.GetAsync<List<ClientDto>>($"/api/client{suffix}") ?? new();
            return dtos.Select(Map).ToList();
        }

        public async Task<ClientViewModel?> GetClientAsync(string clientId)
        {
            var dto = await _api.GetAsync<ClientDto>($"/api/client/{clientId}");
            return dto == null ? null : Map(dto);
        }

        public async Task<ClientViewModel?> CreateAsync(CreateClientViewModel model)
        {
            var dto = await _api.PostAsync<CreateClientViewModel, ClientDto>("/api/client", model);
            return dto == null ? null : Map(dto);
        }

        public async Task<bool> UpdateAsync(string clientId, CreateClientViewModel model)
        {
            var dto = await _api.PostAsync<CreateClientViewModel, ClientDto>($"/api/client/{clientId}", model);
            return dto != null;
        }

        public async Task<bool> DeleteAsync(string clientId)
        {
            try
            {
                // If your ApiClient doesn't have a DeleteAsync helper, add one
                await _api.PostAsync<object, object>($"/api/client/{clientId}/delete", new { });
                return true;
            }
            catch { return false; }
        }

        private static ClientViewModel Map(ClientDto d) => new()
        {
            Id = d.ClientId,
            Name = d.ClientName,
            Sector = d.Sector,
            ContactPerson = d.ContactPerson,
            ContactEmail = d.ContactEmail,
            ContactPhone = d.ContactPhone,
            Status = d.ContractStatus,
            AssignedRep = d.AssignedRepId ?? "Unassigned",
            TotalDealsValue = d.TotalDealsValue,
            LastContactDate = d.LastContactDate
        };
    }
}
