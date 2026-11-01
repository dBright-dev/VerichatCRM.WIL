using VerichatCRM.Core.Entities;

namespace VerichatCRM.Web.Models.Auth
{
    /// <summary>
    /// What an applicant is told while their application is out of their own hands.
    /// <para>
    /// One screen carries both states because the person arriving has one question -
    /// why will these credentials not let me in - and the two honest answers are
    /// "an employee has not read it yet" and "an employee read it and said no, for
    /// this reason". Which of the two it is comes from the API's approval state, not
    /// from a guess, so the screen cannot promise a wait to someone who was declined
    /// or a refusal to someone who is merely queued.
    /// </para>
    /// </summary>
    public class AccountReviewViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = UserApproval.Pending;

        /// <summary>The server's own sentence, including the reason when there is one.</summary>
        public string Message { get; set; } = string.Empty;

        public bool IsDeclined => UserApproval.IsRejected(ApprovalStatus);

        /// <summary>
        /// Builds the screen from the API's answer. The response carries the account
        /// it is talking about in both cases - a declined sign-in proves the password
        /// first, so naming the applicant back to them leaks nothing they did not
        /// just demonstrate.
        /// </summary>
        public static AccountReviewViewModel From(AuthResponse response, string? postedEmail) => new()
        {
            DisplayName = response.User?.DisplayName ?? string.Empty,
            Email = response.User?.Email ?? postedEmail ?? string.Empty,
            Role = response.User?.Role ?? string.Empty,
            ApprovalStatus = response.ApprovalStatus ?? UserApproval.Pending,
            Message = response.Message
        };
    }
}
