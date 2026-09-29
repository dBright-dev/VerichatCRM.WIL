using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly AppDbContext _context;

        public AssetRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Asset>> GetAllAsync() =>
            await _context.Assets
                .Include(a => a.ServiceLogs)
                .OrderBy(a => a.SerialNumber)
                .ToListAsync();

        public async Task<IEnumerable<Asset>> GetByClientAsync(string clientId) =>
            await _context.Assets
                .Include(a => a.ServiceLogs)
                .Where(a => a.ClientId == clientId)
                .ToListAsync();

        public async Task<IEnumerable<Asset>> GetWarrantyExpiringAsync(int daysAhead)
        {
            var cutoff = DateTime.UtcNow.AddDays(daysAhead);
            return await _context.Assets
                .Where(a => a.WarrantyExpiry != null
                         && a.WarrantyExpiry <= cutoff
                         && a.WarrantyExpiry >= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(string assetId) =>
            await _context.Assets
                .Include(a => a.ServiceLogs)
                .FirstOrDefaultAsync(a => a.AssetId == assetId);

        public async Task<Asset> AddAsync(Asset asset)
        {
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return asset;
        }

        public async Task UpdateAsync(Asset asset)
        {
            _context.Assets.Update(asset);
            await _context.SaveChangesAsync();
        }

        public async Task AddServiceLogAsync(AssetServiceLog log)
        {
            var asset = await _context.Assets
                .Include(a => a.ServiceLogs)
                .FirstOrDefaultAsync(a => a.AssetId == log.AssetId);

            if (asset == null) return;
            asset.ServiceLogs.Add(log);
            asset.LastServiceDate = log.ServiceDate;
            asset.NextServiceDue = log.ServiceDate.AddMonths(6);
            await _context.SaveChangesAsync();
        }
    }
}
