namespace VerichatCRM.API.DTOs.Responses
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }

        /// <summary>
        /// The account's approval state, carried whenever it is the reason the
        /// answer is what it is: a sign-up that has to wait, a sign-in that is
        /// waiting, a declined application. The web and mobile front ends branch on
        /// this instead of matching on the wording of <see cref="Message"/>, so the
        /// screen they show cannot drift from the sentence the server chose.
        /// </summary>
        public string? ApprovalStatus { get; set; }
    }
}
