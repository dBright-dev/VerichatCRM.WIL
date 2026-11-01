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
            // Named the same as the sidebar tile and the browser tab, so the three
            // agree on what this screen is called.
            ViewData["Title"] = "Admin Dashboard";
            ViewData["Subtitle"] = $"System Administrator overview — {DateTime.Now:dddd, d MMMM yyyy}";
            // Its own view, not Views/Dashboard/Index.cshtml: the admin board is the
            // one that pairs the numbers with the controls that change them.
            return View(dashboard);
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
            ViewData["Subtitle"] = "Change roles and deactivate accounts";
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

        /// <summary>
        /// The queue of accounts people created for themselves. Reachable by the
        /// administrator and by Sales and Procurement - see
        /// <see cref="Perm.AccountApprovals"/> - and on this screen the only
        /// available actions are yes and no, never a role change: a reviewer cannot
        /// promote an applicant into something they did not ask for.
        /// </summary>
        [RequirePermission(Perm.AccountApprovals)]
        public async Task<IActionResult> Approvals()
        {
            ViewData["Title"] = "Account Approvals";
            ViewData["Subtitle"] = "Applications that signed themselves up";
            var pending = await _admin.GetPendingAccountsAsync();
            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.AccountApprovals)]
        public async Task<IActionResult> ApproveAccount(string userId)
        {
            var outcome = await _admin.ApproveAccountAsync(userId);
            TempData[outcome.Success ? "SuccessMessage" : "ErrorMessage"] = outcome.Message;
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.AccountApprovals)]
        public async Task<IActionResult> RejectAccount(string userId, string? reason)
        {
            var outcome = await _admin.RejectAccountAsync(userId, reason ?? string.Empty);
            TempData[outcome.Success ? "SuccessMessage" : "ErrorMessage"] = outcome.Message;
            return RedirectToAction(nameof(Approvals));
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