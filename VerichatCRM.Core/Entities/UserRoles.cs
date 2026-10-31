using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public static class UserRoles
    {
        public const string SystemAdministrator = "System Administrator";
        public const string SalesRepresentative = "Sales Representative";
        public const string ProcurementManager = "Procurement Manager";
        public const string FieldSupportEngineer = "Field Support Engineer";
        public const string Logistics = "Logistics";
        public const string MedicalStudent = "Medical Student";
        public const string ClientUser = "Client User";

        public static readonly string[] All =
        {
            SystemAdministrator,
            SalesRepresentative,
            ProcurementManager,
            FieldSupportEngineer,
            Logistics,
            MedicalStudent,
            ClientUser
        };

        /// <summary>
        /// Roles a person may choose for themselves when self-registering on the
        /// web or mobile sign-up form. System Administrator is deliberately
        /// excluded: an administrator account must be created by an existing
        /// administrator (via Users &amp; Roles) or by the one-time
        /// configuration bootstrap, never claimed from a public sign-up form.
        /// </summary>
        public static readonly string[] SelfRegisterable =
        {
            SalesRepresentative,
            ProcurementManager,
            FieldSupportEngineer,
            Logistics,
            ClientUser,
            // Students sign themselves up to use the mobile catalogue and quick
            // cart. They get no staff screens, so the risk this would otherwise
            // open - claiming a role with permissions from a public form - is nil.
            MedicalStudent
        };

        /// <summary>
        /// Roles whose holder orders consumables through the mobile app. Both of
        /// them need a row in Clients, because orders, quotations and the enquiry
        /// queue are keyed on Clients.ClientId - and that row uses the UserId as
        /// its key, so it has to be created whenever an account becomes one of
        /// these roles, not only at mobile sign-up.
        /// </summary>
        public static readonly string[] ClientAudience =
        {
            ClientUser,
            MedicalStudent
        };
    }
}
