using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services
{
    public partial class MobileService
    {
        public async Task<ClientDashboardResponse> GetDashboardAsync(string userId)
        {
            var user = await _users.GetByIdAsync(userId);
            var orders = await _db.Orders
                .Where(o => o.ClientId == userId || o.TrackingNumber == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            var active = orders.Count(o => o.Status != "Delivered" && o.Status != "Cancelled");
            var pending = orders.Count(o => o.Status == "Pending" || o.Status == "Processing");
            var tracked = orders
                .Where(o => !string.IsNullOrEmpty(o.TrackingNumber))
                .Select(o => new ClientTrackingOrderDto
                {
                    Id = o.OrderId,
                    Reference = o.TrackingNumber!,
                    Status = o.Status
                }).ToList();

            return new ClientDashboardResponse
            {
                FullName = user?.DisplayName ?? "Client",
                ActiveCount = active,
                PendingCount = pending,
                RecurringCount = 0,
                Vitals = await LatestVitalsAsync(userId),
                NextRecurringOrder = null,
                TrackingOrders = tracked
            };
        }

        private async Task<ClientVitalsDto?> LatestVitalsAsync(string userId)
        {
            var latest = await _db.VitalReadings
                .Where(v => v.UserId == userId)
                .GroupBy(v => v.Type)
                .Select(g => g.OrderByDescending(v => v.RecordedAt).FirstOrDefault())
                .ToListAsync();

            if (latest.Count == 0) return null;

            int? heart = null; double? temp = null; int? sys = null; int? dia = null;
            foreach (var r in latest.Where(r => r != null))
            {
                if (r!.Type == "heartRate" && int.TryParse(r.Value, out var h)) heart = h;
                if (r.Type == "temperature" && double.TryParse(r.Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var t)) temp = t;
                if (r.Type == "bloodPressure")
                {
                    var parts = r.Value.Split('/');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], out var s)) sys = s;
                        if (int.TryParse(parts[1], out var d)) dia = d;
                    }
                }
            }
            return new ClientVitalsDto { HeartRate = heart, Temperature = temp, Systolic = sys, Diastolic = dia };
        }

        public async Task<IEnumerable<MobileDeviceResponse>> GetDevicesAsync(string userId) =>
            await _db.MobileDevices
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Name)
                .Select(d => MapDevice(d))
                .ToListAsync();

        public async Task<MobileDeviceResponse?> RegisterDeviceAsync(string userId, string name, string type, string? externalId)
        {
            var device = new MobileDevice
            {
                UserId = userId,
                Name = name.Trim(),
                Type = type,
                Status = "connected",
                ExternalId = string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim()
            };
            _db.MobileDevices.Add(device);
            await _db.SaveChangesAsync();
            return MapDevice(device);
        }

        public async Task<MobileDeviceResponse?> SyncDeviceAsync(string userId, string deviceId, IReadOnlyList<DeviceSyncReadingDto>? readings)
        {
            var device = await _db.MobileDevices
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId);
            if (device == null) return null;
            device.LastSyncAt = DateTime.UtcNow;
            if (device.Status != "connected") device.Status = "connected";

            // Persist any readings the app pulled from the physical device.
            if (readings != null && readings.Count > 0)
            {
                foreach (var r in readings.Where(r => !string.IsNullOrWhiteSpace(r.Value)))
                {
                    _db.VitalReadings.Add(new VitalReading
                    {
                        UserId = userId,
                        Type = r.Type,
                        Value = r.Value.Trim(),
                        Unit = r.Unit ?? string.Empty,
                        StatusLabel = string.IsNullOrWhiteSpace(r.StatusLabel) ? "Normal" : r.StatusLabel,
                        InRange = r.InRange,
                        RecordedAt = r.RecordedAt ?? DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();
            return MapDevice(device);
        }

        public async Task<MobileDeviceResponse?> ReconnectDeviceAsync(string userId, string deviceId)
        {
            var device = await _db.MobileDevices
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId);
            if (device == null) return null;
            device.Status = "connected";
            device.LastSyncAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return MapDevice(device);
        }

        public async Task<bool> RemoveDeviceAsync(string userId, string deviceId)
        {
            var device = await _db.MobileDevices
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId);
            if (device == null) return false;
            _db.MobileDevices.Remove(device);
            await _db.SaveChangesAsync();
            return true;
        }

        private static MobileDeviceResponse MapDevice(MobileDevice d) => new()
        {
            Id = d.DeviceId,
            Name = d.Name,
            Type = d.Type,
            Status = d.Status,
            LastSyncAt = d.LastSyncAt,
            ExternalId = d.ExternalId
        };
    }
}
