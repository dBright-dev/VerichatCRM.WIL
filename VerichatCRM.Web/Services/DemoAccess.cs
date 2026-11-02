namespace VerichatCRM.Web.Services
{
    /// <summary>
    /// The one rule that decides whether this web host offers the one-click demo
    /// sign-in, shared by the partial that renders the buttons and the controller
    /// that resolves the code they post.
    /// <para>
    /// It used to be <c>IWebHostEnvironment.IsDevelopment()</c> at both sites, which
    /// meant a published reviewer could not use the panel at all. The rule is now an
    /// opt-in setting, and it is deliberately the <em>same</em> key the API reads
    /// (<c>Bootstrap:DemoUsers</c>, see <c>DemoUserSeeder</c>): one setting name turns
    /// both tiers, so a host cannot reach the state this project would otherwise ship
    /// with - buttons on the sign-in page and no accounts behind them.
    /// </para>
    /// <para>
    /// Development stays implicitly on so a local clone needs no setting. Deployed runs
    /// get it from <c>appsettings.json</c>, where the key is set true because this build
    /// is submitted for assessment and the marker signs in from the published site; an
    /// app setting of <c>Bootstrap__DemoUsers=false</c> on the host outranks the file.
    /// Nothing here is secret either way: the switch only reveals accounts whose
    /// addresses and passwords are already printed in README.md.
    /// </para>
    /// </summary>
    public static class DemoAccess
    {
        public const string SettingKey = "Bootstrap:DemoUsers";

        public static bool IsEnabled(IConfiguration configuration, IWebHostEnvironment environment) =>
            environment.IsDevelopment()
            || string.Equals(configuration[SettingKey], "true", StringComparison.OrdinalIgnoreCase);
    }
}
