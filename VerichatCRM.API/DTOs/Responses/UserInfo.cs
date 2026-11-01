namespace VerichatCRM.API.DTOs.Responses
{
    public class UserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Whether the owner has clicked the link sent to this address. Front ends
        /// show it as a nudge, never as a wall - an employee's approval is what
        /// grants access here, and approval is the stronger claim.
        /// </summary>
        public bool EmailVerified { get; set; }
    }
}