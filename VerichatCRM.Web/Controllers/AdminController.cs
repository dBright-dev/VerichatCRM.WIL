using Microsoft.AspNetCore.Mvc;

namespace VerichatCRM.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Admin Dashboard";
            ViewData["Subtitle"] = "Global analytics, users, and system configuration.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "System Administrator";
            return View();
        }
    }
}
