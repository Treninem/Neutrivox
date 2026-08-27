using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectWorkspaceController
{
    private readonly WorkspaceService _workspaceService;
    public ProjectWorkspace Workspace { get; private set; } = new();
    public ProjectSelection Selection { get; } = new();

    public ProjectWorkspaceController(WorkspaceService workspaceService) => _workspaceService = workspaceService;

    public void LoadProject(AutomationProject project)
    {
        Workspace = _workspaceService.CreateDefault(project);
        Selection.Clear();
    }

    public void EnsureDeviceLayout(AutomationProject project)
    {
        foreach (var device in project.Devices)
            if (!Workspace.DevicePositions.ContainsKey(device.Id))
                _workspaceService.ArrangeDevices(project, Workspace);
    }
}
