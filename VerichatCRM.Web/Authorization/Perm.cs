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
        public const string AppointmentManagement = "appointments.manage";

        // The generic /Dashboard workspace screen. It is not anyone's landing page -
        // each staff role lands on its own dashboard - but it is a staff screen all
        // the same, and it was the one controller in the web tier that checked only
        // for a session. The API behind it refuses a client token, so what a Client
        // User or Medical Student got by typing the URL was an empty staff shell
        // titled after their own role. Every staff role holds this; neither client
        // role does.
        public const string Workspace = "workspace.dashboard";

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
        public const string TicketUpdate = "support.ticket.update";
        public const string WarehouseAlerts = "warehouse.alerts";

        // Administration
        public const string AdminAnalytics = "admin.analytics";
        public const string DashboardAnalytics = "dashboard.analytics";
        public const string AdminRules = "admin.rules";
        public const string RoleAssignments = "admin.roles";

        /// <summary>
        /// Reviewing an account that a person created for themselves. Sign-up is
        /// public, so the queue is where "I am a Procurement Manager" stops being a
        /// claim and starts being a permission - and where a hospital buyer is told
        /// they really are one. Held by the administrator and by the two roles that
        /// can tell: Sales meets the clients, Procurement meets the suppliers and
        /// the institutions. Ticket handlers and dispatchers read no identities, and
        /// the client audience cannot approve anyone - least of all itself.
        /// </summary>
        public const string AccountApprovals = "accounts.approve";

        // Client audience (the accounts that order rather than fulfil)
        public const string ClientPortal = "client.portal";
        public const string ClientQuoteRespond = "client.quote.respond";

        // Permissions granted to each role.
        public static readonly Dictionary<string, string[]> ByRole = new()
        {
            [Roles.SystemAdministrator] = new[]
            {
                AdminDashboard, ProcurementDashboard, SupportDashboard, SalesDashboard, Workspace, AppointmentManagement,
                ClientDirectory, ClientProfile, ClientCreate, ClientAssign,
                AssetTracking, LogisticsTracker,
                PipelineBoard, InquiryQueue, SalesLeads, SalesMap, QuotationBuilder, ProductCatalog, ProductCreate, ProductEdit,
                TicketHub, TicketUpdate, WarehouseAlerts,
                AdminAnalytics, DashboardAnalytics, AdminRules, RoleAssignments, AccountApprovals,
            },

            // Client directory + client details, asset tracking, pipeline board,
            // quotation builder and product catalog. Cannot create clients.
            [Roles.SalesRepresentative] = new[]
            {
                SalesDashboard, DashboardAnalytics, Workspace, AppointmentManagement,
                ClientDirectory, ClientProfile,
                AssetTracking,
                PipelineBoard, InquiryQueue, SalesLeads, SalesMap, QuotationBuilder, ProductCatalog, ProductEdit,
                AccountApprovals,
            },

            // Client directory, product catalog, ticket hub and logistics tracker,
            // monitored in real time. Warehouse alerts are theirs: the sidebar has
            // always offered it to them, but the permission was granted to nobody
            // except the administrator, so the link walked straight into a 403.
            [Roles.ProcurementManager] = new[]
            {
                ProcurementDashboard, Workspace, AppointmentManagement,
                ClientDirectory, ClientProfile,
                ProductCatalog,
                TicketHub, LogisticsTracker, WarehouseAlerts,
                AccountApprovals,
            },

            // Ticket hub (resolve + assign) and asset tracking.
            [Roles.FieldSupportEngineer] = new[]
            {
                SupportDashboard, Workspace,
                AssetTracking,
                TicketHub, TicketUpdate, AppointmentManagement,
            },

            // Delivery/logistics movements: tracker and asset movement.
            [Roles.Logistics] = new[]
            {
                SupportDashboard, Workspace,
                AssetTracking,
                LogisticsTracker,
                TicketHub,
            },

            // The client audience signs in on the web as well as on the phone. What
            // they get is the self-service portal, not a staff screen: catalogue,
            // their own tickets and their own quotations. Accepting or declining a
            // sent quotation is the one thing the two roles differ on, because the
            // API only lets a Client User answer - a training account can browse and
            // request, but it never signs the company up to a purchase.
            [Roles.ClientUser] = new[]
            {
                ClientPortal, ClientQuoteRespond,
            },

            [Roles.MedicalStudent] = new[]
            {
                ClientPortal,
            },
        };

        // Nullable because the sidebar asks before anyone is signed in, and a
        // missing UserRole is simply "no permissions" rather than an error.
        public static bool Has(string? role, string permission)
        {
            return ByRole.TryGetValue(role ?? string.Empty, out var perms)
                && perms.Contains(permission);
        }
    }
}
