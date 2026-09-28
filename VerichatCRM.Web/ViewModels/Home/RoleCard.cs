namespace VerichatCRM.Web.ViewModels.Home
{
    public class RoleCard
    {
        public string Name { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string DashboardUrl { get; set; } = string.Empty;
    }
}
