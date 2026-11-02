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

    private static (FakeUserRepository Users, FakeMobileService Mobile, AuthService Service) Create()
    {
        var users = new FakeUserRepository();
        var mobile = new FakeMobileService();
        return (users, mobile, new AuthService(users, mobile, CreateJwt()));
    }

    [Fact]
    public async Task Register_Client_IsLiveImmediatelyAndGetsTheOrderingRow()
    {
        var (users, mobile, service) = Create();
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Client", Email = " CLIENT@Example.com ", Password = Password, Role = UserRoles.ClientUser
        });

        var stored = Assert.Single(users.Users);
        Assert.Equal("client@example.com", stored.Email);
        Assert.Equal(UserRoles.ClientUser, response.User!.Role);

        // Nothing sits between this form and a working account now, so the two things a client
        // used to receive only on approval - the live flag and the Clients row that orders,
        // quotations and enquiries are keyed on - are written here instead.
        Assert.True(response.Success);
        Assert.True(stored.IsActive);
        Assert.Equal(stored.UserId, Assert.Single(mobile.ClientAccountIdsCreated));
    }

    [Fact]
    public async Task Register_Student_IsLiveAndGetsTheOrderingRowToo()
    {
        var (users, mobile, service) = Create();
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Student", Email = "student@example.com", Password = Password, Role = UserRoles.MedicalStudent
        });

        Assert.True(response.Success);
        Assert.Equal(UserRoles.MedicalStudent, Assert.Single(users.Users).Role);
        Assert.Single(mobile.ClientAccountIdsCreated);
    }

    [Fact]
    public async Task Register_SystemAdministrator_IsRefusedAtTheDoor()
    {
        var (users, mobile, service) = Create();
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Ops", Email = "ops@example.com", Password = Password, Role = UserRoles.SystemAdministrator
        });

        // This form used to record the claim and leave it for a reviewer. With no reviewer,
        // refusing here is the only thing between a public page and a staff login, so the row
        // must not be written at all.
        Assert.False(response.Success);
        Assert.Empty(users.Users);
        Assert.Empty(mobile.ClientAccountIdsCreated);
    }

    [Fact]
    public async Task Register_InventedRole_IsRefusedToo()
    {
        var (users, mobile, service) = Create();
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Tourist", Email = "tourist@example.com", Password = Password, Role = "Chief Executive Officer"
        });

        Assert.False(response.Success);
        Assert.Empty(users.Users);
    }

    [Fact]
    public async Task Register_DoesNotHandOutASession()
    {
        var (users, mobile, service) = Create();
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Client", Email = "client@example.com", Password = Password, Role = UserRoles.ClientUser
        });

        // Deliberate: the sign-up screens hold the password the person just chose and exchange
        // it through the ordinary login, so one code path mints sessions.
        Assert.True(response.Success);
        Assert.Null(response.Token);
    }

    [Fact]
    public async Task Register_EmailAlreadyTaken_IsRefusedIgnoringCase()
    {
        var (users, mobile, service) = Create();
        await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Client", Email = "client@example.com", Password = Password, Role = UserRoles.ClientUser
        });

        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Other", Email = "CLIENT@EXAMPLE.COM", Password = Password, Role = UserRoles.ClientUser
        });

        Assert.False(response.Success);
        Assert.Single(users.Users);
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsAToken()
    {
        var users = WithUser(UserRoles.ClientUser);
        var service = new AuthService(users, new FakeMobileService(), CreateJwt());

        var response = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = Password });

        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_GivesTheSameSentenceAnUnknownAddressGets()
    {
        var users = WithUser(UserRoles.ClientUser);
        var service = new AuthService(users, new FakeMobileService(), CreateJwt());

        var wrongPassword = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = "wrong-password" });
        var unknownAddress = await service.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = Password });

        Assert.False(wrongPassword.Success);
        Assert.Equal(wrongPassword.Message, unknownAddress.Message);
        Assert.Null(wrongPassword.Token);
        Assert.Null(wrongPassword.User);
    }

    [Fact]
    public async Task Login_DeactivatedAccount_IsRefusedEvenWithTheRightPassword()
    {
        var users = WithUser(UserRoles.ClientUser);
        users.Users.Single().IsActive = false;
        var service = new AuthService(users, new FakeMobileService(), CreateJwt());

        var response = await service.LoginAsync(new LoginRequest { Email = "who@example.com", Password = Password });

        Assert.False(response.Success);
        Assert.Null(response.Token);
    }

    [Fact]
    public async Task CreateByAdmin_StaffRole_IsLiveAndSignsInFirstTime()
    {
        var (users, mobile, service) = Create();
        var response = await service.CreateAccountByAdminAsync(new RegisterRequest
        {
            DisplayName = "Thandi Nkosi", Email = "thandi@example.com", Password = Password, Role = UserRoles.SalesRepresentative
        });

        Assert.True(response.Success);
        var stored = Assert.Single(users.Users);
        Assert.True(stored.IsActive);
        Assert.Equal(UserRoles.SalesRepresentative, stored.Role);

        // A sales account is not a client, so it must not acquire a Clients row.
        Assert.Empty(mobile.ClientAccountIdsCreated);

        var signIn = await service.LoginAsync(new LoginRequest { Email = "thandi@example.com", Password = Password });
        Assert.True(signIn.Success);
        Assert.False(string.IsNullOrWhiteSpace(signIn.Token));
    }

    [Fact]
    public async Task CreateByAdmin_CanMintAnAdministrator_WhichThePublicFormCannot()
    {
        var (users, _, service) = Create();
        var response = await service.CreateAccountByAdminAsync(new RegisterRequest
        {
            DisplayName = "Second Admin", Email = "admin2@example.com", Password = Password, Role = UserRoles.SystemAdministrator
        });

        // The role a stranger may claim and the role an authenticated administrator
        // may hand out are different questions, and this is the only path that answers
        // the second one.
        Assert.True(response.Success);
        Assert.Equal(UserRoles.SystemAdministrator, Assert.Single(users.Users).Role);
    }

    [Fact]
    public async Task CreateByAdmin_ClientRole_GetsTheOrderingRowToo()
    {
        var (users, mobile, service) = Create();
        await service.CreateAccountByAdminAsync(new RegisterRequest
        {
            DisplayName = "Clinic", Email = "clinic@example.com", Password = Password, Role = UserRoles.ClientUser
        });

        var stored = Assert.Single(users.Users);
        Assert.Equal(stored.UserId, Assert.Single(mobile.ClientAccountIdsCreated));
    }

    [Fact]
    public async Task CreateByAdmin_EmailAlreadyTaken_IsRefused()
    {
        var (users, _, service) = Create();
        await service.CreateAccountByAdminAsync(new RegisterRequest
        {
            DisplayName = "First", Email = "dup@example.com", Password = Password, Role = UserRoles.Logistics
        });

        var response = await service.CreateAccountByAdminAsync(new RegisterRequest
        {
            DisplayName = "Second", Email = "DUP@example.com", Password = Password, Role = UserRoles.Logistics
        });

        Assert.False(response.Success);
        Assert.Single(users.Users);
    }

    [Fact]
    public async Task CreateByAdmin_InventedRole_IsRefused()
    {
        var (users, _, service) = Create();
        var response = await service.CreateAccountByAdminAsync(new RegisterRequest
        {
            DisplayName = "Tourist", Email = "tourist@example.com", Password = Password, Role = "Chief Executive Officer"
        });

        Assert.False(response.Success);
        Assert.Empty(users.Users);
    }

    private static FakeUserRepository WithUser(string role)
    {
        var users = new FakeUserRepository();
        users.Users.Add(new User
        {
            DisplayName = "Someone",
            Email = "who@example.com",
            PasswordHash = PasswordHasher.Hash(Password),
            Role = role
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
