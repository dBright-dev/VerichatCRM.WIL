using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Entities;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Auth;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebAuthService _auth;

        public AccountController(IWebAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                if (HttpContext.Session.GetString("JwtToken") != null && role != null)
                    return RedirectToRoleDashboard(role);
            }
            catch
            {
                // Session may not be available - continue to login view
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _auth.LoginAsync(request);

            if (!result.Success || result.Token == null || result.User == null)
            {
                ModelState.AddModelError("", result.Message);
                return View(request);
            }

            try
            {
                HttpContext.Session.SetString("JwtToken", result.Token);
                HttpContext.Session.SetString("UserRole", result.User.Role);
                HttpContext.Session.SetString("UserName", result.User.DisplayName);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                // Also set cookie for JavaScript access (used by notifications.js)
                Response.Cookies.Append("JwtToken", result.Token, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
            catch
            {
                // Session may not be configured - continue anyway
            }

            // System Administrator always lands on the Admin dashboard.
            if (result.User.Role == "System Administrator")
                return RedirectToAction("Dashboard", "Admin");
            return RedirectToRoleDashboard(result.User.Role);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Never trust the posted role string: it must be one of the roles the
            // application knows about, otherwise arbitrary values reach the database.
            if (!UserRoles.All.Contains(request.Role))
            {
                ModelState.AddModelError("Role", "Select a valid role.");
            }

            if (!ModelState.IsValid) return View(request);

            var result = await _auth.RegisterAsync(request);

            if (!result.Success || result.Token == null || result.User == null)
            {
                ModelState.AddModelError("", result.Message);
                return View(request);
            }

            try
            {
                HttpContext.Session.SetString("JwtToken", result.Token);
                HttpContext.Session.SetString("UserRole", result.User.Role);
                HttpContext.Session.SetString("UserName", result.User.DisplayName);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                // Also set cookie for JavaScript access (used by notifications.js)
                Response.Cookies.Append("JwtToken", result.Token, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
            catch
            {
                // Session may not be configured - continue anyway
            }

            if (result.User.Role == "System Administrator")
                return RedirectToAction("Dashboard", "Admin");
            return RedirectToRoleDashboard(result.User.Role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
            }
            catch
            {
                // Session may not be available
            }
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Sends a freshly authenticated user to the dashboard for their role.
        /// The role is read from the database via the API at login - it is never
        /// chosen by the user at sign-in time.
        /// </summary>
        private IActionResult RedirectToRoleDashboard(string role)
        {
            return role switch
            {
                Roles.SystemAdministrator => RedirectToAction("Dashboard", "Admin"),
                Roles.SalesRepresentative => RedirectToAction("Index", "Pipeline"),
                Roles.ProcurementManager => RedirectToAction("Dashboard", "Procurement"),
                Roles.FieldSupportEngineer => RedirectToAction("Dashboard", "Support"),
                Roles.Logistics => RedirectToAction("Dashboard", "Support"),

                // Client and student accounts have no staff screens in this
                // application; they sign in through the mobile app instead.
                _ => RedirectToAction("NoAccess", "Account"),
            };
        }

        [HttpGet]
        public IActionResult NoAccess()
        {
            try
            {
                if (HttpContext.Session.GetString("JwtToken") is null)
                    return RedirectToAction(nameof(Login));
            }
            catch
            {
                return RedirectToAction(nameof(Login));
            }
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
