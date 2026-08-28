using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectIntegrationState(
    string ProjectName,
    int DeviceCount,
    int ConnectionCount,
    int LogicNetworkCount,
    int TagCount,
    int BoundDeviceCount,
    bool CanSimulate,
    bool HasBlockingDiagnostics,
    bool HasDeploymentTargets);

/// <summary>Aggregates existing project services into one UI-facing state without duplicating project data.</summary>
public sealed class ProjectIntegrationFacade
{
    private readonly ProjectDiagnosticsService _diagnostics = new();
    private readonly ProjectReadinessService _readiness = new();

    public ProjectIntegrationState Build(AutomationProject project)
    {
        var diagnostics = _diagnostics.Diagnose(project);
        var readiness = _readiness.Assess(project);
        return new(
            project.Name,
            project.Devices.Count,
            project.Connections.Count,
            project.Logic.Networks.Count,
            project.Tags.Count,
            project.Devices.Count(x => x.PhysicalBinding is not null),
            readiness.CanSimulate,
            diagnostics.HasErrors,
            project.Devices.Any(x => x.PhysicalBinding is not null));
    }
}
