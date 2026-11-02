using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.Models.Public;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Home;

namespace VerichatCRM.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPublicEnquiryApiService _enquiries;
        private readonly IPublicCatalogApiService _catalog;

        public HomeController(IPublicEnquiryApiService enquiries, IPublicCatalogApiService catalog)
        {
            _enquiries = enquiries;
            _catalog = catalog;
        }

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

        /// <summary>
        /// The four pages a visitor with no account reads before deciding to trust this
        /// company. They sit on the landing shell, so they need URLs a person can guess -
        /// <c>/about</c> rather than <c>/Home/About</c> - and the footer and navigation of
        /// that shell link the short forms.
        /// </summary>
        [Route("about")]
        public IActionResult About() => View();

        [Route("services")]
        public IActionResult Services() => View();

        [Route("gallery")]
        public async Task<IActionResult> Gallery() =>
            View(GalleryViewModel.From(await _catalog.GetProductsAsync()));

        [HttpGet]
        [Route("contact")]
        public IActionResult Contact() => View(new PublicEnquiryForm());

        /// <summary>
        /// Files the enquiry through the API's public door and shows the receipt.
        /// </summary>
        [HttpPost]
        [Route("contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(PublicEnquiryForm form)
        {
            // A filled-in field that no reader of this page was shown means the submitter
            // was not a reader. Answered with the same screen and the same headline as a
            // real enquiry, minus the reference, so a script scanning the responses learns
            // only that posting here always looks like it worked.
            if (!string.IsNullOrWhiteSpace(form.Website))
                return View("ContactSent", Receipt(form, null));

            if (!ModelState.IsValid) return View(form);

            var result = await _enquiries.SubmitAsync(form);

            // Refusal is not a receipt. The visitor still has the enquiry in front of them
            // and the page still names the two ways round it that do not need this server.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(form);
            }

            return View("ContactSent", Receipt(form, result));
        }

        private static ContactReceiptViewModel Receipt(PublicEnquiryForm form, PublicEnquiryResult? result) => new()
        {
            Name = form.Name,
            Email = form.Email,
            Message = result?.Message ??
                "Your enquiry is with the sales desk. Expect an answer at " + form.Email +
                " within one working day.",
            Reference = result?.Reference
        };

        /// <summary>
        /// The published notice. One URL, <c>/privacy</c>, which the staff sidebar, the
        /// sign-in footnote and the landing footer all now point at: a policy that exists
        /// at two addresses is two chances for one of them to be out of date.
        /// </summary>
        [Route("privacy")]
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