using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Controllers
{
    /// <summary>Appointments, lab results, e-prescriptions for the mobile app.</summary>
    [ApiController]
    [Route("api/mobile")]
    [Authorize]
    public class MobileTelehealthController : ControllerBase
    {
        private readonly IMobileService _mobile;
        private readonly AppDbContext _db;

        public MobileTelehealthController(IMobileService mobile, AppDbContext db) { _mobile = mobile; _db = db; }

        private string UserId =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        [HttpGet("appointments")]
        public async Task<IActionResult> Appointments() =>
            Ok(await _mobile.GetAppointmentsAsync(UserId));

        [HttpPost("appointments")]
        public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request) =>
            Ok(await _mobile.BookAppointmentAsync(UserId, request.DoctorName,
                request.Specialty, request.Facility, request.StartsAt));

        [HttpGet("appointments/all")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> StaffAppointments()
        {
            var appointments = await (from appointment in _db.TelehealthAppointments.AsNoTracking()
                                      join client in _db.Clients.AsNoTracking() on appointment.UserId equals client.ClientId into clientGroup
                                      from client in clientGroup.DefaultIfEmpty()
                                      orderby appointment.StartsAt descending
                                      select new StaffTelehealthAppointmentResponse
                                      {
                                          Id = appointment.AppointmentId,
                                          ClientUserId = appointment.UserId,
                                          ClientName = client == null ? appointment.UserId : client.ClientName,
                                          DoctorName = appointment.DoctorName,
                                          Specialty = appointment.Specialty,
                                          Facility = appointment.Facility,
                                          StartsAt = appointment.StartsAt,
                                          Status = appointment.Status,
                                          CreatedAt = appointment.CreatedAt
                                      }).ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("lab-results")]
        public async Task<IActionResult> LabResults() =>
            Ok(await _mobile.GetLabResultsAsync(UserId));

        [HttpPost("lab-results/{id}/viewed")]
        public async Task<IActionResult> MarkViewed(string id)
        {
            var ok = await _mobile.MarkLabResultViewedAsync(UserId, id);
            return ok ? Ok(new { viewed = true }) : NotFound(new { message = "Lab result not found." });
        }

        [HttpGet("prescriptions")]
        public async Task<IActionResult> Prescriptions() =>
            Ok(await _mobile.GetPrescriptionsAsync(UserId));
    }
}
