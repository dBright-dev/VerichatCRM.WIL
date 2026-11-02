namespace VerichatCRM.Web.ViewModels.Home;

public class HomeViewModel
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }

    public IReadOnlyList<string> Categories { get; init; } = LandingContent.Categories;
    public IReadOnlyList<ShowcaseItem> Showcase { get; init; } = LandingContent.Showcase;
    public IReadOnlyList<WorkflowStep> Workflow { get; init; } = LandingContent.Workflow;
    public IReadOnlyList<SectorCard> Sectors { get; init; } = LandingContent.Sectors;
    public IReadOnlyList<TeamMember> Team { get; init; } = LandingContent.Team;
}
