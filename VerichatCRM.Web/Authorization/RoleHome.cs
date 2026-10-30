namespace VerichatCRM.Web.Authorization
{
    // Used by the error pages so every dead end has a way back.
    public static class RoleHome
    {
        public static string UrlFor(string? role) => role switch
        {
            Roles.SystemAdministrator => "/admin/dashboard",
            Roles.SalesRepresentative => "/pipeline",
            Roles.ProcurementManager => "/procurement/dashboard",
            Roles.FieldSupportEngineer => "/support/dashboard",
            Roles.Logistics => "/support/dashboard",
            _ => "/"
        };

        public static bool HasWebScreens(string? role) =>
            role is Roles.SystemAdministrator
                or Roles.SalesRepresentative
                or Roles.ProcurementManager
                or Roles.FieldSupportEngineer
                or Roles.Logistics;
    }
}
