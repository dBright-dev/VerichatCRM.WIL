using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllAsync(string? search, string? sector);
        Task<ClientDto?> GetByIdAsync(string clientId);
        Task<ClientDto> CreateAsync(CreateClientRequest request);
        Task<ClientDto?> UpdateAsync(string clientId, CreateClientRequest request);
        Task<bool> DeleteAsync(string clientId);
    }
}
