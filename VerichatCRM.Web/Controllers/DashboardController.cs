using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers;

public class DashboardController(IApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("JwtToken") == null) return RedirectToAction("Login", "Account");
        var model = await api.GetAsync<DashboardAnalyticsResponse>("/api/dashboard/analytics")
            ?? new DashboardAnalyticsResponse { RoleLabel = HttpContext.Session.GetString("UserRole") ?? "Workspace" };
        ViewData["Title"] = "Dashboard";
        ViewData["Subtitle"] = $"{model.RoleLabel} overview — {DateTime.Now:dddd, d MMMM yyyy}";
        return View(model);
    }
}
