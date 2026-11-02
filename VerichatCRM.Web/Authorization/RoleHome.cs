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
            // Kept identical to AccountController.RedirectToRoleDashboard: the two
            // used to disagree about where a Sales Rep's home is, and Logistics
            // pointed at the ticket queue.
            Roles.Logistics => "/logistics/dashboard",
            Roles.ClientUser or Roles.MedicalStudent => "/portal",
            _ => "/"
        };

        // Every audience in the product has a web surface now: the portal is the
        // Client User's and Medical Student's home, so a 403 on a staff screen must
        // not send them off to the mobile app as if the site had nothing for them.
        public static bool HasWebScreens(string? role) =>
            role is Roles.SystemAdministrator
                or Roles.SalesRepresentative
                or Roles.ProcurementManager
                or Roles.FieldSupportEngineer
                or Roles.Logistics
                or Roles.ClientUser
                or Roles.MedicalStudent;
    }
}
