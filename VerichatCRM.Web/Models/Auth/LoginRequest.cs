namespace VerichatCRM.Web.Models.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Display-only role picker (NOT sent to API — API LoginRequest has Email+Password only).
        public string? Role { get; set; }
    }
}
