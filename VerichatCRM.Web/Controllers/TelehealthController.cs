using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Telehealth;
using VerichatCRM.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace VerichatCRM.Web.Controllers;

public class TelehealthController(IApiClient api) : Controller
{
    [RequirePermission(Perm.AppointmentManagement)]
    public async Task<IActionResult> Appointments()
    {
        if (HttpContext.Session.GetString("JwtToken") == null)
            return RedirectToAction("Login", "Account");

        ViewData["Title"] = "Client Appointments";
        ViewData["Subtitle"] = "Consultation and field service requests from the Verichat mobile app.";
        ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
        var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        ViewBag.CanManageAppointments = role is Roles.SystemAdministrator or Roles.SalesRepresentative or Roles.ProcurementManager;
        ViewBag.ApiBaseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiBaseUrl"]?.TrimEnd('/');
        ViewBag.ApiToken = JsonSerializer.Serialize(HttpContext.Session.GetString("JwtToken") ?? string.Empty);
        var appointments = await api.GetAsync<List<TelehealthAppointmentViewModel>>("/api/appointments") ?? new();
        ViewBag.Assignees = (bool)ViewBag.CanManageAppointments
            ? await api.GetAsync<List<TelehealthAssigneeViewModel>>("/api/appointments/assignees") ?? new()
            : new List<TelehealthAssigneeViewModel>();
        return View("~/Views/Sales/Appointments.cshtml", appointments);
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission(Perm.AppointmentManagement)]
    public async Task<IActionResult> Update(string id, string status, string? assignedToUserId, string? meetingUrl)
    {
        if (HttpContext.Session.GetString("JwtToken") == null) return RedirectToAction("Login", "Account");
        var updated = await api.PatchAsync<object, TelehealthAppointmentViewModel>($"/api/appointments/{Uri.EscapeDataString(id)}", new { status, assignedToUserId, meetingUrl });
        TempData["AppointmentMessage"] = updated == null ? "The appointment could not be updated." : "Appointment updated.";
        return RedirectToAction(nameof(Appointments));
    }
}
