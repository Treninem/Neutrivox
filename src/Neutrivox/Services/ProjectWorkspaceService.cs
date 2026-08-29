using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Coordinates the single project workflow used for offline design, simulation and physical deployment.</summary>
public sealed class ProjectWorkspaceService
{
    private readonly ProjectEquipmentService _equipment;
    private readonly ProjectValidationService _validation;
    private readonly DeviceCatalogService _catalog;

    public ProjectWorkspaceService(ProjectEquipmentService equipment, ProjectValidationService validation, DeviceCatalogService? catalog = null)
    {
        _equipment = equipment;
        _validation = validation;
        _catalog = catalog ?? new DeviceCatalogService();
    }

    public ProjectWorkspaceSnapshot GetSnapshot(AutomationProject project)
    {
        var validation = _validation.Validate(project, _catalog);
        return new ProjectWorkspaceSnapshot(
            project.Id,
            project.Name,
            project.Devices.Count,
            project.Devices.Sum(d => d.Channels.Count),
            validation.ErrorCount,
            validation.WarningCount);
    }

    public WorkspaceReadiness GetReadiness(AutomationProject project)
    {
        var result = _validation.Validate(project, _catalog);
        return result.ErrorCount > 0
            ? WorkspaceReadiness.NeedsFixes
            : result.WarningCount > 0
                ? WorkspaceReadiness.ReadyWithWarnings
                : WorkspaceReadiness.Ready;
    }
}

public sealed record ProjectWorkspaceSnapshot(Guid ProjectId, string ProjectName, int DeviceCount, int ChannelCount, int ErrorCount, int WarningCount);
public enum WorkspaceReadiness { Ready, ReadyWithWarnings, NeedsFixes }
