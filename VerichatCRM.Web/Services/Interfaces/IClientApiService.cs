using VerichatCRM.Core.Models.Clients;
using VerichatCRM.Web.Models.Client;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IClientApiService
    {
        Task<List<ClientViewModel>> GetClientsAsync(string? search, string? sector);
        Task<ClientViewModel?> GetClientAsync(string clientId);
        Task<ClientViewModel?> CreateAsync(CreateClientViewModel model);
        Task<bool> UpdateAsync(string clientId, CreateClientViewModel model);
        Task<bool> DeleteAsync(string clientId);
    }

}
