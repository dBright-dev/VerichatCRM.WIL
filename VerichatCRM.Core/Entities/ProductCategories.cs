using System;
using System.Collections.Generic;

namespace VerichatCRM.Core.Entities
{
    /// <summary>
    /// The product catalogue's segmentation. Kept here because the API validates
    /// inbound values, the web app renders the pick-list, and the seed data has
    /// to agree - the same five strings were previously duplicated in three
    /// places with no single owner.
    /// <para>
    /// Membership is not cosmetic: <c>CreateProductRequestValidator</c> rejects any
    /// category outside this list, so a value invented in a seed script or typed into
    /// the form makes the row uneditable from the UI. The three later additions exist
    /// because the published site sells into mining and rescue and into teaching
    /// laboratories, and the original five had no name for either - which is how the
    /// free-text values <c>Clinical</c>, <c>Rehab</c> and <c>Education</c> crept in.
    /// Adding one here means adding it to the pick-list and the seed together.
    /// </para>
    /// </summary>
    public static class ProductCategories
    {
        public const string PPE = "PPE";
        public const string Laboratory = "Laboratory";
        public const string HospitalInfrastructure = "Hospital Infrastructure";
        public const string HomeCare = "Home Care";
        public const string InfantCare = "Infant Care";
        public const string ClinicalConsumables = "Clinical Consumables";
        public const string MiningIndustrial = "Mining & Industrial";
        public const string TeachingSimulation = "Teaching & Simulation";

        public static readonly string[] All =
        {
            PPE,
            Laboratory,
            HospitalInfrastructure,
            HomeCare,
            InfantCare,
            ClinicalConsumables,
            MiningIndustrial,
            TeachingSimulation
        };

        private static readonly HashSet<string> Lookup =
            new(All, StringComparer.OrdinalIgnoreCase);

        /// <summary>Case-insensitive membership test used by the validators.</summary>
        public static bool IsValid(string? category) => category != null && Lookup.Contains(category);
    }
}
