using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Tests;

/// <summary>
/// Only one of this service's operations belongs to sign-up: the client row an
/// ordering role needs. The rest of the interface is the mobile portal's data,
/// which an auth test never reaches, so those members refuse rather than pretend
/// - a test that starts needing one has grown past this file.
/// </summary>
internal sealed class FakeMobileService : IMobileService
{
    public List<string> ClientAccountIdsCreated { get; } = [];

    public Task EnsureClientAccountAsync(string userId)
    {
        ClientAccountIdsCreated.Add(userId);
        return Task.CompletedTask;
    }

    public Task<ClientDashboardResponse> GetDashboardAsync(string userId) => NotReached<ClientDashboardResponse>();
    public Task<IEnumerable<MobileDeviceResponse>> GetDevicesAsync(string userId) => NotReached<IEnumerable<MobileDeviceResponse>>();
    public Task<MobileDeviceResponse?> RegisterDeviceAsync(string userId, string name, string type, string? externalId) => NotReached<MobileDeviceResponse?>();
    public Task<MobileDeviceResponse?> SyncDeviceAsync(string userId, string deviceId, IReadOnlyList<DeviceSyncReadingDto>? readings) => NotReached<MobileDeviceResponse?>();
    public Task<MobileDeviceResponse?> ReconnectDeviceAsync(string userId, string deviceId) => NotReached<MobileDeviceResponse?>();
    public Task<bool> RemoveDeviceAsync(string userId, string deviceId) => NotReached<bool>();
    public Task<WellnessSummaryResponse> GetWellnessSummaryAsync(string userId) => NotReached<WellnessSummaryResponse>();
    public Task<HealthMetricResponse> LogVitalAsync(string userId, string type, string value, string unit, string statusLabel, bool inRange, DateTime? recordedAt) => NotReached<HealthMetricResponse>();
    public Task<GoalsResponse> GetGoalsAsync(string userId) => NotReached<GoalsResponse>();
    public Task<GoalItemResponse?> UpdateGoalAsync(string userId, string goalId, double? current, double? target, string? name, string? unit, string? type) => NotReached<GoalItemResponse?>();
    public Task<WeeklyExerciseResponse> LogExerciseAsync(string userId, int dayIndex, int minutes) => NotReached<WeeklyExerciseResponse>();
    public Task<IEnumerable<TelehealthAppointmentResponse>> GetAppointmentsAsync(string userId) => NotReached<IEnumerable<TelehealthAppointmentResponse>>();
    public Task<TelehealthAppointmentResponse> BookAppointmentAsync(string userId, string doctorName, string specialty, string facility, DateTime startsAt) => NotReached<TelehealthAppointmentResponse>();
    public Task<LabResultsMobileResponse> GetLabResultsAsync(string userId) => NotReached<LabResultsMobileResponse>();
    public Task<bool> MarkLabResultViewedAsync(string userId, string labResultId) => NotReached<bool>();
    public Task<IEnumerable<MobilePrescriptionResponse>> GetPrescriptionsAsync(string userId) => NotReached<IEnumerable<MobilePrescriptionResponse>>();
    public Task<MobileProfileResponse> GetProfileAsync(string userId) => NotReached<MobileProfileResponse>();
    public Task<MobileProfileResponse> UpdateProfileAsync(string userId, Action<User> updateUser, Action<MobileProfile> updateProfile) => NotReached<MobileProfileResponse>();
    public Task<MobileSettingsResponse> GetSettingsAsync(string userId) => NotReached<MobileSettingsResponse>();
    public Task<MobileSettingsResponse> UpdateSettingsAsync(string userId, bool? push, bool? biometric, bool? dark) => NotReached<MobileSettingsResponse>();
    public Task<LogHealthMetricResponse> LogHealthMetricAsync(string userId, LogHealthMetricRequest request) => NotReached<LogHealthMetricResponse>();

    private static Task<T> NotReached<T>() =>
        throw new InvalidOperationException("A test reached a mobile-portal operation; this fake only carries sign-up.");
}
