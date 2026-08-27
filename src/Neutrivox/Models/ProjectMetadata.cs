namespace Neutrivox.Models;

public sealed class ProjectMetadata
{
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0";
}

public enum ProjectStatus
{
    Draft,
    ReadyForReview,
    Validated
}
