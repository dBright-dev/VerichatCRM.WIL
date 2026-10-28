using VerichatCRM.Web.Authorization;

namespace VerichatCRM.Web.Models
{
    public class StatusPageViewModel
    {
        public int Code { get; set; }
        public string Heading { get; set; } = "";
        public string Message { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsAuthenticated { get; set; }
        public string? RoleLabel { get; set; }
        public string PrimaryUrl { get; set; } = "/";
        public string PrimaryLabel { get; set; } = "Go to home";
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public static StatusPageViewModel For(int code, string? role, bool isAuthenticated, string? requestId = null)
        {
            var model = new StatusPageViewModel
            {
                Code = code,
                IsAuthenticated = isAuthenticated,
                RoleLabel = role,
                RequestId = requestId,
                PrimaryUrl = isAuthenticated ? RoleHome.UrlFor(role) : "/account/login",
                PrimaryLabel = !isAuthenticated
                    ? "Sign in"
                    : RoleHome.HasWebScreens(role) ? "Back to your dashboard" : "Go to home"
            };

            switch (code)
            {
                case 403:
                    model.Icon = "🔒";
                    model.Heading = "You don't have access to this page";
                    model.Message = RoleHome.HasWebScreens(role)
                        ? $"Your role ({role}) can't open this screen. If you think you need access, ask a system administrator."
                        : "Client accounts sign in through the Verichat mobile app. Staff screens aren't available to your role.";
                    break;
                case 404:
                    model.Icon = "🧭";
                    model.Heading = "We can't find that page";
                    model.Message = "The link may be broken, or the page may have moved.";
                    break;
                default:
                    model.Icon = "⚠";
                    model.Heading = "Something went wrong on our side";
                    model.Message = "Try again in a moment. If it keeps happening, share the request ID below with your administrator.";
                    break;
            }

            return model;
        }
    }
}
