using System;
using System.Collections.Generic;
using System.Linq;
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
        /// web or mobile sign-up form, and they are only the two client-facing ones.
        ///
        /// This list is the whole of the access control on the sign-up form. It used
        /// to be seven roles wide and backstopped by an employee approval queue;
        /// that queue is gone, so nothing reviews a claim any more - which means a
        /// claim must not be able to buy a staff login. A Sales Representative or
        /// Procurement Manager account is created by an employee at /Admin/Users, or
        /// by a role change there, and the API's registration service refuses
        /// anything not listed here.
        /// </summary>
        public static readonly string[] SelfRegisterable =
        {
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
