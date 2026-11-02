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
    /// <strong>One setting decides whether anything is written.</strong>
    /// <c>Bootstrap:DemoUsers</c> has to read <c>true</c>, and that is the whole rule.
    /// It used to demand the Development environment as well, which made the shortcut
    /// impossible anywhere a reviewer actually goes - a published host is not in
    /// Development, so the sign-in panel and the accounts behind it could never both
    /// exist off a developer's machine. The key is now on in the checked-in
    /// <c>appsettings.json</c>, so a publish carries the accounts with it, and the web
    /// tier reads the same key name to decide whether to render the buttons - see
    /// <c>VerichatCRM.Web.Services.DemoAccess</c>. A host that must not be open wins
    /// the argument: a <c>Bootstrap__DemoUsers=false</c> app setting outranks the file.
    /// </para>
    /// <para>
    /// Setting it is a deliberate act, because it publishes these credentials to
    /// everyone who can reach the host: the passwords are committed on purpose (a
    /// credential nobody can reproduce is a locked door, not a test account), one of
    /// these accounts is a System Administrator, and the addresses end in
    /// <c>.test.local</c> only so that a mail or reset addressed to them can never
    /// reach a real mailbox. Turn it on for a demonstration host and off again when
    /// the demonstration is over.
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
            ILogger logger)
        {
            if (!string.Equals(configuration["Bootstrap:DemoUsers"], "true",
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
                    "Seeded {Count} demo sign-in accounts, one per role, including {Admin}. " +
                    "They exist because Bootstrap:DemoUsers=true on this host. If this is a " +
                    "deployed system those credentials are now live for anyone who can reach it, " +
                    "so unset the setting when the demonstration is over.",
                    created, UserRoles.SystemAdministrator);
        }
    }
}
