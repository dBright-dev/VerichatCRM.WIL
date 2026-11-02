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
        /// web or mobile sign-up form.
        ///
        /// System Administrator is in this list because the role is meant to be
        /// claimable from the form, and it is listed in <see cref="ApprovalExempt"/>
        /// so the account it creates signs in without waiting for a reviewer.
        /// Read the consequence there before removing it from either list.
        /// </summary>
        public static readonly string[] SelfRegisterable =
        {
            SystemAdministrator,
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
        /// Roles whose self-registered account is written as Approved rather than
        /// Pending, so the holder can sign in immediately and never appears in
        /// /Admin/Approvals.
        ///
        /// The review gate exists because a claim on a public form is not evidence
        /// of the thing claimed. Exempting a role switches that check off for it:
        /// /Account/Register is anonymous, so anyone who reaches it can create an
        /// approved System Administrator and then read every client record, edit
        /// every staff account and approve or reject other people's applications.
        /// To put the gate back for this role, remove it from this array - new
        /// sign-ups queue again, and accounts already created keep their Approved
        /// status until someone changes it at /Admin/Approvals or in the database.
        /// </summary>
        public static readonly string[] ApprovalExempt =
        {
            SystemAdministrator
        };

        public static bool IsApprovalExempt(string? role) =>
            role != null && ApprovalExempt.Contains(role);


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
