using VerichatCRM.Web.Models.Client;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IClientFieldApiService
    {
        Task<List<MapClientDto>> GetMapDataAsync();
        Task<bool> AssignAsync(string clientId, string salesRepId);
        Task<List<AssignmentHistoryItem>> GetAssignmentHistoryAsync(string clientId);
        Task<bool> LogVisitAsync(string clientId, string? note);
    }
}