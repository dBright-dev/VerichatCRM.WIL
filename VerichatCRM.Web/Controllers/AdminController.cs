using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminApiService _admin;
        private readonly IApiClient _api;

        public AdminController(IAdminApiService admin, IApiClient api)
        {
            _admin = admin;
            _api = api;
        }

        [RequirePermission(Perm.AdminDashboard)]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _api.GetAsync<DashboardAnalyticsResponse>("/api/dashboard/analytics")
                ?? new DashboardAnalyticsResponse();
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = $"System Administrator overview — {DateTime.Now:dddd, d MMMM yyyy}";
            return View("~/Views/Dashboard/Index.cshtml", dashboard);
        }

        [RequirePermission(Perm.AdminAnalytics)]
        public IActionResult Analytics() => RedirectToAction("Index", "Analytics");

        [RequirePermission(Perm.AdminRules)]
        public IActionResult Rules()
        {
            ViewData["Title"] = "Rules Engine";
            ViewData["Subtitle"] = "Security rules and automation";
            return View();
        }

        [RequirePermission(Perm.RoleAssignments)]
        public async Task<IActionResult> Users()
        {
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
            await _admin.UpdateUserRoleAsync(userId, role);
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.RoleAssignments)]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            await _admin.DeactivateUserAsync(userId);
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.WarehouseAlerts)]
        public async Task<IActionResult> RunSlaCheck()
        {
            await _admin.RunSlaCheckAsync();
            TempData["SuccessMessage"] = "SLA escalation check triggered.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.WarehouseAlerts)]
        public async Task<IActionResult> ResolveAlert(string alertId)
        {
            await _admin.ResolveAlertAsync(alertId);
            return RedirectToAction("WarehouseAlerts", "Procurement");
        }
    }
}