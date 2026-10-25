using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Telehealth;
using VerichatCRM.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace VerichatCRM.Web.Controllers;

public class TelehealthController(IApiClient api) : Controller
{
    [RequirePermission(Perm.SalesDashboard)]
    public async Task<IActionResult> Appointments()
    {
        if (HttpContext.Session.GetString("JwtToken") == null)
            return RedirectToAction("Login", "Account");

        ViewData["Title"] = "Client Appointments";
        ViewData["Subtitle"] = "Consultation and field service requests from the Verichat mobile app.";
        ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
        ViewBag.ApiBaseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiBaseUrl"]?.TrimEnd('/');
        ViewBag.ApiToken = JsonSerializer.Serialize(HttpContext.Session.GetString("JwtToken") ?? string.Empty);
        var appointments = await api.GetAsync<List<TelehealthAppointmentViewModel>>("/api/appointments") ?? new();
        return View("~/Views/Sales/Appointments.cshtml", appointments);
    }

    [HttpPost, ValidateAntiForgeryToken, RequirePermission(Perm.SalesDashboard)]
    public async Task<IActionResult> Update(string id, string status, string? assignedToUserId)
    {
        if (HttpContext.Session.GetString("JwtToken") == null) return RedirectToAction("Login", "Account");
        var updated = await api.PatchAsync<object, TelehealthAppointmentViewModel>($"/api/appointments/{Uri.EscapeDataString(id)}", new { status, assignedToUserId });
        TempData["AppointmentMessage"] = updated == null ? "The appointment could not be updated." : "Appointment updated.";
        return RedirectToAction(nameof(Appointments));
    }
}
