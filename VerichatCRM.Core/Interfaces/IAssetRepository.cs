using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface IAssetRepository
    {
        Task<IEnumerable<Asset>> GetAllAsync();
        Task<IEnumerable<Asset>> GetByClientAsync(string clientId);
        Task<IEnumerable<Asset>> GetWarrantyExpiringAsync(int daysAhead);
        Task<Asset?> GetByIdAsync(string assetId);
        Task<Asset> AddAsync(Asset asset);
        Task UpdateAsync(Asset asset);
        Task AddServiceLogAsync(AssetServiceLog log);
    }
}
