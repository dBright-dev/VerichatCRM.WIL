using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    /// <summary>Vital readings + wellness summary (home-care devices).</summary>
    [ApiController]
    [Route("api/mobile")]
    [Authorize]
    public class MobileVitalsController : ControllerBase
    {
        private readonly IMobileService _mobile;

        public MobileVitalsController(IMobileService mobile) => _mobile = mobile;

        private string UserId =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        [HttpGet("vitals")]
        public async Task<IActionResult> Summary() =>
            Ok(await _mobile.GetWellnessSummaryAsync(UserId));

        [HttpPost("vitals")]
        public async Task<IActionResult> LogVital([FromBody] LogVitalRequest request) =>
            Ok(await _mobile.LogVitalAsync(UserId, request.Type, request.Value,
                request.Unit, request.StatusLabel, request.InRange, request.RecordedAt));

        [HttpGet("goals")]
        public async Task<IActionResult> Goals() =>
            Ok(await _mobile.GetGoalsAsync(UserId));

        [HttpPatch("goals/{goalId}")]
        public async Task<IActionResult> UpdateGoal(string goalId, [FromBody] UpdateGoalRequest request)
        {
            var goal = await _mobile.UpdateGoalAsync(UserId, goalId,
                request.Current, request.Target, request.Name, request.Unit, request.Type);
            return goal == null ? NotFound(new { message = "Goal not found." }) : Ok(goal);
        }

        [HttpPost("goals/exercise")]
        public async Task<IActionResult> LogExercise([FromBody] LogExerciseRequest request) =>
            Ok(await _mobile.LogExerciseAsync(UserId, request.DayIndex, request.Minutes));
    }
}
