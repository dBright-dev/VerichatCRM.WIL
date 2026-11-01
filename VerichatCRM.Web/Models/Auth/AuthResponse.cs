namespace VerichatCRM.Web.Models.Auth
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserInfo? User { get; set; }

        /// <summary>
        /// The account's approval state, sent by the API whenever that state is why
        /// the answer is what it is. The sign-in and sign-up actions branch on it to
        /// choose the screen - the waiting page rather than the form again - instead
        /// of guessing from the wording of <see cref="Message"/>.
        /// </summary>
        public string? ApprovalStatus { get; set; }
    }
}
