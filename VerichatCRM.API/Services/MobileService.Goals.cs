using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services
{
    public partial class MobileService
    {
        public async Task<GoalItemResponse?> UpdateGoalAsync(
            string userId, string goalId, double? current, double? target,
            string? name, string? unit, string? type)
        {
            var goal = await _db.WellnessGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);
            if (goal == null) return null;

            if (current.HasValue) goal.Current = current.Value;
            if (target.HasValue) goal.Target = target.Value;
            if (!string.IsNullOrWhiteSpace(name)) goal.Name = name;
            if (!string.IsNullOrWhiteSpace(unit)) goal.Unit = unit;
            if (!string.IsNullOrWhiteSpace(type)) goal.Type = type;
            goal.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new GoalItemResponse
            {
                Id = goal.GoalId,
                Type = goal.Type,
                Name = goal.Name,
                Current = goal.Current,
                Target = goal.Target,
                Unit = goal.Unit
            };
        }

        public async Task<WeeklyExerciseResponse> LogExerciseAsync(string userId, int dayIndex, int minutes)
        {
            var weekStart = WeekStartUtc(DateTime.UtcNow);
            var log = await _db.WeeklyExerciseLogs.FirstOrDefaultAsync(
                l => l.UserId == userId && l.WeekStart == weekStart && l.DayIndex == dayIndex);
            if (log == null)
            {
                log = new WeeklyExerciseLog { UserId = userId, WeekStart = weekStart, DayIndex = dayIndex };
                _db.WeeklyExerciseLogs.Add(log);
            }
            log.Minutes = minutes;
            await _db.SaveChangesAsync();

            var logs = await _db.WeeklyExerciseLogs
                .Where(l => l.UserId == userId && l.WeekStart == weekStart)
                .ToListAsync();
            return new WeeklyExerciseResponse
            {
                TargetPerDayMin = 30,
                Days = WeeklyLabels.Select((label, i) => new WeeklyExerciseDayResponse
                {
                    Label = label,
                    Minutes = logs.FirstOrDefault(l => l.DayIndex == i)?.Minutes ?? 0
                }).ToList(),
                TodayIndex = ((int)DateTime.UtcNow.DayOfWeek + 6) % 7
            };
        }
    }
}
