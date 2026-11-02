using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Admin;
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

        [RequirePermission(Perm.RoleAssignments)]
        public IActionResult CreateUser()
        {
            ViewData["Title"] = "Add user";
            ViewData["Subtitle"] = "Create an account with any role";
            return View(new CreateUserViewModel());
        }

        /// <summary>
        /// Creates a login for a named person. This is the only path to a staff
        /// account: the public sign-up form offers the two client-facing roles and
        /// nothing else, and the approval queue that used to turn a self-claimed
        /// staff application into a real one is gone.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.RoleAssignments)]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            // The select is populated from UserRoles.All, so an unexpected value is
            // a crafted post rather than a person picking something the screen hides.
            if (!UserRoles.All.Contains(model.Role))
                ModelState.AddModelError(nameof(model.Role), "Select a valid role.");

            if (!ModelState.IsValid) return View(model);

            var outcome = await _admin.CreateUserAsync(new CreateUserRequest
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role
            });

            if (!outcome.Success)
            {
                // The API's own sentence, because "Email is already registered." and
                // "the service was unreachable" need different next actions from
                // whoever is standing at this form.
                ModelState.AddModelError("", outcome.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = outcome.Message;
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// The notes from people who could not get in at all. Read by
        /// <see cref="UserRoles.SystemAdministrator"/>, Sales and Procurement -
        /// <see cref="Perm.SignInHelp"/> - because those are the roles that can change
        /// a role or deactivate an account, which is the complete list of fixes a
        /// sign-in problem needs.
        /// </summary>
        [RequirePermission(Perm.SignInHelp)]
        public async Task<IActionResult> HelpRequests()
        {
            ViewData["Title"] = "Sign-in Help";
            ViewData["Subtitle"] = "Notes from people who cannot get in";
            var notes = await _admin.GetHelpRequestsAsync();
            return View(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.SignInHelp)]
        public async Task<IActionResult> ResolveHelpRequest(string id)
        {
            var outcome = await _admin.ResolveHelpRequestAsync(id);
            TempData[outcome.Success ? "SuccessMessage" : "ErrorMessage"] = outcome.Message;
            return RedirectToAction(nameof(HelpRequests));
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