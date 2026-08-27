using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class WorkspaceService
{
    public ProjectWorkspace CreateDefault(AutomationProject project)
    {
        var workspace = new ProjectWorkspace();
        ArrangeDevices(project, workspace);
        return workspace;
    }

    public void ArrangeDevices(AutomationProject project, ProjectWorkspace workspace)
    {
        const double startX = 40, startY = 40, width = 220, height = 120, gap = 45;
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(Math.Max(1, project.Devices.Count))));
        for (var index = 0; index < project.Devices.Count; index++)
        {
            var row = index / columns;
            var column = index % columns;
            workspace.DevicePositions[project.Devices[index].Id] = new WorkspaceDevicePosition(startX + column * (width + gap), startY + row * (height + gap));
        }
    }

    public void MoveDevice(ProjectWorkspace workspace, Guid deviceId, double x, double y)
        => workspace.DevicePositions[deviceId] = new WorkspaceDevicePosition(Math.Max(0, x), Math.Max(0, y));
}
