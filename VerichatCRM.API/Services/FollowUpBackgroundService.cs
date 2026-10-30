using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class FollowUpBackgroundService : BackgroundService
    {
        private static readonly TimeSpan DueSoonWindow = TimeSpan.FromDays(7);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FollowUpBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public FollowUpBackgroundService(IServiceScopeFactory scopeFactory, ILogger<FollowUpBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Follow-up background service started. Interval: {Interval}", _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;
                    var dueSoonThreshold = now.Add(DueSoonWindow);

                    var overdueInquiries = await db.Inquiries
                        .Where(i => i.NextFollowUpDate.HasValue
                                    && i.NextFollowUpDate.Value < now
                                    && i.FollowUpStatus != InquiryFollowUpStatuses.Overdue
                                    && i.FollowUpStatus != InquiryFollowUpStatuses.Completed)
                        .ToListAsync(stoppingToken);

                    foreach (var inquiry in overdueInquiries)
                    {
                        inquiry.FollowUpStatus = InquiryFollowUpStatuses.Overdue;
                        inquiry.UpdatedAt = now;
                    }

                    var calibrationDueSoon = await db.CalibrationSchedules
                        .Where(s => s.Status == CalibrationScheduleStatuses.Pending
                                    && s.ScheduledDate >= now
                                    && s.ScheduledDate <= dueSoonThreshold)
                        .Join(db.Clients.Where(c => c.RequiresCalibration),
                            schedule => schedule.ClientId,
                            client => client.ClientId,
                            (schedule, client) => new
                            {
                                schedule.ScheduleId,
                                schedule.ClientId,
                                schedule.EquipmentId,
                                schedule.ScheduledDate
                            })
                        .ToListAsync(stoppingToken);

                    if (overdueInquiries.Count > 0)
                    {
                        _logger.LogWarning("Follow-up scan flagged {Count} overdue inquiries.", overdueInquiries.Count);
                    }

                    if (calibrationDueSoon.Count > 0)
                    {
                        _logger.LogWarning("Calibration scan flagged {Count} schedules due within 7 days.", calibrationDueSoon.Count);
                    }

                    if (overdueInquiries.Count > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Follow-up background scan failed.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
