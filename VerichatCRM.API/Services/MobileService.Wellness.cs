using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services
{
    public partial class MobileService
    {
        public async Task<WellnessSummaryResponse> GetWellnessSummaryAsync(string userId)
        {
            var defs = new[]
            {
                new { Type = "bloodPressure", Label = "Blood Pressure", Unit = "mmHg" },
                new { Type = "heartRate", Label = "Heart Rate", Unit = "bpm" },
                new { Type = "bloodGlucose", Label = "Blood Glucose", Unit = "mmol/L" },
                new { Type = "temperature", Label = "Temperature", Unit = "°C" },
                new { Type = "bodyWeight", Label = "Body Weight", Unit = "kg" },
            };

            var metrics = new List<HealthMetricResponse>();
            foreach (var m in defs)
            {
                var latest = await _db.VitalReadings
                    .Where(v => v.UserId == userId && v.Type == m.Type)
                    .OrderByDescending(v => v.RecordedAt)
                    .FirstOrDefaultAsync();
                metrics.Add(new HealthMetricResponse
                {
                    Id = latest?.ReadingId ?? m.Type,
                    Type = m.Type,
                    Label = m.Label,
                    Value = latest?.Value,
                    Unit = m.Unit,
                    StatusLabel = latest?.StatusLabel ?? "No data",
                    InRange = latest?.InRange ?? true
                });
            }

            var inRange = metrics.Count(m => m.InRange);
            return new WellnessSummaryResponse
            {
                HealthScore = metrics.Any(m => m.Value != null)
                    ? (int)Math.Round(inRange * 100.0 / metrics.Count)
                    : null,
                Metrics = metrics
            };
        }

        public async Task<HealthMetricResponse> LogVitalAsync(
            string userId, string type, string value, string unit,
            string statusLabel, bool inRange, DateTime? recordedAt)
        {
            var reading = new VitalReading
            {
                UserId = userId,
                Type = type,
                Value = value.Trim(),
                Unit = unit ?? string.Empty,
                StatusLabel = string.IsNullOrWhiteSpace(statusLabel) ? "Normal" : statusLabel,
                InRange = inRange,
                RecordedAt = recordedAt ?? DateTime.UtcNow
            };
            _db.VitalReadings.Add(reading);
            await _db.SaveChangesAsync();
            return new HealthMetricResponse
            {
                Id = reading.ReadingId,
                Type = reading.Type,
                Label = type,
                Value = reading.Value,
                Unit = reading.Unit,
                StatusLabel = reading.StatusLabel,
                InRange = reading.InRange
            };
        }

        public async Task<GoalsResponse> GetGoalsAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            var goals = await _db.WellnessGoals
                .Where(g => g.UserId == userId && g.Date == today)
                .ToListAsync();

            if (goals.Count == 0)
            {
                goals = KnownGoalTypes.Select(t => new WellnessGoal
                {
                    UserId = userId,
                    Type = t,
                    Name = DefaultGoalName(t),
                    Target = DefaultGoalTarget(t),
                    Unit = DefaultGoalUnit(t),
                    Date = today
                }).ToList();
                _db.WellnessGoals.AddRange(goals);
                await _db.SaveChangesAsync();
            }

            var weekStart = WeekStartUtc(DateTime.UtcNow);
            var logs = await _db.WeeklyExerciseLogs
                .Where(l => l.UserId == userId && l.WeekStart == weekStart)
                .ToListAsync();

            int todayIdx = ((int)DateTime.UtcNow.DayOfWeek + 6) % 7;
            return new GoalsResponse
            {
                Goals = goals.Select(g => new GoalItemResponse
                {
                    Id = g.GoalId,
                    Type = g.Type,
                    Name = g.Name,
                    Current = g.Current,
                    Target = g.Target,
                    Unit = g.Unit
                }).ToList(),
                WeeklyExercise = new WeeklyExerciseResponse
                {
                    TargetPerDayMin = 30,
                    Days = WeeklyLabels.Select((label, i) => new WeeklyExerciseDayResponse
                    {
                        Label = label,
                        Minutes = logs.FirstOrDefault(l => l.DayIndex == i)?.Minutes ?? 0
                    }).ToList(),
                    TodayIndex = (DateTime.UtcNow - weekStart).TotalDays < 7 ? todayIdx : null
                }
            };
        }
    }
}
