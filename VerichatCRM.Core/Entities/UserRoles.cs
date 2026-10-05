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
            ClientUser
        };
    }
}
