namespace VerichatCRM.Web.Configuration
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class JwtSettings
    {
        public string CookieName { get; set; } = "verichat_token";
    }
}
