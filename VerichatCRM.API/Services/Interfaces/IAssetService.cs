using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces;

public interface IAssetService
{
    Task<IEnumerable<AssetResponse>> GetAllAsync();
    Task<IEnumerable<AssetResponse>> GetByClientAsync(string clientId);
    Task<IEnumerable<AssetResponse>> GetWarrantyExpiringAsync(int daysAhead);
    Task<AssetResponse?> GetByIdAsync(string assetId);
    Task<AssetResponse> CreateAsync(CreateAssetRequest request);
    Task<AssetServiceLogResponse?> AddServiceLogAsync(
        string assetId,
        CreateAssetServiceLogRequest request,
        string userId);
}