using System;
using System.Collections.Generic;

namespace VerichatCRM.Core.Entities
{
    /// <summary>
    /// The product catalogue's segmentation. Kept here because the API validates
    /// inbound values, the web app renders the pick-list, and the seed data has
    /// to agree - the same five strings were previously duplicated in three
    /// places with no single owner.
    /// </summary>
    public static class ProductCategories
    {
        public const string PPE = "PPE";
        public const string Laboratory = "Laboratory";
        public const string HospitalInfrastructure = "Hospital Infrastructure";
        public const string HomeCare = "Home Care";
        public const string InfantCare = "Infant Care";

        public static readonly string[] All =
        {
            PPE,
            Laboratory,
            HospitalInfrastructure,
            HomeCare,
            InfantCare
        };

        private static readonly HashSet<string> Lookup =
            new(All, StringComparer.OrdinalIgnoreCase);

        /// <summary>Case-insensitive membership test used by the validators.</summary>
        public static bool IsValid(string? category) => category != null && Lookup.Contains(category);
    }
}
