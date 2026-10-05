using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VerichatCRM.Web.Authorization
{
    /// <summary>
    /// Server-side access control for staff screens.
    /// <para>
    /// Applied to a controller action (or the whole controller). The signed-in
    /// user's role is read from the session that was established at login. If
    /// there is no session the visitor is sent to the login page; if the role
    /// does not hold the required permission the request is refused, so a user
    /// cannot reach a screen by typing its URL.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RequirePermissionAttribute : ActionFilterAttribute
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (session.GetString("JwtToken") is null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var role = session.GetString("UserRole");
            if (Perm.Has(role ?? string.Empty, _permission))
            {
                return;
            }

            // Authenticated but not entitled: refuse rather than redirect, so the
            // user is never shown a screen they may not use.
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}
