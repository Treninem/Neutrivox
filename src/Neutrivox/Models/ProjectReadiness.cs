namespace Neutrivox.Models;

public enum ProjectReadinessLevel
{
    Information,
    Warning,
    Blocking
}

public sealed record ProjectReadinessItem(
    ProjectReadinessLevel Level,
    string Code,
    string Title,
    string Description,
    string SuggestedAction);

public sealed class ProjectReadinessReport
{
    public List<ProjectReadinessItem> Items { get; } = [];
    public bool CanSimulate => Items.All(x => x.Level != ProjectReadinessLevel.Blocking);
    public bool CanPrepareDeployment => CanSimulate && Items.All(x => x.Code != "NO_DEVICES");
    public int BlockingCount => Items.Count(x => x.Level == ProjectReadinessLevel.Blocking);
    public int WarningCount => Items.Count(x => x.Level == ProjectReadinessLevel.Warning);
}
