namespace VerichatCRM.Web.Models.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // There is deliberately no Role field: the role is read from the database
        // by the API at sign-in, never chosen by the user.
    }
}
