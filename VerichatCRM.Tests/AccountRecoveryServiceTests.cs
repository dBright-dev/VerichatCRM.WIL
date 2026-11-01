using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Services;
using VerichatCRM.Core.Entities;
using Xunit;

namespace VerichatCRM.Tests;

/// <summary>
/// The two ways an account can be unreachable: a forgotten password, and an address
/// nobody has confirmed. Both flows hand a bearer token to somebody who cannot sign
/// in, so most of what is proved here is what the flow refuses to say and how
/// quickly the token dies.
/// </summary>
public class AccountRecoveryServiceTests
{
    private const string WebOrigin = "http://web.test";

    [Fact]
    public async Task ForgotPassword_UnknownAddress_SaysExactlyWhatAKnownOneWould()
    {
        var (users, _, service) = Create();
        var known = await AddUserAsync(users);

        var unknown = await service.ForgotPasswordAsync("nobody@example.com");
        var actual = await service.ForgotPasswordAsync(known.Email);

        Assert.True(unknown.Success);
        Assert.Equal(actual.Message, unknown.Message);
        Assert.Contains("If that address has a Verichat account", unknown.Message);
    }

    [Fact]
    public async Task ForgotPassword_WritesTheTokenHashAndLinksToTheWebApp()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);

        await service.ForgotPasswordAsync(user.Email);

        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetExpiry);

        // The row carries a digest. Somebody with SELECT on Users, or a backup that
        // was never meant to be read, gets nothing they can paste into the form.
        var token = LastToken(email, "ResetPassword");
        Assert.NotEqual(token, user.PasswordResetToken);
        Assert.Equal(RecoveryToken.Hash(token), user.PasswordResetToken);

        // The link is the web app's page rather than this API's: a reset form served
        // from the API would have nowhere to post back to.
        Assert.StartsWith($"{WebOrigin}/Account/ResetPassword?token=", LastLink(email, "ResetPassword"));
    }

    [Fact]
    public async Task ForgotPassword_ForADeactivatedAccount_WritesNothingAndSendsWiththing()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        user.IsActive = false;

        var result = await service.ForgotPasswordAsync(user.Email);

        // The same neutral sentence, because "that account is disabled" would confirm
        // the address exists. Re-enabling is an employee's job, not a link's.
        Assert.True(result.Success);
        Assert.Null(user.PasswordResetToken);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task ResetPassword_WithTheIssuedToken_SetsThePasswordAndBurnsTheLink()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        var original = user.PasswordHash;
        await service.ForgotPasswordAsync(user.Email);

        var result = await service.ResetPasswordAsync(LastToken(email, "ResetPassword"), "brand-new-password");

        Assert.True(result.Success);
        Assert.False(PasswordHasher.Verify("existing-password", user.PasswordHash));
        Assert.True(PasswordHasher.Verify("brand-new-password", user.PasswordHash));
        Assert.NotEqual(original, user.PasswordHash);

        // A used link is a dead link, even though the email it arrived in may still
        // be sitting in the inbox, forwarded, or open in a second tab.
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetExpiry);
    }

    [Fact]
    public async Task ResetPassword_TheSameTokenWorksOnce()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        await service.ForgotPasswordAsync(user.Email);
        var token = LastToken(email, "ResetPassword");

        Assert.True((await service.ResetPasswordAsync(token, "first-changes-it")).Success);

        var second = await service.ResetPasswordAsync(token, "second-should-not");
        Assert.False(second.Success);
        Assert.True(PasswordHasher.Verify("first-changes-it", user.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_AskingTwiceRetiresTheFirstLink()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        await service.ForgotPasswordAsync(user.Email);
        var first = LastToken(email, "ResetPassword");
        await service.ForgotPasswordAsync(user.Email);

        var stale = await service.ResetPasswordAsync(first, "not-from-the-old-link");
        Assert.False(stale.Success);

        var newest = await service.ResetPasswordAsync(LastToken(email, "ResetPassword"), "from-the-newest");
        Assert.True(newest.Success);
    }

    [Fact]
    public async Task ResetPassword_ExpiredLink_RefusedWithSomethingToDo()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        await service.ForgotPasswordAsync(user.Email);
        var token = LastToken(email, "ResetPassword");
        user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(-1);

        var result = await service.ResetPasswordAsync(token, "too-late-password");

        Assert.False(result.Success);
        Assert.Contains("expired", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(PasswordHasher.Verify("too-late-password", user.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_ForgedOrBlankToken_RefusedEveryTime()
    {
        var (users, _, service) = Create();
        var user = await AddUserAsync(users);

        foreach (var token in new[] { "", "   ", Guid.NewGuid().ToString("N"), "DEADBEEF" })
            Assert.False((await service.ResetPasswordAsync(token, "whatever-else")).Success);

        Assert.False(PasswordHasher.Verify("whatever-else", user.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_TooShort_LeavesTheAccountAloneAndKeepsTheLink()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        await service.ForgotPasswordAsync(user.Email);
        var token = LastToken(email, "ResetPassword");

        var result = await service.ResetPasswordAsync(token, "short");

        Assert.False(result.Success);
        Assert.True(PasswordHasher.Verify("existing-password", user.PasswordHash));
        // The link was not spent by a request that never reached the write, so the
        // applicant fixes the password instead of starting the whole flow again.
        Assert.NotNull(user.PasswordResetToken);
    }

    [Fact]
    public async Task ResetPassword_OnAPendingApplication_DoesNotPromiseASignIn()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users, UserApproval.Pending);
        await service.ForgotPasswordAsync(user.Email);

        var result = await service.ResetPasswordAsync(LastToken(email, "ResetPassword"), "new-enough-password");

        // A reset proves the mailbox, not the application. Telling this person to sign
        // in would send them to a login screen that refuses them.
        Assert.True(result.Success);
        Assert.Equal(UserApproval.Pending, result.ApprovalStatus);
        Assert.Contains("approve", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConfirmEmail_WithTheIssuedToken_MarksTheAddressConfirmedAndSpendsTheLink()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);

        await service.SendConfirmationEmailAsync(user.UserId);
        Assert.StartsWith($"{WebOrigin}/Account/VerifyEmail?token=", LastLink(email, "VerifyEmail"));
        Assert.Equal(RecoveryToken.Hash(LastToken(email, "VerifyEmail")), user.EmailVerificationToken);

        var result = await service.ConfirmEmailAsync(LastToken(email, "VerifyEmail"));

        Assert.True(result.Success);
        Assert.True(user.EmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.False((await service.ConfirmEmailAsync("whatever")).Success);
    }

    [Fact]
    public async Task ConfirmEmail_Expired_RefusedAndTheAddressStaysUnconfirmed()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        await service.SendConfirmationEmailAsync(user.UserId);
        var token = LastToken(email, "VerifyEmail");
        user.EmailVerificationExpiry = DateTime.UtcNow.AddDays(-1);

        Assert.False((await service.ConfirmEmailAsync(token)).Success);
        Assert.False(user.EmailVerified);
    }

    [Fact]
    public async Task ConfirmEmail_AnAddressAlreadyConfirmed_IsNotMailedAgain()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);
        user.EmailVerified = true;

        await service.SendConfirmationEmailAsync(user.UserId);

        Assert.Empty(email.Sent);
        Assert.Null(user.EmailVerificationToken);
    }

    [Fact]
    public async Task SendConfirmation_EmailServerDown_StillLeavesAUsableLink()
    {
        var (users, email, service) = Create();
        email.FailOnSend = true;
        var user = await AddUserAsync(users);

        await service.SendConfirmationEmailAsync(user.UserId);

        // The token is written before the send and the failure is logged rather than
        // thrown: a relay that is down costs the email, not the flow.
        Assert.NotNull(user.EmailVerificationToken);
    }

    [Fact]
    public async Task Tokens_AreRandomPerRequestRatherThanDerivedFromTheAccount()
    {
        var (users, email, service) = Create();
        var user = await AddUserAsync(users);

        await service.ForgotPasswordAsync(user.Email);
        var first = LastToken(email, "ResetPassword");
        await service.ForgotPasswordAsync(user.Email);
        var second = LastToken(email, "ResetPassword");

        // Asking twice from the same address yields unrelated values, and nothing
        // about the account predicts either - so guessing a live link is a 128-bit
        // search rather than a guess about the person.
        Assert.NotEqual(first, second);
        Assert.DoesNotContain(user.Email.Split('@')[0], first);
        Assert.Equal(64, first.Length);
        Assert.Matches("^[0-9a-f]+$", first);
    }

    /// <summary>
    /// The link the fake transport was last asked to send for one of these pages.
    /// Read from the email rather than the row because the row only ever holds the
    /// hash - which is exactly the property being tested.
    /// </summary>
    private static string LastLink(FakeEmailService email, string path) =>
        email.Sent.Last(s => s.Body.Contains(path)).Body;

    private static string LastToken(FakeEmailService email, string path)
    {
        var body = LastLink(email, path);
        const string marker = "token=";
        var start = body.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = body.IndexOfAny(['"', ' ', '\n', '\r'], start);
        return end < 0 ? body[start..] : body[start..end];
    }

    private static (FakeUserRepository, FakeEmailService, AccountRecoveryService) Create()
    {
        var users = new FakeUserRepository();
        var email = new FakeEmailService();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Sites:WebBaseUrl"] = WebOrigin })
            .Build();
        var service = new AccountRecoveryService(
            users, email, config, NullLogger<AccountRecoveryService>.Instance);
        return (users, email, service);
    }

    private static async Task<User> AddUserAsync(
        FakeUserRepository users, string approvalStatus = UserApproval.Approved)
    {
        var user = new User
        {
            DisplayName = "Recovery Tester",
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = PasswordHasher.Hash("existing-password"),
            Role = UserRoles.ClientUser,
            ApprovalStatus = approvalStatus
        };
        await users.AddAsync(user);
        return user;
    }
}
