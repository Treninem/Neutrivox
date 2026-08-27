using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Coordinates the single project workflow used for offline design, simulation and physical deployment.</summary>
public sealed class ProjectWorkspaceService
{
    private readonly ProjectEquipmentService _equipment;
    private readonly ProjectValidationService _validation;

    public ProjectWorkspaceService(ProjectEquipmentService equipment, ProjectValidationService validation)
    {
        _equipment = equipment;
        _validation = validation;
    }

    public ProjectWorkspaceSnapshot GetSnapshot(AutomationProject project)
    {
        var validation = _validation.Validate(project);
        return new ProjectWorkspaceSnapshot(
            project.Id,
            project.Name,
            project.Devices.Count,
            project.Devices.Sum(d => d.Channels.Count),
            validation.Results.Count(r => r.Level == ValidationLevel.Error),
            validation.Results.Count(r => r.Level == ValidationLevel.Warning));
    }

    public WorkspaceReadiness GetReadiness(AutomationProject project)
    {
        var result = _validation.Validate(project);
        return result.Results.Any(x => x.Level == ValidationLevel.Error)
            ? WorkspaceReadiness.NeedsFixes
            : result.Results.Any(x => x.Level == ValidationLevel.Warning)
                ? WorkspaceReadiness.ReadyWithWarnings
                : WorkspaceReadiness.Ready;
    }
}

public sealed record ProjectWorkspaceSnapshot(Guid ProjectId, string ProjectName, int DeviceCount, int ChannelCount, int ErrorCount, int WarningCount);
public enum WorkspaceReadiness { Ready, ReadyWithWarnings, NeedsFixes }
