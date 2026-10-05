namespace VerichatCRM.Web.Authorization
{
    /// <summary>
    /// Web-side role names. These must match the strings persisted in
    /// <c>Users.Role</c> by the API, because the web app reads the role from the
    /// session after the API issues the JWT.
    /// </summary>
    public static class Roles
    {
        public const string SystemAdministrator = "System Administrator";
        public const string SalesRepresentative = "Sales Representative";
        public const string ProcurementManager = "Procurement Manager";
        public const string FieldSupportEngineer = "Field Support Engineer";
        public const string Logistics = "Logistics";
        public const string ClientUser = "Client User";
        public const string MedicalStudent = "Medical Student";

        /// <summary>Roles a user may pick when self-registering on the web.</summary>
        public static readonly string[] SelfRegisterable =
        {
            SalesRepresentative,
            ProcurementManager,
            FieldSupportEngineer,
            Logistics,
            ClientUser,
        };
    }
}