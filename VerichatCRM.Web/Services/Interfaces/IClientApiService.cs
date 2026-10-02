using VerichatCRM.Core.Models.Clients;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IClientApiService
    {
        Task<List<ClientViewModel>> GetClientsAsync(string? search, string? sector);
        Task<ClientViewModel?> GetClientAsync(string clientId);
    }

}
