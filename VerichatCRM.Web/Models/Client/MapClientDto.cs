namespace VerichatCRM.Web.Models.Client
{
    public class MapClientDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Sector { get; set; } = "Other";
        public string Status { get; set; } = "";
        public string? AssignedRep { get; set; }
        public DateTime? LastContactDate { get; set; }

        // Computed here so the browser doesn't repeat the follow-up rule.
        public bool HasLocation => Lat.HasValue && Lng.HasValue;
        public string LastContactText => FollowUpHelper.Describe(LastContactDate);
        public bool IsStale => FollowUpHelper.IsStale(LastContactDate);
    }

    // Maps free-text client sectors onto the four map pin types.
    public static class MapSector
    {
        public static string Normalize(string? raw)
        {
            var s = (raw ?? "").Trim().ToLowerInvariant();
            if (s.Contains("hospital")) return "Hospital";
            if (s.Contains("clinic")) return "Clinic"; // also matches "clinical"
            if (s.Contains("mine") || s.Contains("mining")) return "Mine";
            if (s.Contains("educ") || s.Contains("school") || s.Contains("universit") || s.Contains("college"))
                return "Education";
            return "Other";
        }
    }
}