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

                // The token also goes in a cookie so the SignalR handshake to the API
                // can authenticate without the page ever reading it.
                IssueHubCookie(result.Token);
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

                // The token also goes in a cookie so the SignalR handshake to the API
                // can authenticate without the page ever reading it.
                IssueHubCookie(result.Token);
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

            // The session is gone but the bearer token in the hub cookie is valid
            // for its full expiry, so it has to be withdrawn with the session or
            // signing out leaves a usable credential in the browser.
            Response.Cookies.Delete("JwtToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });

            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Issues the HttpOnly cookie the SignalR handshake to the API presents.
        /// The token never reaches page script, which is why the hub has to carry
        /// it as a cookie instead of an Authorization header.
        /// </summary>
        private void IssueHubCookie(string token)
        {
            // The hub lives on another port, and for a non-registrable host such
            // as localhost the port is part of the site - so even on a developer
            // machine the request is cross-site and needs SameSite=None. Browsers
            // only accept that with Secure, but they treat http://localhost as
            // potentially trustworthy, so Secure is allowed there too.
            var host = HttpContext.Request.Host.Host;
            var isLocalhost = host is "localhost" or "127.0.0.1" or "[::1]";
            var crossSiteSendable = HttpContext.Request.IsHttps || isLocalhost;

            Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = crossSiteSendable,
                SameSite = crossSiteSendable ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
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
