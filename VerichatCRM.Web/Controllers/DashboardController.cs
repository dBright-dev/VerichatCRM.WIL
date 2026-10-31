using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers;

// The session check inside Index only asks whether a token exists, not who holds
// it, so on its own it let a signed-in Client User or Medical Student open this
// staff screen. The permission filter is what decides access here.
[RequirePermission(Perm.Workspace)]
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
