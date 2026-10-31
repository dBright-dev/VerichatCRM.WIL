using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.ViewModels.Home;

namespace VerichatCRM.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                IsAuthenticated = HttpContext.Session.GetString("JwtToken") != null,
                UserName = HttpContext.Session.GetString("UserName"),
                UserRole = HttpContext.Session.GetString("UserRole")
            };
            return View(model);
        }

        public IActionResult Privacy() => View();

        // Target of UseExceptionHandler for unhandled exceptions.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("Error", BuildStatusModel(500));
        }

        // Target of UseStatusCodePagesWithReExecute. Renders a branded page for 404 / 403 / other errors and sends 401 back to the login screen.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusPage(int code = 404)
        {
            // Missing images/css/js: don't render an HTML page into an asset request.
            var original = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (original is not null && Path.HasExtension(original.OriginalPath))
                return StatusCode(code);

            if (code == StatusCodes.Status401Unauthorized)
                return RedirectToAction("Login", "Account");

            if (code < 400 || code > 599) code = 404;
            Response.StatusCode = code;

            return code switch
            {
                404 => View("NotFound", BuildStatusModel(404)),
                403 => View("Unauthorized", BuildStatusModel(403)),
                _ => View("Error", BuildStatusModel(code))
            };
        }

        private StatusPageViewModel BuildStatusModel(int code) =>
            StatusPageViewModel.For(
                code,
                HttpContext.Session.GetString("UserRole"),
                HttpContext.Session.GetString("JwtToken") != null,
                Activity.Current?.Id ?? HttpContext.TraceIdentifier);

    }
}