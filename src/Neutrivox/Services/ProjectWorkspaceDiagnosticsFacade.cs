using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record WorkspaceDiagnosticsSnapshot(
    UnifiedWorkspaceState State,
    IReadOnlyList<ValidationIssue> Issues,
    string Summary);

/// <summary>
/// Single facade used by a future workspace screen to refresh all project health information at once.
/// </summary>
public sealed class ProjectWorkspaceDiagnosticsFacade
{
    private readonly UnifiedProjectWorkspaceService _workspace;
    private readonly ProjectValidationService _validation;

    public ProjectWorkspaceDiagnosticsFacade(DeviceCatalogService catalog)
    {
        Catalog = catalog;
        _validation = new ProjectValidationService();
        _workspace = new UnifiedProjectWorkspaceService(_validation);
    }

    public DeviceCatalogService Catalog { get; }

    public WorkspaceDiagnosticsSnapshot Refresh(AutomationProject project)
    {
        var state = _workspace.BuildState(project, Catalog);
        var validation = _validation.Validate(project, Catalog);
        return new(state, validation.Issues, _workspace.BuildHumanReadableSummary(project));
    }
}
