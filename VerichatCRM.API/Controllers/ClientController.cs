using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using VerichatCRM.API.Hubs;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clients;
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        public ClientController(IClientService clients, AppDbContext db, IHubContext<NotificationHub> hub) { _clients = clients; _db = db; _hub = hub; }

        public sealed record CreateTimelineNoteRequest([Required, StringLength(4000, MinimumLength = 1)] string Text);
        private sealed record ClientTimelineItem(string Id, string Type, string Title, string Text, DateTime CreatedAt, string CreatedByUserId);

        [HttpGet("{clientId}/timeline")]
        public async Task<IActionResult> GetTimeline(string clientId)
        {
            if (User.IsInRole(UserRoles.ClientUser) && clientId != CurrentUserId()) return Forbid();
            if (!await _db.Clients.AnyAsync(client => client.ClientId == clientId)) return NotFound();

            var items = await _db.Tickets.AsNoTracking().Where(ticket => ticket.ClientId == clientId)
                .Select(ticket => new ClientTimelineItem(ticket.TicketId, "ticket", "Ticket created", ticket.Title, ticket.CreatedAt, ticket.CreatedByUserId)).ToListAsync();
            var activities = await (from activity in _db.Set<TicketActivity>().AsNoTracking()
                                    join ticket in _db.Tickets.AsNoTracking() on activity.TicketId equals ticket.TicketId
                                    where ticket.ClientId == clientId
                                    select new ClientTimelineItem(activity.ActivityId, "ticket", activity.Action,
                                        activity.Notes ?? (activity.ToValue == null ? ticket.Title : ticket.Title + ": " + activity.ToValue),
                                        activity.PerformedAt, activity.PerformedByUserId)).ToListAsync();
            var inquiries = await _db.Inquiries.AsNoTracking().Where(inquiry => inquiry.ClientId == clientId)
                .Select(inquiry => new ClientTimelineItem(inquiry.InquiryId, "inquiry", "Inquiry " + inquiry.Status,
                    inquiry.Subject + (inquiry.Description == null ? string.Empty : ": " + inquiry.Description),
                    inquiry.CreatedAt, inquiry.OwnerUserId ?? string.Empty)).ToListAsync();
            var notes = await _db.ClientTimelineNotes.AsNoTracking().Where(note => note.ClientId == clientId)
                .Select(note => new ClientTimelineItem(note.NoteId, "note", "Note", note.Text, note.CreatedAt, note.CreatedByUserId)).ToListAsync();
            var appointments = await _db.Appointments.AsNoTracking().Where(appointment => appointment.ClientId == clientId)
                .Select(appointment => new ClientTimelineItem(appointment.Id, "appointment", "Appointment " + appointment.Status,
                    appointment.Topic, appointment.CreatedAt, appointment.RequestedByUserId)).ToListAsync();
            var quotations = await _db.Quotations.AsNoTracking().Where(quotation => quotation.ClientId == clientId)
                .Select(quotation => new ClientTimelineItem(quotation.QuotationId, "quotation", "Quote " + quotation.Status,
                    quotation.QuoteReference + " · " + quotation.GrandTotal, quotation.CreatedAt, string.Empty)).ToListAsync();

            return Ok(items.Concat(activities).Concat(inquiries).Concat(notes).Concat(appointments).Concat(quotations).OrderBy(item => item.CreatedAt));
        }

        [HttpPost("{clientId}/notes")]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Field Support Engineer")]
        public async Task<IActionResult> AddTimelineNote(string clientId, [FromBody] CreateTimelineNoteRequest request)
        {
            if (!await _db.Clients.AnyAsync(client => client.ClientId == clientId)) return NotFound();
            var text = request.Text.Trim();
            if (text.Length == 0) return BadRequest("Note text cannot be empty.");
            var note = new ClientTimelineNote
            {
                ClientId = clientId,
                Text = text,
                CreatedByUserId = CurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };
            _db.ClientTimelineNotes.Add(note);
            await _db.SaveChangesAsync();
            await _hub.Clients.Group("user:" + clientId).SendAsync(NotificationHub.NotificationEvent,
                new { title = "Timeline Updated", message = "A new note was added to your client timeline.", clientId });
            return CreatedAtAction(nameof(GetTimeline), new { clientId }, note);
        }

        [HttpGet]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Field Support Engineer")]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? sector) =>
            Ok(await _clients.GetAllAsync(search, sector));

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetById(string clientId)
        {
            if (User.IsInRole(UserRoles.ClientUser) && clientId != CurrentUserId()) return Forbid();
            var c = await _clients.GetByIdAsync(clientId);
            return c == null ? NotFound() : Ok(c);
        }

        private string CurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value ?? string.Empty;

        [HttpPost]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
        {
            var c = await _clients.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { clientId = c.ClientId }, c);
        }

        [HttpPut("{clientId}")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Update(string clientId, [FromBody] CreateClientRequest request)
        {
            var c = await _clients.UpdateAsync(clientId, request);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpDelete("{clientId}")]
        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> Delete(string clientId)
        {
            await _clients.DeleteAsync(clientId);
            return NoContent();
        }
    }
}
