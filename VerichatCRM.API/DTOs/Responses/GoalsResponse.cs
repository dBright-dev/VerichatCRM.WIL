using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    public class GoalsResponse
    {
        [JsonPropertyName("goals")]
        public List<GoalItemResponse> Goals { get; set; } = new();

        [JsonPropertyName("weeklyExercise")]
        public WeeklyExerciseResponse? WeeklyExercise { get; set; }
    }

    public class GoalItemResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("current")]
        public double Current { get; set; }

        [JsonPropertyName("target")]
        public double Target { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;
    }

    public class WeeklyExerciseResponse
    {
        [JsonPropertyName("targetPerDayMin")]
        public int TargetPerDayMin { get; set; } = 30;

        [JsonPropertyName("days")]
        public List<WeeklyExerciseDayResponse> Days { get; set; } = new();

        [JsonPropertyName("todayIndex")]
        public int? TodayIndex { get; set; }
    }

    public class WeeklyExerciseDayResponse
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }
    }
}
