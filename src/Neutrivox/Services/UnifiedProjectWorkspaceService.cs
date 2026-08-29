using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record UnifiedWorkspaceState(
    string ProjectName,
    int DeviceCount,
    int ChannelCount,
    int ConnectionCount,
    int LogicNetworkCount,
    int LogicInstructionCount,
    int ValidationErrorCount,
    int ValidationWarningCount,
    bool HasPhysicalBindings,
    bool HasSimulationData);

/// <summary>
/// Provides one read model for the main workspace. It deliberately combines existing services
/// instead of keeping a second copy of project state in the UI.
/// </summary>
public sealed class UnifiedProjectWorkspaceService
{
    private readonly ProjectSummaryService _summary = new();
    private readonly LogicEditorWorkflowService _logic = new();
    private readonly ProjectValidationService _validation;

    public UnifiedProjectWorkspaceService(ProjectValidationService validation)
    {
        _validation = validation;
    }

    public UnifiedWorkspaceState BuildState(AutomationProject project, DeviceCatalogService catalog)
    {
        var validation = _validation.Validate(project, catalog);
        return new UnifiedWorkspaceState(
            project.Name,
            project.Devices.Count,
            project.Devices.Sum(x => x.Channels.Count),
            project.Connections.Count,
            project.Logic.Networks.Count,
            project.Logic.Networks.Sum(x => x.Instructions.Count),
            validation.Issues.Count(x => x.Severity == Models.ValidationSeverity.Error),
            validation.Issues.Count(x => x.Severity == Models.ValidationSeverity.Warning),
            project.Devices.Any(x => x.PhysicalBinding is not null),
            project.Logic.Networks.Any(x => x.Instructions.Count > 0));
    }

    public string BuildHumanReadableSummary(AutomationProject project)
        => _summary.CreateHumanReadableSummary(project);

    public IReadOnlyList<LogicBlockDefinition> GetLogicToolbox() => _logic.Toolbox;
}
