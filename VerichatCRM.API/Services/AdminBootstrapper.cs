using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.Helpers;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// Creates the very first System Administrator when the user table is empty.
    /// <para>
    /// The application ships with NO demo/seed data - every client, product,
    /// ticket and quotation must be entered by an administrator through the UI.
    /// That leaves a bootstrap problem: an empty database has no way to sign in.
    /// This resolves it by reading the initial admin's credentials from
    /// configuration, so no password is ever hard-coded in source control.
    /// </para>
    /// <para>
    /// Configure with environment variables (preferred, keeps secrets out of
    /// appsettings files):
    /// <c>Bootstrap__AdminEmail</c>, <c>Bootstrap__AdminPassword</c>,
    /// <c>Bootstrap__AdminName</c>.
    /// </para>
    /// </summary>
    public static class AdminBootstrapper
    {
        public static async Task BootstrapAsync(
            AppDbContext context,
            IConfiguration configuration,
            ILogger logger)
        {
            // Only ever runs on a genuinely empty user table, so restarting the
            // app can never resurrect or overwrite an administrator.
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users already exist - skipping admin bootstrap.");
                return;
            }

            var email = configuration["Bootstrap:AdminEmail"]?.Trim();
            var password = configuration["Bootstrap:AdminPassword"];
            var displayName = configuration["Bootstrap:AdminName"]?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "No users exist and no bootstrap administrator is configured. " +
                    "Set Bootstrap__AdminEmail and Bootstrap__AdminPassword to create " +
                    "the first administrator, otherwise nobody can sign in.");
                return;
            }

            var user = new User
            {
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Administrator" : displayName,
                Email = email.ToLowerInvariant(),
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserRoles.SystemAdministrator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Created the initial System Administrator '{Email}'. " +
                "Change this password and remove the Bootstrap__* variables once signed in.",
                email);
        }
    }
}