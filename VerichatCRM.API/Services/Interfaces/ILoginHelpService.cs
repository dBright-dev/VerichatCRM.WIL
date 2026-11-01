using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    /// <summary>
    /// The note someone sends when the sign-in screen will not let them through.
    /// <para>
    /// Kept deliberately narrow. This is not a helpdesk: there is no thread, no
    /// status beyond open-or-closed, and nothing here grants access. It exists
    /// because the three recovery routes - password reset, email confirmation, the
    /// approval queue - all assume the account is basically sound, and a person
    /// whose application has sat unread for a week, or who typed their employer's
    /// address instead of their own, has nowhere else to say so.
    /// </para>
    /// </summary>
    public interface ILoginHelpService
    {
        /// <summary>
        /// Records the note and tells an administrator about it. The channel label is
        /// normalised inside, so "Mobile", "web" or nonsense from a hand-built request
        /// all land on one of the two words the queue displays.
        /// </summary>
        Task<HelpOutcome> SubmitAsync(AskForHelpRequest request);

        Task<List<LoginHelpResponse>> GetOpenAsync();

        Task<HelpOutcome> ResolveAsync(string id, string resolverUserId);
    }

    /// <summary>
    /// Whether the note was kept, and the sentence to show the person who sent it or
    /// the administrator who closed it.
    /// </summary>
    public record HelpOutcome(bool Success, string Message);
}
