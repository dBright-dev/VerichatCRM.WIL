<<<<<<< Updated upstream
﻿namespace VerichatCRM.API.Services
=======
﻿using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Enums;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Services
>>>>>>> Stashed changes
{
    public class DeliveryLogService : IDeliveryLogService
    {
        private readonly IDeliveryLogRepository _deliveries;

        public DeliveryLogService(IDeliveryLogRepository deliveries) => _deliveries = deliveries;

        public async Task<IEnumerable<DeliveryLogResponse>> GetAllAsync() =>
            (await _deliveries.GetAllAsync()).Select(Map);

        public async Task<IEnumerable<DeliveryLogResponse>> GetByClientAsync(string clientId) =>
            (await _deliveries.GetByClientAsync(clientId)).Select(Map);

        public async Task<DeliveryLogResponse?> GetByTrackingAsync(string trackingNumber)
        {
            var d = await _deliveries.GetByTrackingAsync(trackingNumber);
            return d == null ? null : Map(d);
        }

        public async Task<DeliveryLogResponse?> UpdateStatusAsync(string deliveryId, UpdateDeliveryStatusRequest request, string userId)
        {
            var all = await _deliveries.GetAllAsync();
            var delivery = all.FirstOrDefault(d => d.DeliveryId == deliveryId);
            if (delivery == null) return null;

            if (!DeliveryStatuses.All.Contains(request.NewStatus))
                throw new InvalidOperationException($"Invalid delivery status '{request.NewStatus}'.");

            delivery.Status = request.NewStatus;
            delivery.UpdatedAt = DateTime.UtcNow;

            if (request.NewStatus == DeliveryStatuses.Delivered)
                delivery.ActualArrival = DateTime.UtcNow;

            delivery.StatusHistory.Add(new DeliveryStatusUpdate
            {
                DeliveryId = delivery.DeliveryId,
                Status = request.NewStatus,
                Location = request.Location,
                Notes = request.Notes,
                UpdatedByUserId = userId
            });

            await _deliveries.UpdateAsync(delivery);
            return Map(delivery);
        }

        private static DeliveryLogResponse Map(DeliveryLog d) => new()
        {
            DeliveryId = d.DeliveryId,
            TrackingNumber = d.TrackingNumber,
            OrderId = d.OrderId,
            ClientId = d.ClientId,
            Origin = d.Origin,
            Destination = d.Destination,
            WeightKg = d.WeightKg,
            Carrier = d.Carrier,
            Status = d.Status,
            EstimatedArrival = d.EstimatedArrival,
            ActualArrival = d.ActualArrival,
            StatusHistory = d.StatusHistory
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => new DeliveryStatusUpdateResponse
                {
                    UpdateId = s.UpdateId,
                    Status = s.Status,
                    Location = s.Location,
                    Notes = s.Notes,
                    UpdatedAt = s.UpdatedAt
                }).ToList()
        };
    }
}
