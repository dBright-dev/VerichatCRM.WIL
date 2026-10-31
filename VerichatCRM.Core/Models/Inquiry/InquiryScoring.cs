namespace VerichatCRM.Core.Models.Inquiry
{
    /// <summary>
    /// How soon the prospect needs the goods, as captured on the enquiry.
    /// </summary>
    public static class InquiryUrgencies
    {
        public const string Immediate = "Immediate";
        public const string High = "High";
        public const string Standard = "Standard";
        public const string Flexible = "Flexible";

        public static readonly string[] All = { Immediate, High, Standard, Flexible };
    }

    /// <summary>
    /// The qualification score an enquiry is given when the person entering it
    /// does not set one. Four weighted factors, 100 points in total:
    /// sector 25, contract status 20, estimated deal value 35, urgency 20.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a starting position for the queue, not a verdict: a score entered
    /// by hand is never overwritten (see InquiryService), and the queue is worked
    /// highest-score-first, so re-ranking an enquiry is done by editing its score.
    /// The value bands are in rand.
    /// </para>
    /// <para>
    /// Unknown sectors, contract statuses and urgencies score at the middle band
    /// rather than zero, because a free-text field that has not been filled in
    /// yet is not evidence of a cold lead.
    /// </para>
    /// </remarks>
    public static class InquiryScoring
    {
        public const int SectorWeight = 25;
        public const int ContractWeight = 20;
        public const int ValueWeight = 35;
        public const int UrgencyWeight = 20;

        /// <summary>
        /// Sectors ranked by how much recurring consumable business they tend to
        /// bring. Anything not listed - a new sector, a typo, blank - is treated
        /// as mid-tier.
        /// </summary>
        private static readonly Dictionary<string, double> SectorRanks =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Clinical"] = 1.0,
                ["Hospital"] = 1.0,
                ["Mining"] = 0.8,
                ["Manufacturing"] = 0.7,
                ["Pharmaceutical"] = 0.9,
                ["Waste Management"] = 0.5,
                ["Logistics"] = 0.5,
                ["Retail"] = 0.4,
                ["Education"] = 0.3,
            };

        private const double UnknownSectorRank = 0.6;

        public static int Calculate(
            string? sector,
            string? contractStatus,
            decimal? estimatedValue,
            string? urgency)
        {
            var score = 0;
            score += SectorPoints(sector);
            score += ContractPoints(contractStatus);
            score += ValuePoints(estimatedValue);
            score += UrgencyPoints(urgency);

            // A prospect with nothing filled in still has to land somewhere in
            // the queue, and the bands can only ever return whole points.
            return Math.Clamp(score, 0, 100);
        }

        internal static int SectorPoints(string? sector)
        {
            var rank = sector is null || !SectorRanks.TryGetValue(sector.Trim(), out var found)
                ? UnknownSectorRank
                : found;

            return (int)Math.Round(SectorWeight * rank, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// An existing contract is worth more than a cold prospect: the account is
        /// already set up, the buying relationship exists, and the paperwork is
        /// done. "Inactive" is deliberately below "Prospect" - reactivating a
        /// lapsed account has to win back trust a new lead does not need.
        /// </summary>
        internal static int ContractPoints(string? contractStatus) => contractStatus?.Trim().ToLowerInvariant() switch
        {
            "active" => ContractWeight,
            "prospect" => (int)Math.Round(ContractWeight * 0.6, MidpointRounding.AwayFromZero),
            "inactive" => (int)Math.Round(ContractWeight * 0.3, MidpointRounding.AwayFromZero),
            _ => (int)Math.Round(ContractWeight * 0.5, MidpointRounding.AwayFromZero),
        };

        internal static int ValuePoints(decimal? estimatedValue)
        {
            if (estimatedValue is null || estimatedValue <= 0m)
                return (int)Math.Round(ValueWeight * 0.3, MidpointRounding.AwayFromZero);

            return estimatedValue switch
            {
                >= 500_000m => ValueWeight,
                >= 150_000m => (int)Math.Round(ValueWeight * 0.8, MidpointRounding.AwayFromZero),
                >= 50_000m => (int)Math.Round(ValueWeight * 0.6, MidpointRounding.AwayFromZero),
                >= 10_000m => (int)Math.Round(ValueWeight * 0.4, MidpointRounding.AwayFromZero),
                _ => (int)Math.Round(ValueWeight * 0.2, MidpointRounding.AwayFromZero),
            };
        }

        internal static int UrgencyPoints(string? urgency) => urgency?.Trim().ToLowerInvariant() switch
        {
            "immediate" => UrgencyWeight,
            "high" => (int)Math.Round(UrgencyWeight * 0.75, MidpointRounding.AwayFromZero),
            "standard" => (int)Math.Round(UrgencyWeight * 0.5, MidpointRounding.AwayFromZero),
            "flexible" => (int)Math.Round(UrgencyWeight * 0.25, MidpointRounding.AwayFromZero),
            _ => (int)Math.Round(UrgencyWeight * 0.5, MidpointRounding.AwayFromZero),
        };
    }
}
