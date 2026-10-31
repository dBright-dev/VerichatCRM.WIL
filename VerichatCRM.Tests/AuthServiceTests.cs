using Microsoft.Extensions.Configuration;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Services;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using Xunit;

namespace VerichatCRM.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_RefusesPublicAdministratorRole()
    {
        var users = new FakeUserRepository();
        var service = new AuthService(users, CreateJwt());
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Client", Email = "client@example.com", Password = "password1", Role = UserRoles.SystemAdministrator
        });
        Assert.False(response.Success);
        Assert.Empty(users.Users);
    }

    [Fact]
    public async Task Register_NormalizesEmailAndPersistsClientRole()
    {
        var users = new FakeUserRepository();
        var service = new AuthService(users, CreateJwt());
        var response = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Client", Email = " CLIENT@Example.com ", Password = "password1", Role = UserRoles.ClientUser
        });
        Assert.True(response.Success);
        Assert.Equal("client@example.com", Assert.Single(users.Users).Email);
        Assert.Equal(UserRoles.ClientUser, response.User!.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
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

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult(Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        public Task<User?> GetByIdAsync(string userId) => Task.FromResult(Users.FirstOrDefault(u => u.UserId == userId));
        public Task<bool> EmailExistsAsync(string email) => Task.FromResult(Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        public Task<User> AddAsync(User user) { Users.Add(user); return Task.FromResult(user); }
        public Task UpdateAsync(User user) => Task.CompletedTask;
        public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult<IEnumerable<User>>(Users);
        public Task<User?> GetByResetTokenAsync(string token) => Task.FromResult<User?>(null);
        public Task<int> CountByRoleAsync(string role) => Task.FromResult(0);
    }
}
