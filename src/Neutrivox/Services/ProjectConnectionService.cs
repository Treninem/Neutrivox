using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectConnectionService
{
    public bool CanConnect(AutomationProject project, Guid fromDeviceId, Guid toDeviceId, string interfaceName, out string reason)
    {
        reason = string.Empty;
        if (fromDeviceId == toDeviceId) { reason = "A device cannot be connected to itself."; return false; }
        if (string.IsNullOrWhiteSpace(interfaceName)) { reason = "Connection interface is required."; return false; }
        if (!project.Devices.Any(x => x.Id == fromDeviceId) || !project.Devices.Any(x => x.Id == toDeviceId))
        { reason = "Both devices must belong to the current project."; return false; }
        if (project.Connections.Any(x => x.FromDeviceId == fromDeviceId && x.ToDeviceId == toDeviceId && x.Interface == interfaceName))
        { reason = "This connection already exists."; return false; }
        return true;
    }

    public bool AddConnection(AutomationProject project, Guid fromDeviceId, Guid toDeviceId, string interfaceName, out string reason)
    {
        if (!CanConnect(project, fromDeviceId, toDeviceId, interfaceName, out reason)) return false;
        project.Connections.Add(new DeviceConnection { FromDeviceId = fromDeviceId, ToDeviceId = toDeviceId, Interface = interfaceName.Trim() });
        return true;
    }

    public bool RemoveConnection(AutomationProject project, Guid fromDeviceId, Guid toDeviceId, string interfaceName)
    {
        var item = project.Connections.FirstOrDefault(x => x.FromDeviceId == fromDeviceId && x.ToDeviceId == toDeviceId && x.Interface == interfaceName);
        if (item is null) return false;
        project.Connections.Remove(item);
        return true;
    }
}
