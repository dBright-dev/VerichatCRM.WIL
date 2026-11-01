using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Tests;

/// <summary>
/// The users table as the auth and approval services see it: a list, an
/// equality check on email, and an update counter. Mutating the <see cref="User"/>
/// an operation returned is visible to the next call, because the repository hands
/// out the object it holds rather than a copy - the same thing EF does inside a
/// request scope.
/// </summary>
internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];
    public int UpdateCount { get; private set; }

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> GetByIdAsync(string userId) =>
        Task.FromResult(Users.FirstOrDefault(u => u.UserId == userId));

    public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult<IEnumerable<User>>(Users);

    public Task<IEnumerable<User>> GetByApprovalStatusAsync(string approvalStatus) =>
        Task.FromResult(Users.Where(u => u.ApprovalStatus == approvalStatus).ToList().AsEnumerable());

    // Mirrors SQL's NULL-vs-value comparison: an account with no token outstanding
    // never matches, whatever the caller searches for.
    public Task<User?> GetByResetTokenAsync(string tokenHash) =>
        Task.FromResult(Users.FirstOrDefault(u => u.PasswordResetToken == tokenHash));

    public Task<User?> GetByVerificationTokenAsync(string tokenHash) =>
        Task.FromResult(Users.FirstOrDefault(u => u.EmailVerificationToken == tokenHash));

    public Task<bool> EmailExistsAsync(string email) =>
        Task.FromResult(Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<User> AddAsync(User user)
    {
        Users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        UpdateCount++;
        return Task.CompletedTask;
    }

    public Task<int> CountByRoleAsync(string role) => Task.FromResult(Users.Count(u => u.Role == role));
}
