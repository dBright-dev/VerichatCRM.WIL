using Microsoft.AspNetCore.Mvc;
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
            var role = HttpContext.Session.GetString("UserRole");
            if (HttpContext.Session.GetString("JwtToken") != null && role != null)
                return RedirectToRoleDashboard(role);

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

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserRole", result.User.Role);
            HttpContext.Session.SetString("UserName", result.User.DisplayName);
            HttpContext.Session.SetString("UserEmail", result.User.Email);

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
            if (!ModelState.IsValid) return View(request);

            var result = await _auth.RegisterAsync(request);

            if (!result.Success || result.Token == null || result.User == null)
            {
                ModelState.AddModelError("", result.Message);
                return View(request);
            }

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserRole", result.User.Role);
            HttpContext.Session.SetString("UserName", result.User.DisplayName);
            HttpContext.Session.SetString("UserEmail", result.User.Email);

            if (result.User.Role == "System Administrator")
                return RedirectToAction("Dashboard", "Admin");
            return RedirectToRoleDashboard(result.User.Role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            return role switch
            {
                "System Administrator" => RedirectToAction("Dashboard", "Admin"),
                "Sales Representative" => RedirectToAction("Index", "Pipeline"),
                "Procurement Manager" => RedirectToAction("Dashboard", "Procurement"),
                "Field Support Engineer" => RedirectToAction("Dashboard", "Support"),
                "Medical Student" => RedirectToAction("Index", "Catalog"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
