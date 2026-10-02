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
            if (HttpContext.Session.GetString("JwtToken") != null)
                return RedirectToAction("Dashboard", "Procurement");

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

            return RedirectToAction("Dashboard", "Procurement");
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

            return RedirectToAction("Dashboard", "Procurement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
