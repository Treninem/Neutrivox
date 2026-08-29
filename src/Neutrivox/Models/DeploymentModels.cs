namespace Neutrivox.Models;

public enum DeploymentState
{
    Draft,
    Validated,
    ReadyForConfirmation,
    Confirmed,
    Completed,
    Failed,
    Cancelled
}

public sealed class DeploymentTarget
{
    public int Order { get; init; }
    public Guid ProjectDeviceId { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string DefinitionId { get; init; } = string.Empty;
    public string? Endpoint { get; init; }
    public string IdentificationState { get; init; } = "Unverified";
}

public sealed class DeploymentPlan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ProjectId { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DeploymentState State { get; set; } = DeploymentState.Draft;
    public List<DeploymentTarget> Targets { get; } = [];
    public List<string> ValidationMessages { get; } = [];
    public string? PlanFingerprint { get; set; }
    public string? ConfirmedByUser { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
}

public sealed record DeploymentPreview(
    DeploymentPlan Plan,
    IReadOnlyList<ProjectReadinessItem> ReadinessItems,
    bool RequiresUserConfirmation,
    string Summary);
