using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.Hubs;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentController(AppDbContext db, IHubContext<NotificationHub> hub) : ControllerBase
{
    public sealed record BookRequest(
        [Required, StringLength(160, MinimumLength = 2)] string Topic,
        [Required] DateTime ScheduledDate,
        [StringLength(4000)] string? Notes);
    public sealed record ChangeRequest(
        [StringLength(24)] string? Status,
        [StringLength(450)] string? AssignedToUserId,
        [StringLength(4000)] string? Notes);
    public sealed record AppointmentDto(string Id, string ClientId, string ClientName,
        string RequestedByUserId, string Topic, DateTime ScheduledDate, string Status,
        string? Notes, string? AssignedToUserId, DateTime CreatedAt);

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? string.Empty;

    [HttpPost]
    [Authorize(Roles = "Client User")]
    public async Task<IActionResult> Book(BookRequest request)
    {
        if (request.ScheduledDate <= DateTime.UtcNow)
            return BadRequest(new { message = "Choose a future appointment date and time." });
        var client = await db.Clients.SingleOrDefaultAsync(c => c.ClientId == UserId);
        if (client == null) return BadRequest(new { message = "Your CRM client account is not set up yet." });
        var appointment = new Appointment
        {
            ClientId = client.ClientId,
            RequestedByUserId = UserId,
            Topic = request.Topic.Trim(),
            ScheduledDate = request.ScheduledDate.ToUniversalTime(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = AppointmentStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        var result = Map(appointment, client.ClientName);
        await NotifyStaff("New Appointment Requested", result);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = from appointment in db.Appointments.AsNoTracking()
                    join client in db.Clients.AsNoTracking() on appointment.ClientId equals client.ClientId
                    select new { appointment, client.ClientName };
        if (User.IsInRole(UserRoles.ClientUser)) query = query.Where(x => x.appointment.RequestedByUserId == UserId);
        else if (User.IsInRole(UserRoles.FieldSupportEngineer)) query = query.Where(x => x.appointment.AssignedToUserId == UserId);
        else if (!User.IsInRole(UserRoles.SystemAdministrator) && !User.IsInRole(UserRoles.SalesRepresentative)) return Forbid();
        var rows = await query.OrderBy(x => x.appointment.ScheduledDate)
            .Select(x => new AppointmentDto(x.appointment.Id, x.appointment.ClientId, x.ClientName,
                x.appointment.RequestedByUserId, x.appointment.Topic, x.appointment.ScheduledDate,
                x.appointment.Status, x.appointment.Notes, x.appointment.AssignedToUserId, x.appointment.CreatedAt))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var appt = await db.Appointments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (appt == null) return NotFound();
        if (User.IsInRole(UserRoles.ClientUser) && appt.RequestedByUserId != UserId) return Forbid();
        if (User.IsInRole(UserRoles.FieldSupportEngineer) && appt.AssignedToUserId != UserId) return Forbid();
        var clientName = await db.Clients.Where(c => c.ClientId == appt.ClientId).Select(c => c.ClientName).FirstOrDefaultAsync() ?? string.Empty;
        return Ok(Map(appt, clientName));
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "System Administrator,Sales Representative")]
    public async Task<IActionResult> Update(string id, ChangeRequest request)
    {
        var appt = await db.Appointments.SingleOrDefaultAsync(x => x.Id == id);
        if (appt == null) return NotFound();
        if (request.Status != null)
        {
            var status = request.Status.Trim();
            if (!AppointmentStatuses.All.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Status must be Pending, Approved, or Declined." });
            appt.Status = AppointmentStatuses.All.Single(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
        }
        if (request.AssignedToUserId != null)
        {
            if (!string.IsNullOrWhiteSpace(request.AssignedToUserId) &&
                !await db.Users.AnyAsync(u => u.UserId == request.AssignedToUserId && u.IsActive))
                return BadRequest(new { message = "The selected representative is not active." });
            appt.AssignedToUserId = string.IsNullOrWhiteSpace(request.AssignedToUserId) ? null : request.AssignedToUserId.Trim();
        }
        if (request.Notes != null) appt.Notes = request.Notes.Trim();
        appt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var clientName = await db.Clients.Where(c => c.ClientId == appt.ClientId).Select(c => c.ClientName).FirstOrDefaultAsync() ?? string.Empty;
        var dto = Map(appt, clientName);
        if (appt.Status == AppointmentStatuses.Approved)
            await hub.Clients.Group("user:" + appt.RequestedByUserId).SendAsync(NotificationHub.NotificationEvent,
                new { title = "Appointment Approved", message = $"Your appointment about {appt.Topic} has been approved.", appointment = dto });
        await NotifyStaff("Appointment Updated", dto);
        return Ok(dto);
    }

    private async Task NotifyStaff(string title, AppointmentDto appointment) =>
        await Task.WhenAll(
            hub.Clients.Group(UserRoles.SystemAdministrator).SendAsync(NotificationHub.NotificationEvent,
                new { title, message = $"{appointment.ClientName}: {appointment.Topic}", appointment }),
            hub.Clients.Group(UserRoles.SalesRepresentative).SendAsync(NotificationHub.NotificationEvent,
                new { title, message = $"{appointment.ClientName}: {appointment.Topic}", appointment }));

    private static AppointmentDto Map(Appointment a, string clientName) => new(a.Id, a.ClientId,
        clientName, a.RequestedByUserId, a.Topic, a.ScheduledDate, a.Status, a.Notes,
        a.AssignedToUserId, a.CreatedAt);
}
