using VerichatCRM.Web.Models.Client;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class ClientFieldApiService : IClientFieldApiService
    {
        private readonly IApiClient _api;
        private readonly IClientApiService _clients;
        private readonly ILogger<ClientFieldApiService> _logger;

        public ClientFieldApiService(IApiClient api, IClientApiService clients, ILogger<ClientFieldApiService> logger)
        {
            _api = api;
            _clients = clients;
            _logger = logger;
        }

        public async Task<List<MapClientDto>> GetMapDataAsync()
        {
            try
            {
                var data = await _api.GetAsync<List<MapClientDto>>("/api/client/map-data");
                if (data is not null)
                {
                    foreach (var d in data) d.Sector = MapSector.Normalize(d.Sector);
                    return data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "map-data endpoint unavailable, falling back to the client list.");
            }

            var clients = await _clients.GetClientsAsync(null, null);
            return clients.Select(c => new MapClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Sector = MapSector.Normalize(c.Sector),
                Status = c.Status,
                AssignedRep = c.AssignedRep,
                LastContactDate = c.LastContactDate
            }).ToList();
        }

        public async Task<bool> AssignAsync(string clientId, string salesRepId)
        {
            try
            {
                var result = await _api.PostAsync<object, object>(
                    $"/api/client/{Uri.EscapeDataString(clientId)}/assign", new { SalesRepId = salesRepId });
                return result is not null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to assign client {Id}.", clientId);
                return false;
            }
        }

        public async Task<List<AssignmentHistoryItem>> GetAssignmentHistoryAsync(string clientId)
        {
            try
            {
                return await _api.GetAsync<List<AssignmentHistoryItem>>(
                    $"/api/client/{Uri.EscapeDataString(clientId)}/assignment-history") ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Assignment history unavailable for client {Id}.", clientId);
                return new();
            }
        }

        public async Task<bool> LogVisitAsync(string clientId, string? note)
        {
            try
            {
                var result = await _api.PostAsync<object, object>(
                    $"/api/client/{Uri.EscapeDataString(clientId)}/log-visit", new { Note = note });
                return result is not null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log visit for client {Id}.", clientId);
                return false;
            }
        }
    }
}