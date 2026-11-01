using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    /// <summary>
    /// The reviewer's side of account approval: read the queue, decide an
    /// application. The applicant's side is in <see cref="IAuthService"/>, which
    /// writes the Pending row and refuses to hand out a token for it.
    /// </summary>
    public interface IAccountApprovalService
    {
        Task<List<PendingAccountResponse>> GetPendingAsync();

        Task<ApprovalDecision> ApproveAsync(string userId, string reviewerUserId);

        Task<ApprovalDecision> RejectAsync(string userId, string reviewerUserId, string? reason);
    }

    /// <summary>
    /// The outcome of a decision attempt. A review screen has to be able to say why
    /// it did not record what the reviewer clicked - the application was already
    /// decided, it is the reviewer's own, or it is not in the database - so the
    /// service answers with a sentence rather than an exception.
    /// <para>
    /// A successful decision also carries the applicant's id and role, because the
    /// caller that opens a client account's CRM record needs both and the service
    /// that writes the approval does not own that table.
    /// </para>
    /// </summary>
    public record ApprovalDecision(bool Success, string Message, string? UserId = null, string? Role = null);
}
