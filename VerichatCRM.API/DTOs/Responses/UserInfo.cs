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
        /// show it as a nudge, never as a wall - a confirmed address proves the
        /// mailbox is read, but the account already signs in without it.
        /// </summary>
        public bool EmailVerified { get; set; }
    }
}