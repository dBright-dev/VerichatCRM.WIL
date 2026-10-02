<<<<<<< Updated upstream
﻿namespace VerichatCRM.API.Services
=======
﻿using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Services
>>>>>>> Stashed changes
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assets;

        public AssetService(IAssetRepository assets) => _assets = assets;

        public async Task<IEnumerable<AssetResponse>> GetAllAsync() =>
            (await _assets.GetAllAsync()).Select(Map);

        public async Task<IEnumerable<AssetResponse>> GetByClientAsync(string clientId) =>
            (await _assets.GetByClientAsync(clientId)).Select(Map);

        public async Task<IEnumerable<AssetResponse>> GetWarrantyExpiringAsync(int daysAhead) =>
            (await _assets.GetWarrantyExpiringAsync(daysAhead)).Select(Map);

        public async Task<AssetResponse?> GetByIdAsync(string assetId)
        {
            var a = await _assets.GetByIdAsync(assetId);
            return a == null ? null : Map(a);
        }

        public async Task<AssetResponse> CreateAsync(CreateAssetRequest request)
        {
            var asset = new Asset
            {
                SerialNumber = request.SerialNumber.Trim(),
                Model = request.Model.Trim(),
                Manufacturer = request.Manufacturer.Trim(),
                ClientId = request.ClientId,
                Location = request.Location,
                WarrantyExpiry = request.WarrantyExpiry,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _assets.AddAsync(asset);
            return Map(asset);
        }

        public async Task<AssetServiceLogResponse?> AddServiceLogAsync(string assetId, CreateAssetServiceLogRequest request, string userId)
        {
            var asset = await _assets.GetByIdAsync(assetId);
            if (asset == null) return null;

            var log = new AssetServiceLog
            {
                AssetId = assetId,
                ServiceDate = request.ServiceDate,
                Description = request.Description.Trim(),
                Outcome = request.Outcome,
                PartsReplaced = request.PartsReplaced,
                PerformedByUserId = userId
            };

            await _assets.AddServiceLogAsync(log);

            return new AssetServiceLogResponse
            {
                LogId = log.LogId,
                ServiceDate = log.ServiceDate,
                Description = log.Description,
                Outcome = log.Outcome,
                PartsReplaced = log.PartsReplaced
            };
        }

        private static AssetResponse Map(Asset a) => new()
        {
            AssetId = a.AssetId,
            SerialNumber = a.SerialNumber,
            Model = a.Model,
            Manufacturer = a.Manufacturer,
            ClientId = a.ClientId,
            Location = a.Location,
            WarrantyExpiry = a.WarrantyExpiry,
            LastServiceDate = a.LastServiceDate,
            NextServiceDue = a.NextServiceDue,
            Status = a.Status,
            ServiceLogs = a.ServiceLogs
                .OrderByDescending(l => l.ServiceDate)
                .Select(l => new AssetServiceLogResponse
                {
                    LogId = l.LogId,
                    ServiceDate = l.ServiceDate,
                    Description = l.Description,
                    Outcome = l.Outcome,
                    PartsReplaced = l.PartsReplaced
                }).ToList()
        };
    }
}
