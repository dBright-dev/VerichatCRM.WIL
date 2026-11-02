using Microsoft.Extensions.Configuration;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Services;
using VerichatCRM.Core.Entities;
using Xunit;

namespace VerichatCRM.Tests;

public class AuthServiceTests
{
    private const string Password = "password1";

    [Fact]
    public async Task Register_SystemAdministratorNeedsNoApproval()
    {
        var users = new FakeUserRepository();
        var service = new AuthService(users, CreateJwt());
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Ops", Email = "ops@example.com", Password = Password, Role = UserRoles.SystemAdministrator
        });

        Assert.True(response.Success);
        Assert.Equal(UserApproval.Approved, response.ApprovalStatus);
        Assert.Equal(UserApproval.Approved, Assert.Single(users.Users).ApprovalStatus);
    }

    [Fact]
    public async Task Register_RefusesARoleNobodyMaySelfAssign()
    {
        var users = new FakeUserRepository();
        var service = new AuthService(users, CreateJwt());
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Tourist", Email = "tourist@example.com", Password = Password, Role = "Chief Executive Officer"
        });

        Assert.False(response.Success);
        Assert.Empty(users.Users);
    }

    [Fact]
    public async Task Register_WritesAnApplicationAndWithholdsTheToken()
    {
        var users = new FakeUserRepository();
        var service = new AuthService(users, CreateJwt());
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Client", Email = " CLIENT@Example.com ", Password = Password, Role = UserRoles.ClientUser
        });

        var stored = Assert.Single(users.Users);
        Assert.Equal("client@example.com", stored.Email);
        Assert.Equal(UserRoles.ClientUser, response.User!.Role);

        // The point of the gate: a claim made on a public form is not access. The
        // account is written down, and nothing that reads it hands out a session.
        Assert.True(response.Success);
        Assert.Equal(UserApproval.Pending, stored.ApprovalStatus);
        Assert.Equal(UserApproval.Pending, response.ApprovalStatus);
        Assert.Null(response.Token);
    }

    [Fact]
    public async Task Login_ApprovedAccount_ReturnsAToken()
    {
        var users = WithUser(UserRoles.ClientUser, UserApproval.Approved);
        var service = new AuthService(users, CreateJwt());

        var response = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = Password });

        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_PendingApplication_IsRefusedButNamesTheWait()
    {
        var users = WithUser(UserRoles.ProcurementManager, UserApproval.Pending);
        var service = new AuthService(users, CreateJwt());

        var response = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = Password });

        Assert.False(response.Success);
        Assert.Null(response.Token);
        Assert.Equal(UserApproval.Pending, response.ApprovalStatus);
        Assert.Contains("approve", response.Message);
    }

    [Fact]
    public async Task Login_DeclinedApplication_QuotesTheReasonBack()
    {
        var users = WithUser(UserRoles.SalesRepresentative, UserApproval.Approved);
        var applicant = users.Users.Single();
        applicant.ApprovalStatus = UserApproval.Rejected;
        applicant.RejectionReason = "This address is not one of our suppliers.";
        var service = new AuthService(users, CreateJwt());

        var response = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = Password });

        Assert.False(response.Success);
        Assert.Null(response.Token);
        Assert.Equal(UserApproval.Rejected, response.ApprovalStatus);
        Assert.Contains("not one of our suppliers", response.Message);
    }

    [Fact]
    public async Task Login_WrongPassword_SaysNothingAboutTheApplication()
    {
        // The account is waiting for a reviewer, and the answer to a bad password
        // must be the same sentence used for an address that does not exist at all.
        // Approving the applicant is a decision about them; this is not theirs to
        // overhear, and the check order in AuthService is what keeps it that way.
        var users = WithUser(UserRoles.ProcurementManager, UserApproval.Pending);
        var service = new AuthService(users, CreateJwt());

        var response = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = "wrong-password" });

        Assert.False(response.Success);
        Assert.Equal("Invalid credentials.", response.Message);
        Assert.Null(response.ApprovalStatus);
        Assert.Null(response.User);
    }

    private static FakeUserRepository WithUser(string role, string approvalStatus)
    {
        var users = new FakeUserRepository();
        users.Users.Add(new User
        {
            DisplayName = "Someone",
            Email = "who@example.com",
            PasswordHash = PasswordHasher.Hash(Password),
            Role = role,
            ApprovalStatus = approvalStatus
        });
        return users;
    }

    private static JwtHelper CreateJwt()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "012345678901234567890123456789012345678901234567",
            ["Jwt:Issuer"] = "test", ["Jwt:Audience"] = "test", ["Jwt:ExpiryMinutes"] = "60"
        }).Build();
        return new JwtHelper(config);
    }
}
