using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.ViewModels.Admin;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminApiService _admin;
        private readonly IApiClient _api;

        public AdminController(
            IAdminApiService admin, IApiClient api)
        {
            _admin = admin;
            _api = api;
        }

        private bool IsAdmin()
        {
            if (HttpContext.Session.GetString("JwtToken") == null) return false;
            var role = HttpContext.Session.GetString("UserRole");
            return role == "System Administrator";
        }

        [RequirePermission(Perm.AdminDashboard)]
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Account");
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var dashboard = await _api.GetAsync<VerichatCRM.Core.Models.Dashboard.DashboardAnalyticsResponse>("/api/dashboard/analytics")
                ?? new VerichatCRM.Core.Models.Dashboard.DashboardAnalyticsResponse();
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = $"System Administrator overview — {DateTime.Now:dddd, d MMMM yyyy}";
            return View("~/Views/Dashboard/Index.cshtml", dashboard);
        }

        [RequirePermission(Perm.AdminAnalytics)]
        public async Task<IActionResult> Analytics()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Analytics";
            ViewData["Subtitle"] = "Live year-to-date performance from CRM records";
            var analytics = await _api.GetAsync<VerichatCRM.Core.Models.Dashboard.DashboardAnalyticsResponse>("/api/dashboard/analytics")
                ?? new VerichatCRM.Core.Models.Dashboard.DashboardAnalyticsResponse();
            return View(analytics);
        }

        [RequirePermission(Perm.AdminRules)]
        public IActionResult Rules()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Rules Engine";
            ViewData["Subtitle"] = "Security rules and automation";
            return View();
        }

        [RequirePermission(Perm.RoleAssignments)]
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Users & Roles";
            ViewData["Subtitle"] = "View, create, edit, deactivate all users";
            var users = await _admin.GetUsersAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.RoleAssignments)]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await _admin.UpdateUserRoleAsync(userId, role);
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.RoleAssignments)]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await _admin.DeactivateUserAsync(userId);
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.WarehouseAlerts)]
        public async Task<IActionResult> RunSlaCheck()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await _admin.RunSlaCheckAsync();
            TempData["SuccessMessage"] = "SLA escalation check triggered.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.WarehouseAlerts)]
        public async Task<IActionResult> ResolveAlert(string alertId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            await _admin.ResolveAlertAsync(alertId);
            return RedirectToAction("WarehouseAlerts", "Procurement");
        }
    }
}
