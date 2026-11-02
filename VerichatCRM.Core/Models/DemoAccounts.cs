using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Models
{
    /// <summary>
    /// One development sign-in account, and the one-click button that reaches it.
    /// </summary>
    /// <param name="Code">The value a sign-in form posts to name this account. It is
    /// never an email address, so a demo button cannot be turned into a probe for
    /// which addresses are registered.</param>
    /// <param name="Label">What the button says. The role names are too long for a
    /// chip, and the full name stays available as the tooltip.</param>
    /// <param name="Landing">Caption only - the board this role signs in to. The
    /// redirect itself is decided by <c>AccountController.RedirectToRoleDashboard</c>
    /// from the role the API returns, so changing that mapping does not change where
    /// the button actually goes; it only makes this label wrong.</param>
    public sealed record DemoAccount(
        string Code,
        string Label,
        string Role,
        string DisplayName,
        string Email,
        string Password,
        string Landing);

    /// <summary>
    /// The seven accounts <c>DemoUserSeeder</c> writes, listed once so the seeder and
    /// the sign-in screens cannot drift apart. A button that names an address the
    /// seeder did not write is a button that fails, and the only way to notice is to
    /// click it.
    /// <para>
    /// These are development credentials and nothing more. The seeder writes them only
    /// while the API runs in Development with <c>Bootstrap:DemoUsers=true</c>, and the
    /// web and mobile clients show the buttons only in Development too, so a deployed
    /// system has neither the accounts nor the shortcuts. They are committed on
    /// purpose: a credential nobody can reproduce is not a test account, it is a
    /// locked door.
    /// </para>
    /// <para>
    /// Every address ends in .test.local, which is not a routable domain. These
    /// accounts receive a confirmation mail and can be pointed at by a password
    /// reset, so the suffix is what guarantees neither can ever reach a real
    /// mailbox - and what stops a demo row in the client directory reading like
    /// a customer Verichat signed.
    /// </para>
    /// </summary>
    public static class DemoAccounts
    {
        public static readonly IReadOnlyList<DemoAccount> All = new List<DemoAccount>
        {
            new("admin",       "Admin",       UserRoles.SystemAdministrator,  "Dumisani Ncube", "admin@verichat.test.local",       "Demo!Admin2026",  "/Admin/Dashboard"),
            new("sales",       "Sales Rep",   UserRoles.SalesRepresentative,  "Chipiya Mwanza", "sales@verichat.test.local",       "Demo!Sales2026",  "/Pipeline"),
            new("procurement", "Procurement", UserRoles.ProcurementManager,   "Naledi Mokoena", "procurement@verichat.test.local", "Demo!Stock2026",  "/Procurement/Dashboard"),
            new("engineer",    "Support",     UserRoles.FieldSupportEngineer, "Sipho Dlamini",  "engineer@verichat.test.local",    "Demo!Fix2026",    "/Support/Dashboard"),
            new("logistics",   "Logistics",   UserRoles.Logistics,            "Thabo Rasheed",  "logistics@verichat.test.local",   "Demo!Move2026",   "/Logistics/Dashboard"),
            new("student",     "Student",     UserRoles.MedicalStudent,       "Keerthi Naidoo", "student@verichat.test.local",     "Demo!Study2026",  "/Portal"),
            new("client",      "Client",      UserRoles.ClientUser,           "Reneé Fisher",   "client@verichat.test.local",      "Demo!Client2026", "/Portal"),
        };

        public static DemoAccount? Find(string? code) =>
            string.IsNullOrWhiteSpace(code)
                ? null
                : All.FirstOrDefault(a => a.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }
}
