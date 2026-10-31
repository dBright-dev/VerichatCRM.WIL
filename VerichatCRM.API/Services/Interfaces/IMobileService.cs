using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IMobileService
    {
        Task<ClientDashboardResponse> GetDashboardAsync(string userId);
        Task<IEnumerable<MobileDeviceResponse>> GetDevicesAsync(string userId);
        Task<MobileDeviceResponse?> RegisterDeviceAsync(string userId, string name, string type, string? externalId);
        Task<MobileDeviceResponse?> SyncDeviceAsync(string userId, string deviceId, IReadOnlyList<DeviceSyncReadingDto>? readings);
        Task<MobileDeviceResponse?> ReconnectDeviceAsync(string userId, string deviceId);
        Task<bool> RemoveDeviceAsync(string userId, string deviceId);
        Task<WellnessSummaryResponse> GetWellnessSummaryAsync(string userId);
        Task<HealthMetricResponse> LogVitalAsync(string userId, string type, string value, string unit, string statusLabel, bool inRange, DateTime? recordedAt);
        Task<GoalsResponse> GetGoalsAsync(string userId);
        Task<GoalItemResponse?> UpdateGoalAsync(string userId, string goalId, double? current, double? target, string? name, string? unit, string? type);
        Task<WeeklyExerciseResponse> LogExerciseAsync(string userId, int dayIndex, int minutes);
        Task<IEnumerable<TelehealthAppointmentResponse>> GetAppointmentsAsync(string userId);
        Task<TelehealthAppointmentResponse> BookAppointmentAsync(string userId, string doctorName, string specialty, string facility, DateTime startsAt);
        Task<LabResultsMobileResponse> GetLabResultsAsync(string userId);
        Task<bool> MarkLabResultViewedAsync(string userId, string labResultId);
        Task<IEnumerable<MobilePrescriptionResponse>> GetPrescriptionsAsync(string userId);
        Task<MobileProfileResponse> GetProfileAsync(string userId);
        Task<MobileProfileResponse> UpdateProfileAsync(string userId, Action<User> updateUser, Action<MobileProfile> updateProfile);
        Task<MobileSettingsResponse> GetSettingsAsync(string userId);
        Task<MobileSettingsResponse> UpdateSettingsAsync(string userId, bool? push, bool? biometric, bool? dark);
        Task EnsureClientAccountAsync(string userId);

        /// <summary>Persists a health metric reading posted from a paired IoT device.</summary>
        Task<LogHealthMetricResponse> LogHealthMetricAsync(string userId, LogHealthMetricRequest request);
    }
}
