namespace VerichatCRM.Web.Authorization
{
    /// <summary>
    /// Screen-level permissions. Every protected controller action declares one
    /// of these so access is enforced on the server - hiding a sidebar link is
    /// not access control, because the URL can still be typed directly.
    /// </summary>
    public static class Perm
    {
        // Role landing dashboards
        public const string AdminDashboard = "admin.dashboard";
        public const string ProcurementDashboard = "procurement.dashboard";
        public const string SupportDashboard = "support.dashboard";
        public const string SalesDashboard = "sales.dashboard";

        // Clients
        public const string ClientAssign = "clients.assign";
        public const string ClientDirectory = "clients.directory";
        public const string ClientProfile = "clients.profile";
        public const string ClientCreate = "clients.create";

        // Assets & logistics
        public const string AssetTracking = "assets.tracking";
        public const string LogisticsTracker = "logistics.tracker";

        // Sales workflow
        public const string SalesMap = "sales.map";
        public const string SalesLeads = "sales.leads";
        public const string PipelineBoard = "pipeline.board";
        public const string InquiryQueue = "inquiries.queue";
        public const string QuotationBuilder = "quotation.builder";
        public const string ProductCatalog = "products.catalog";
        public const string ProductCreate = "products.create";
        public const string ProductEdit = "products.edit";

        // Support & operations
        public const string TicketHub = "tickets.hub";
        public const string WarehouseAlerts = "warehouse.alerts";

        // Administration
        public const string AdminAnalytics = "admin.analytics";
        public const string AdminRules = "admin.rules";
        public const string RoleAssignments = "admin.roles";

        /// <summary>Permissions granted to each role.</summary>
        public static readonly Dictionary<string, string[]> ByRole = new()
        {
            [Roles.SystemAdministrator] = new[]
            {
                AdminDashboard, ProcurementDashboard, SupportDashboard, SalesDashboard,
                ClientDirectory, ClientProfile, ClientCreate, ClientAssign,
                AssetTracking, LogisticsTracker,
                PipelineBoard, InquiryQueue, SalesLeads, SalesMap, QuotationBuilder, ProductCatalog, ProductCreate, ProductEdit,
                TicketHub, WarehouseAlerts,
                AdminAnalytics, AdminRules, RoleAssignments,
            },

            // Client directory + client details, asset tracking, pipeline board,
            // quotation builder and product catalog. Cannot create clients.
            [Roles.SalesRepresentative] = new[]
            {
                SalesDashboard,
                ClientDirectory, ClientProfile,
                AssetTracking,
                PipelineBoard, InquiryQueue, SalesLeads, SalesMap, QuotationBuilder, ProductCatalog, ProductEdit,
            },

            // Client directory, product catalog, ticket hub and logistics tracker,
            // monitored in real time.
            [Roles.ProcurementManager] = new[]
            {
                ProcurementDashboard,
                ClientDirectory, ClientProfile,
                ProductCatalog,
                TicketHub, LogisticsTracker,
            },

            // Ticket hub (resolve + assign) and asset tracking.
            [Roles.FieldSupportEngineer] = new[]
            {
                SupportDashboard,
                AssetTracking,
                TicketHub,
            },

            // Delivery/logistics movements: tracker and asset movement.
            [Roles.Logistics] = new[]
            {
                SupportDashboard,
                AssetTracking,
                LogisticsTracker,
                TicketHub,
            },

            // Client accounts sign in through the mobile app; they have no staff
            // screens in the web application.
            [Roles.ClientUser] = Array.Empty<string>(),

            [Roles.MedicalStudent] = Array.Empty<string>(),
        };

        public static bool Has(string role, string permission)
        {
            return ByRole.TryGetValue(role ?? string.Empty, out var perms)
                && perms.Contains(permission);
        }
    }
}