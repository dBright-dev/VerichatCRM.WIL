using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Controllers;

[ApiController]
[Route("api/salesgoals")]
[Authorize]
public class SalesGoalsController(AppDbContext db) : ControllerBase
{
    public sealed record GoalResponse(string Id, string Title, decimal Target, decimal Achieved, string Period, string PeriodLabel);
    public sealed record CreateGoalRequest([Required, StringLength(450)] string UserId,
        [Required, StringLength(160)] string Title, [Range(typeof(decimal), "0.01", "999999999999")] decimal Target,
        [Range(typeof(decimal), "0", "999999999999")] decimal Achieved, [Required] string Period, string? PeriodLabel);
    public sealed record ProgressRequest([Range(typeof(decimal), "0", "999999999999")] decimal Achieved);

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
    private IQueryable<SalesGoal> Mine => db.SalesGoals.AsNoTracking().Where(goal => goal.UserId == CurrentUserId);

    [HttpGet("my-goals")]
    public async Task<IActionResult> MyGoals() => Ok(await Mine.OrderBy(goal => goal.Period).Select(goal => Map(goal)).ToListAsync());

    [HttpGet]
    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> All() => Ok(await db.SalesGoals.AsNoTracking().OrderBy(goal => goal.UserId).Select(goal => Map(goal)).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> Create(CreateGoalRequest request)
    {
        if (!await db.Users.AnyAsync(user => user.UserId == request.UserId)) return BadRequest(new { message = "The target user was not found." });
        var period = request.Period.Trim().ToLowerInvariant();
        if (period is not ("monthly" or "quarterly" or "yearly")) return BadRequest(new { message = "Period must be monthly, quarterly, or yearly." });
        var goal = new SalesGoal { UserId = request.UserId, Title = request.Title.Trim(), Target = request.Target,
            Achieved = request.Achieved, Period = period, PeriodLabel = string.IsNullOrWhiteSpace(request.PeriodLabel) ? period : request.PeriodLabel.Trim() };
        db.SalesGoals.Add(goal); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(MyGoals), new { id = goal.Id }, Map(goal));
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(string id, ProgressRequest request)
    {
        var goal = await db.SalesGoals.SingleOrDefaultAsync(x => x.Id == id);
        if (goal == null) return NotFound();
        if (!User.IsInRole(UserRoles.SystemAdministrator) && goal.UserId != CurrentUserId) return Forbid();
        goal.Achieved = request.Achieved; goal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(Map(goal));
    }

    private static GoalResponse Map(SalesGoal goal) => new(goal.Id, goal.Title, goal.Target, goal.Achieved, goal.Period, goal.PeriodLabel);
}
