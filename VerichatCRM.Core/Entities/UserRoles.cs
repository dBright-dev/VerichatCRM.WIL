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
        public const string MedicalStudent = "Medical Student";
        public const string ClientUser = "Client User";

        public static readonly string[] All =
        {
            SystemAdministrator,
            SalesRepresentative,
            ProcurementManager,
            FieldSupportEngineer,
            MedicalStudent,
            ClientUser
        };
    }
}
