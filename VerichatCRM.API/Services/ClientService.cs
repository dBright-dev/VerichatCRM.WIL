using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clients;

        public ClientService(IClientRepository clients) => _clients = clients;

        public async Task<IEnumerable<ClientDto>> GetAllAsync(string? search, string? sector)
        {
            var all = await _clients.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(c =>
                    c.ClientName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.ContactPerson.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(sector) && sector != "All")
                all = all.Where(c => c.Sector.Equals(sector, StringComparison.OrdinalIgnoreCase));

            var result = new List<ClientDto>();
            foreach (var c in all)
            {
                var total = await _clients.GetTotalDealsValueAsync(c.ClientId);
                result.Add(Map(c, total));
            }
            return result;
        }

        public async Task<ClientDto?> GetByIdAsync(string clientId)
        {
            var c = await _clients.GetByIdAsync(clientId);
            if (c == null) return null;
            var total = await _clients.GetTotalDealsValueAsync(clientId);
            return Map(c, total);
        }

        public async Task<ClientDto> CreateAsync(CreateClientRequest r)
        {
            var client = new Client
            {
                ClientName = r.ClientName.Trim(),
                Sector = r.Sector.Trim(),
                ContactPerson = r.ContactPerson.Trim(),
                ContactEmail = r.ContactEmail.Trim().ToLowerInvariant(),
                ContactPhone = r.ContactPhone.Trim(),
                Address = r.Address.Trim(),
                ContractStatus = "Prospect"
            };
            await _clients.AddAsync(client);
            return Map(client, 0m);
        }

        public async Task<ClientDto?> UpdateAsync(string clientId, CreateClientRequest r)
        {
            var c = await _clients.GetByIdAsync(clientId);
            if (c == null) return null;

            c.ClientName = r.ClientName.Trim();
            c.Sector = r.Sector.Trim();
            c.ContactPerson = r.ContactPerson.Trim();
            c.ContactEmail = r.ContactEmail.Trim().ToLowerInvariant();
            c.ContactPhone = r.ContactPhone.Trim();
            c.Address = r.Address.Trim();

            await _clients.UpdateAsync(c);
            var total = await _clients.GetTotalDealsValueAsync(clientId);
            return Map(c, total);
        }

        public Task<bool> DeleteAsync(string clientId) => _clients.DeleteAsync(clientId).ContinueWith(_ => true);

        private static ClientDto Map(Client c, decimal total) => new()
        {
            ClientId = c.ClientId,
            ClientName = c.ClientName,
            Sector = c.Sector,
            ContactPerson = c.ContactPerson,
            ContactEmail = c.ContactEmail,
            ContactPhone = c.ContactPhone,
            ContractStatus = c.ContractStatus,
            AssignedRepId = c.AssignedRepId,
            LastContactDate = c.LastContactDate,
            TotalDealsValue = total
        };
    }
}
