using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Models;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.Models.Auth;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebAuthService _auth;
        private readonly IWebHostEnvironment _env;

        public AccountController(IWebAuthService auth, IWebHostEnvironment env)
        {
            _auth = auth;
            _env = env;
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
        public async Task<IActionResult> Login(LoginRequest request, string? demoCode)
        {
            // One-click demo sign-in, and the reason the button posts a code rather
            // than an address and password: the credential is read from the same list
            // the API's seeder writes, so it never reaches the page and a caller
            // cannot post an arbitrary address at this form. Outside Development the
            // seeded accounts do not exist, so the code is refused here instead of
            // being handed to the API to fail against.
            if (!string.IsNullOrWhiteSpace(demoCode))
            {
                var account = _env.IsDevelopment() ? DemoAccounts.Find(demoCode) : null;
                if (account is null)
                {
                    ModelState.AddModelError("", "Demo sign-in is only available while the API is seeding development accounts.");
                    return View(request);
                }

                request = new LoginRequest { Email = account.Email, Password = account.Password };
            }

            if (!ModelState.IsValid) return View(request);

            var result = await _auth.LoginAsync(request);

            if (!result.Success || result.Token == null || result.User == null)
            {
                ModelState.AddModelError("", RefusedWith(result, "We could not sign you in just now. Check your address and password, then try again."));
                return View(request);
            }

            return CompleteSignIn(result);
        }

        /// <summary>
        /// Writes the signed-in session and sends the user to the board their role
        /// owns. Shared with the sign-up path, so the form that just chose a password
        /// signs the person in with it instead of bouncing them to the login screen to
        /// type the same password a second time.
        /// </summary>
        private IActionResult CompleteSignIn(AuthResponse result)
        {
            try
            {
                HttpContext.Session.SetString("JwtToken", result.Token!);
                HttpContext.Session.SetString("UserRole", result.User!.Role);
                HttpContext.Session.SetString("UserName", result.User.DisplayName);
                HttpContext.Session.SetString("UserEmail", result.User.Email);

                // The client portal is scoped to the signed-in client, and every
                // client-facing API route is keyed on this id rather than on the
                // token, so the web layer has to carry it too.
                HttpContext.Session.SetString("UserId", result.User.UserId);

                // The token also goes in a cookie so the SignalR handshake to the API
                // can authenticate without the page ever reading it.
                IssueHubCookie(result.Token!);
            }
            catch
            {
                // Session may not be configured - continue anyway
            }

            // System Administrator always lands on the Admin dashboard. Both callers
            // have already proved Token and User are set, which is why this method
            // takes the response rather than picking the pieces out itself.
            if (result.User!.Role == "System Administrator")
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
            // Never trust the posted role string. The set checked here has to be
            // the set the API enforces (UserRoles.SelfRegisterable), not the full
            // role list - otherwise the form accepts a role the server will refuse
            // and the refusal arrives as an opaque error instead of a field message.
            // That set is only the two client-facing roles: this form opens a client
            // or student account, and a staff account is created by an employee on
            // /Admin/Users.
            if (!UserRoles.SelfRegisterable.Contains(request.Role))
            {
                ModelState.AddModelError("Role", "Select a valid role.");
            }

            if (!ModelState.IsValid) return View(request);

            var result = await _auth.RegisterAsync(request);

            if (!result.Success || result.User == null)
            {
                ModelState.AddModelError("", RefusedWith(result, "We could not submit your details just now. Check them and try again."));
                return View(request);
            }

            // The account is live the moment the API writes it, so the person who just
            // chose the password is signed in with it. Nothing is reviewed, and no
            // "wait for approval" screen is shown - the only gate left on this form is
            // the role list above.
            var signedIn = await _auth.LoginAsync(new LoginRequest
            {
                Email = request.Email,
                Password = request.Password
            });

            if (signedIn.Success && signedIn.Token != null && signedIn.User != null)
                return CompleteSignIn(signedIn);

            // The account exists but this sign-in did not complete - e.g. the session
            // store refused the write. Say so with the login form rather than claiming
            // a session that was never issued.
            ModelState.AddModelError("", "Your account is created. Sign in with the address and password you just chose.");
            return View("Login", new LoginRequest { Email = request.Email });
        }

        /// <summary>
        /// "I cannot get in" - the forgotten-password form. Anonymous, like sign-in
        /// itself; the whole point is that the person cannot authenticate right now.
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordRequest());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _auth.ForgotPasswordAsync(request.Email);

            // One screen for both outcomes, because the API gives one sentence for
            // both outcomes: whether or not the address has an account, the next thing
            // this person can do is the same - wait for a link, or sign up if they
            // never had an account. Anything more specific would turn a help screen
            // into a directory of who registered.
            return View("Recovery", RecoveryNoticeViewModel.Worked(
                "Ask answered",
                RefusedWith(result, "If that address has a Verichat account, a reset link is on its way. " +
                                    "It works once and expires in an hour."),
                request.Email));
        }

        /// <summary>
        /// The page the emailed reset link opens. The token arrives in the query
        /// string because that is all a mail client can do with a link; it is then
        /// posted on as a hidden field so it never appears in a form the user has to
        /// read.
        /// </summary>
        [HttpGet]
        public IActionResult ResetPassword(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return View("Recovery", RecoveryNoticeViewModel.Problem(
                    "That link was cut short",
                    "A reset link carries a long code, and some mail clients break it when they wrap the line. " +
                    "Request a fresh link and click it in one go."));

            return View(new ResetPasswordRequest { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _auth.ResetPasswordAsync(request);

            if (!result.Success)
            {
                // An expired or already-used token is worth the API's exact words,
                // because the fix is to ask again rather than to try the same link a
                // second time.
                ModelState.AddModelError("", RefusedWith(result,
                    "That reset link did not work. Request a new one and try again."));
                return View(request);
            }

            return View("Recovery", RecoveryNoticeViewModel.Worked("Password changed", result.Message));
        }

        /// <summary>
        /// The page an email-confirmation link opens. It acts on the token immediately
        /// rather than showing a button: the click is the confirmation, and asking the
        /// person to submit a form they did not intend to load adds a step that proves
        /// nothing extra.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return View("Recovery", RecoveryNoticeViewModel.Problem(
                    "That link was cut short",
                    "A confirmation link carries a long code, and some mail clients break it when they wrap the line. " +
                    "Sign in and ask for a new one."));

            var result = await _auth.ConfirmEmailAsync(token);

            return result.Success
                ? View("Recovery", RecoveryNoticeViewModel.Worked("Address confirmed", result.Message))
                : View("Recovery", RecoveryNoticeViewModel.Problem("This link did not work", RefusedWith(result,
                    "That confirmation link is not one we still accept. Sign in and ask for a new one.")));
        }

        /// <summary>
        /// "I cannot get in", for the cases the two flows above do not cover. Reachable
        /// from the sign-in form, for somebody whose address is mistyped, whose role is
        /// wrong, or whose account simply does not behave.
        /// </summary>
        [HttpGet]
        public IActionResult Help() => View(new AskForHelpForm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Help(AskForHelpForm form)
        {
            // A filled-in field that no reader of this page was shown means the
            // submitter was not a reader. Answered with the same screen and the same
            // sentence as a real note, so a script scanning the response learns that
            // posting here always looks like it worked - which is also the only
            // honest thing to say, since nothing here is ever verified.
            if (!string.IsNullOrWhiteSpace(form.Website))
                return View("Recovery", RecoveryNoticeViewModel.Worked(
                    "Note received",
                    "Your note is with a Verichat administrator. If the address you gave is a real " +
                    "mailbox, expect an answer there.",
                    form.Email));

            if (!ModelState.IsValid) return View(form);

            var result = await _auth.AskForHelpAsync(form);

            // Not a "success" screen: this always reads the same whatever the address
            // is, and the API's own wording already says what happens next.
            return View("Recovery", RecoveryNoticeViewModel.Worked(
                "Note received",
                RefusedWith(result, "Your note is with a Verichat administrator."),
                form.Email));
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
        /// The API's sentence about a refusal, with something usable when it said
        /// nothing - which happens when the response was an error page or a
        /// validation body rather than the auth payload this form expects.
        /// </summary>
        private static string RefusedWith(Models.Auth.AuthResponse result, string fallback) =>
            string.IsNullOrWhiteSpace(result.Message) ? fallback : result.Message;

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
                // Logistics had the support dashboard as its home, so a dispatcher
                // signed in to a screen of ticket counts. It has its own board now.
                Roles.Logistics => RedirectToAction("Dashboard", "Logistics"),

                // Client and student accounts have no staff screens. They land on the
                // self-service portal, which shows only their own catalogue access,
                // quotations, deliveries and tickets - the same data the mobile app
                // reads, through the same server-side ownership rules.
                Roles.ClientUser or Roles.MedicalStudent => RedirectToAction("Index", "Portal"),

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

            // This used to be a bare View(), which searched for
            // Views/Account/NoAccess.cshtml - a file that never existed - so every
            // permission denial ended as "Invalid view name" / HTTP 500 instead of
            // the 403 page. Views/Shared/Unauthorized.cshtml is the branded 403
            // that the status-code pipeline already uses for the same code.
            var role = HttpContext.Session.GetString("UserRole");
            return View("Unauthorized", StatusPageViewModel.For(403, role, true));
        }
    }
}
