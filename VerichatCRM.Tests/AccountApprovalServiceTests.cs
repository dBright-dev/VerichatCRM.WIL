using Microsoft.Extensions.Logging.Abstractions;
using VerichatCRM.API.Services;
using VerichatCRM.Core.Entities;
using Xunit;

namespace VerichatCRM.Tests;

public class AccountApprovalServiceTests
{
    private const string ReviewerId = "reviewer-1";

    [Fact]
    public async Task Approve_MarksTheApplicationApprovedAndTellsTheApplicant()
    {
        var (users, email, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.ClientUser);

        var decision = await service.ApproveAsync(applicant.UserId, ReviewerId);

        Assert.True(decision.Success);
        Assert.Equal(UserApproval.Approved, applicant.ApprovalStatus);
        Assert.Equal(ReviewerId, applicant.ReviewedByUserId);
        Assert.NotNull(applicant.ReviewedAt);

        // The role travels with the decision because the caller - not this service -
        // opens the applicant's CRM client record, and only the role says whether
        // there is one to open.
        Assert.Equal(applicant.UserId, decision.UserId);
        Assert.Equal(UserRoles.ClientUser, decision.Role);

        var notice = Assert.Single(email.Sent);
        Assert.Equal(applicant.Email, notice.To);
    }

    [Fact]
    public async Task Approve_RefusesTheReviewersOwnApplication()
    {
        // The role gate on the route cannot catch this: a Sales Representative
        // legitimately holds the permission, and can legitimately have signed
        // themselves up and be sitting in the queue.
        var (users, email, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.SalesRepresentative);

        var decision = await service.ApproveAsync(applicant.UserId, applicant.UserId);

        Assert.False(decision.Success);
        Assert.Equal(UserApproval.Pending, applicant.ApprovalStatus);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task Approve_RefusesAnApplicationThatHasAlreadyBeenDecided()
    {
        var (users, email, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.ClientUser);
        await service.RejectAsync(applicant.UserId, ReviewerId, "No evidence this is a clinic.");

        var decision = await service.ApproveAsync(applicant.UserId, ReviewerId);

        Assert.False(decision.Success);
        Assert.Equal(UserApproval.Rejected, applicant.ApprovalStatus);
        Assert.Single(email.Sent);
    }

    [Fact]
    public async Task Reject_WithoutAReason_LeavesTheApplicationWhereItWas()
    {
        var (users, email, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.ProcurementManager);

        var decision = await service.RejectAsync(applicant.UserId, ReviewerId, "   ");

        Assert.False(decision.Success);
        Assert.Equal(UserApproval.Pending, applicant.ApprovalStatus);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task Reject_RecordsTheReasonAndSendsItEscaped()
    {
        var (users, email, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.ProcurementManager);

        var decision = await service.RejectAsync(applicant.UserId, ReviewerId, "We could not match this address to a hospital.");

        Assert.True(decision.Success);
        Assert.Equal(UserApproval.Rejected, applicant.ApprovalStatus);
        Assert.Equal("We could not match this address to a hospital.", applicant.RejectionReason);

        var notice = Assert.Single(email.Sent);
        Assert.Contains("could not match this address", notice.Body);
    }

    [Fact]
    public async Task Reject_EscapesTheReasonBeforeItBecomesAnEmail()
    {
        // The reason is free text an employee typed, and it is re-sent to the
        // applicant inside an HTML body. Markup in it must stay markup on screen.
        var (users, email, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.ClientUser);

        await service.RejectAsync(applicant.UserId, ReviewerId, "<img src=x onerror=alert(1)>");

        var notice = Assert.Single(email.Sent);
        Assert.DoesNotContain("<img", notice.Body);
        Assert.Contains("&lt;img", notice.Body);
    }

    [Fact]
    public async Task DecisionIsRecordedEvenWhenTheNoticeEmailFails()
    {
        // Losing the approval because an SMTP host had a bad minute would be worse
        // than the applicant waiting for the email.
        var (users, email, service) = Create();
        email.FailOnSend = true;
        var applicant = await AddApplicationAsync(users, UserRoles.ClientUser);

        var decision = await service.ApproveAsync(applicant.UserId, ReviewerId);

        Assert.True(decision.Success);
        Assert.Equal(UserApproval.Approved, applicant.ApprovalStatus);
    }

    [Fact]
    public async Task GetPending_FlagsAStaffRoleAsConsoleAccess()
    {
        var (users, _, service) = Create();
        await AddApplicationAsync(users, UserRoles.ClientUser);
        await AddApplicationAsync(users, UserRoles.SalesRepresentative);

        var queue = await service.GetPendingAsync();

        var client = queue.Single(a => a.Role == UserRoles.ClientUser);
        var staff = queue.Single(a => a.Role == UserRoles.SalesRepresentative);
        Assert.False(client.IsStaffRole);
        Assert.True(staff.IsStaffRole);
    }

    [Fact]
    public async Task Approve_ClearsAReasonLeftByAnEarlierRefusal()
    {
        // A declined application that is later approved must not still carry the
        // refusal: the login path quotes it back to the person.
        var (users, _, service) = Create();
        var applicant = await AddApplicationAsync(users, UserRoles.ClientUser);
        await service.RejectAsync(applicant.UserId, ReviewerId, "Wrong address.");
        applicant.ApprovalStatus = UserApproval.Pending;

        await service.ApproveAsync(applicant.UserId, ReviewerId);

        Assert.Null(applicant.RejectionReason);
    }

    private static (FakeUserRepository, FakeEmailService, AccountApprovalService) Create()
    {
        var users = new FakeUserRepository();
        var email = new FakeEmailService();
        var service = new AccountApprovalService(users, email, NullLogger<AccountApprovalService>.Instance);
        return (users, email, service);
    }

    private static async Task<User> AddApplicationAsync(FakeUserRepository users, string role)
    {
        var applicant = new User
        {
            DisplayName = "Applicant",
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "unused",
            Role = role,
            ApprovalStatus = UserApproval.Pending
        };
        await users.AddAsync(applicant);
        return applicant;
    }
}
