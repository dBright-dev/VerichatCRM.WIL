namespace VerichatCRM.Core.Entities;

/// <summary>
/// The three states a self-registered account can be in, and the only place they
/// are spelled. The login path, the approval queue, the badge on the user list and
/// the emails sent to the applicant all read these, so a state cannot be added in
/// one of those places and quietly mean something else in the others.
/// </summary>
public static class UserApproval
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly string[] All = { Pending, Approved, Rejected };

    public static bool IsPending(string? status) =>
        string.Equals(status, Pending, System.StringComparison.OrdinalIgnoreCase);

    public static bool IsApproved(string? status) =>
        string.Equals(status, Approved, System.StringComparison.OrdinalIgnoreCase);

    public static bool IsRejected(string? status) =>
        string.Equals(status, Rejected, System.StringComparison.OrdinalIgnoreCase);
}
