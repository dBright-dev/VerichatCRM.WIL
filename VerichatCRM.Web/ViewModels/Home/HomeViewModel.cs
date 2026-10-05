namespace VerichatCRM.Web.ViewModels.Home
{
    public class HomeViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public List<QuickAction> QuickActions { get; set; } = new();
        public List<RoleCard> Roles { get; set; } = new();
    }
}
