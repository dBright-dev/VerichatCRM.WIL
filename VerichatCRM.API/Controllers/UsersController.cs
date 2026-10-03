using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/auth/users")]
    [Authorize(Roles = "System Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _users;

        public UsersController(IUserRepository users) => _users = users;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromServices] VerichatCRM.Infrastructure.Data.AppDbContext db)
        {
            var users = await System.Threading.Tasks.Task.Run(() =>
                db.Users.OrderByDescending(u => u.CreatedAt)
                    .Select(u => new { u.UserId, u.DisplayName, u.Email, u.Role, u.IsActive, u.CreatedAt, u.LastLoginAt })
                    .ToList());
            return Ok(users);
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleRequest request)
        {
            if (!UserRoles.All.Contains(request.Role))
                return BadRequest(new { message = "Invalid role specified." });

            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            // Only a System Administrator may create another System Administrator —
            // enforced here because this whole controller already requires that role.
            user.Role = request.Role;
            await _users.UpdateAsync(user);
            return Ok(new { message = "Role updated." });
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });
            user.IsActive = false;
            await _users.UpdateAsync(user);
            return Ok(new { message = "User deactivated." });
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(string id)
        {
            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });
            user.IsActive = true;
            await _users.UpdateAsync(user);
            return Ok(new { message = "User activated." });
        }
    }

    public class UpdateRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
