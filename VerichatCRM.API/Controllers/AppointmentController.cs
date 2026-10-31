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
public class AppointmentController(AppDbContext db, IHubContext<NotificationHub> hub, ILogger<AppointmentController> logger) : ControllerBase
{
    public sealed record BookRequest(
        [Required, StringLength(160, MinimumLength = 2)] string Topic,
        [Required] DateTime ScheduledDate,
        [StringLength(4000)] string? Notes);
    public sealed record ChangeRequest(
        [StringLength(24)] string? Status,
        [StringLength(450)] string? AssignedToUserId,
        [StringLength(4000)] string? Notes,
        [StringLength(1000)] string? MeetingUrl);
    public sealed record AppointmentDto(string Id, string ClientId, string ClientName,
        string RequestedByUserId, string Topic, DateTime ScheduledDate, string Status,
        string? Notes, string? AssignedToUserId, DateTime CreatedAt,
        string? ClientEmail, string? ClientPhone, string? MeetingUrl);
    public sealed record AssigneeDto(string UserId, string DisplayName, string Role);

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
        var result = Map(appointment, client);
        try { await NotifyStaff("New Appointment Requested", result); }
        catch (Exception exception) { logger.LogWarning(exception, "Appointment {AppointmentId} was saved, but the staff notification failed.", appointment.Id); }
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = from appointment in db.Appointments.AsNoTracking()
                    join client in db.Clients.AsNoTracking() on appointment.ClientId equals client.ClientId
                    select new { appointment, client };
        if (User.IsInRole(UserRoles.ClientUser)) query = query.Where(x => x.appointment.RequestedByUserId == UserId);
        else if (User.IsInRole(UserRoles.FieldSupportEngineer)) query = query.Where(x => x.appointment.AssignedToUserId == UserId);
        else if (!User.IsInRole(UserRoles.SystemAdministrator) && !User.IsInRole(UserRoles.SalesRepresentative)
                 && !User.IsInRole(UserRoles.ProcurementManager)) return Forbid();
        var rows = await query.OrderBy(x => x.appointment.ScheduledDate)
            .Select(x => new AppointmentDto(x.appointment.Id, x.appointment.ClientId, x.client.ClientName,
                x.appointment.RequestedByUserId, x.appointment.Topic, x.appointment.ScheduledDate,
                x.appointment.Status, x.appointment.Notes, x.appointment.AssignedToUserId, x.appointment.CreatedAt,
                x.client.ContactEmail, x.client.ContactPhone, x.appointment.MeetingUrl))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpGet("assignees")]
    [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager")]
    public async Task<IActionResult> GetAssignees()
    {
        var rows = await db.Users.AsNoTracking()
            .Where(user => user.IsActive &&
                (user.Role == UserRoles.SalesRepresentative || user.Role == UserRoles.FieldSupportEngineer))
            .OrderBy(user => user.DisplayName)
            .Select(user => new AssigneeDto(user.UserId, user.DisplayName, user.Role))
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
        var client = await db.Clients.AsNoTracking().SingleOrDefaultAsync(c => c.ClientId == appt.ClientId);
        return Ok(Map(appt, client));
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager")]
    public async Task<IActionResult> Update(string id, ChangeRequest request)
    {
        var appt = await db.Appointments.SingleOrDefaultAsync(x => x.Id == id);
        if (appt == null) return NotFound();
        if (request.Status != null)
        {
            var status = request.Status.Trim();
            if (!AppointmentStatuses.All.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Status must be Pending, Approved, Declined, or Completed." });
            var normalized = AppointmentStatuses.All.Single(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
            var validTransition = normalized == appt.Status
                || (appt.Status == AppointmentStatuses.Pending && normalized is AppointmentStatuses.Approved or AppointmentStatuses.Declined)
                || (appt.Status == AppointmentStatuses.Approved && normalized == AppointmentStatuses.Completed);
            if (!validTransition)
                return Conflict(new { message = "Appointments can move from Pending to Approved or Declined, then from Approved to Completed." });
            appt.Status = normalized;
        }
        if (request.AssignedToUserId != null)
        {
            if (!string.IsNullOrWhiteSpace(request.AssignedToUserId) &&
                !await db.Users.AnyAsync(u => u.UserId == request.AssignedToUserId && u.IsActive))
                return BadRequest(new { message = "The selected representative is not active." });
            appt.AssignedToUserId = string.IsNullOrWhiteSpace(request.AssignedToUserId) ? null : request.AssignedToUserId.Trim();
        }
        if (request.Notes != null) appt.Notes = request.Notes.Trim();
        if (request.MeetingUrl != null)
        {
            var meetingUrl = request.MeetingUrl.Trim();
            if (meetingUrl.Length > 0 && (!Uri.TryCreate(meetingUrl, UriKind.Absolute, out var parsedUrl)
                || parsedUrl.Scheme is not ("https" or "http")))
                return BadRequest(new { message = "Meeting link must be a valid HTTP or HTTPS URL." });
            appt.MeetingUrl = meetingUrl.Length == 0 ? null : meetingUrl;
        }
        appt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var client = await db.Clients.AsNoTracking().SingleOrDefaultAsync(c => c.ClientId == appt.ClientId);
        var dto = Map(appt, client);
        try
        {
            if (appt.Status == AppointmentStatuses.Approved || appt.Status == AppointmentStatuses.Completed)
                await hub.Clients.Group("user:" + appt.RequestedByUserId).SendAsync(NotificationHub.NotificationEvent,
                    new { title = $"Appointment {appt.Status}", message = $"Your appointment about {appt.Topic} is {appt.Status.ToLowerInvariant()}.", appointment = dto });
            if (!string.IsNullOrWhiteSpace(appt.AssignedToUserId))
                await hub.Clients.Group("user:" + appt.AssignedToUserId).SendAsync(NotificationHub.NotificationEvent,
                    new { title = "Appointment Assigned", message = $"You have been assigned to {dto.ClientName}: {appt.Topic}.", appointment = dto });
            await NotifyStaff("Appointment Updated", dto);
        }
        catch (Exception exception)
        {
            // Do not report a saved appointment change as failed just because a real-time socket is down.
            logger.LogWarning(exception, "Appointment {AppointmentId} was updated, but a SignalR notification failed.", id);
        }
        // A rep-provided link is the client-facing call invitation; the API does not provision a meeting provider.
        if (!string.IsNullOrWhiteSpace(appt.MeetingUrl))
            await hub.Clients.Group("user:" + appt.RequestedByUserId).SendAsync(NotificationHub.NotificationEvent,
                new { title = "Consultation meeting ready", message = $"A meeting link is ready for {appt.Topic}.", appointment = dto });
        return Ok(dto);
    }

    private async Task NotifyStaff(string title, AppointmentDto appointment) =>
        await Task.WhenAll(
            hub.Clients.Group(UserRoles.SystemAdministrator).SendAsync(NotificationHub.NotificationEvent,
                new { title, message = $"{appointment.ClientName}: {appointment.Topic}", appointment }),
            hub.Clients.Group(UserRoles.SalesRepresentative).SendAsync(NotificationHub.NotificationEvent,
                new { title, message = $"{appointment.ClientName}: {appointment.Topic}", appointment }),
            hub.Clients.Group(UserRoles.ProcurementManager).SendAsync(NotificationHub.NotificationEvent,
                new { title, message = $"{appointment.ClientName}: {appointment.Topic}", appointment }));

    private static AppointmentDto Map(Appointment a, Client? client) => new(a.Id, a.ClientId,
        client?.ClientName ?? string.Empty, a.RequestedByUserId, a.Topic, a.ScheduledDate, a.Status, a.Notes,
        a.AssignedToUserId, a.CreatedAt, client?.ContactEmail, client?.ContactPhone, a.MeetingUrl);
}
