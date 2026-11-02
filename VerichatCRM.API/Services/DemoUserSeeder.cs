using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Models;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// Creates one signable-in account per role, so every screen the system has can
    /// actually be opened.
    /// <para>
    /// Roles here are not decoration: each one reaches a different board, a
    /// different slice of the client directory and a different set of buttons, and
    /// two of them reach only the mobile app. A reviewer cannot judge any of that
    /// from a role list, and the sign-up form makes it worse - it now opens only the
    /// two client-facing roles, so the ordinary way to obtain a Sales Representative
    /// login is an administrator creating it, which on a fresh machine nobody is.
    /// This closes that gap by seeding the accounts rather than by widening the form.
    /// </para>
    /// <para>
    /// <strong>Two independent conditions must both hold before anything is
    /// written.</strong> The host has to be running in the Development environment,
    /// and <c>Bootstrap:DemoUsers</c> has to read <c>true</c>. The key is set only in
    /// appsettings.Development.json, so a deployment would have to be handed both
    /// that file and <c>ASPNETCORE_ENVIRONMENT=Development</c> before a single row
    /// appeared. The passwords below are therefore committed on
    /// purpose: they belong to accounts that cannot exist on a server, and a
    /// credential nobody can reproduce is not a test account, it is a locked door.
    /// </para>
    /// </summary>
    public static class DemoUserSeeder
    {
        /// <summary>
        /// The list lives in <see cref="DemoAccounts"/> because the sign-in screens
        /// advertise these accounts with one-click buttons, and a button that names an
        /// address this seeder did not write is a button that fails. See that file for
        /// why the addresses end in .test.local.
        /// </summary>
        private static readonly IReadOnlyList<DemoAccount> Accounts = DemoAccounts.All;

        public static async Task SeedAsync(
            AppDbContext context,
            IMobileService mobile,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger logger)
        {
            if (!environment.IsDevelopment()
                || !string.Equals(configuration["Bootstrap:DemoUsers"], "true",
                                   StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var created = 0;
            foreach (var account in Accounts)
            {
                var email = account.Email.ToLowerInvariant();

                // Idempotent by address, and only ever adds. An account a developer
                // has since promoted, locked or re-passworded is theirs, not this
                // seeder's to take back on the next restart.
                if (await context.Users.AnyAsync(u => u.Email == email)) continue;

                context.Users.Add(new User
                {
                    Email = email,
                    DisplayName = account.DisplayName,
                    PasswordHash = PasswordHasher.Hash(account.Password),
                    Role = account.Role,
                    IsActive = true,

                    // Verified at birth, for the reason AdminBootstrapper gives: the
                    // flag exists to distrust the public sign-up form, and these rows
                    // did not come from it.
                    EmailVerified = true,
                });
                await context.SaveChangesAsync();
                created++;

                // A client-role account is only useful once it has the CRM record
                // that orders, quotations and the enquiry queue are keyed on, which is
                // the same call the registration service makes for a signed-up client.
                // Without it the demo client opens the app to an empty portal.
                if (UserRoles.ClientAudience.Contains(account.Role))
                {
                    var user = await context.Users.FirstAsync(u => u.Email == email);
                    await mobile.EnsureClientAccountAsync(user.UserId);
                }
            }

            if (created > 0)
                logger.LogWarning(
                    "Seeded {Count} development sign-in accounts, one per role, including {Admin}. " +
                    "These exist only while Bootstrap:DemoUsers=true in the Development environment - " +
                    "they are not credentials for any deployed system.",
                    created, UserRoles.SystemAdministrator);
        }
    }
}
