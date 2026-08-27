using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectEquipmentService
{
    public ProjectDevice AddDevice(AutomationProject project, DeviceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(definition);

        var number = project.Devices.Count(x => x.DefinitionId == definition.Id) + 1;
        var device = new ProjectDevice
        {
            DefinitionId = definition.Id,
            Name = $"{definition.Model} {number}"
        };

        foreach (var channel in definition.Channels)
        {
            device.Channels.Add(new IoChannel
            {
                Name = channel.Name,
                Type = channel.Type,
                Direction = channel.Direction
            });
        }

        project.Devices.Add(device);
        return device;
    }

    public bool RemoveDevice(AutomationProject project, Guid deviceId)
    {
        var device = project.Devices.FirstOrDefault(x => x.Id == deviceId);
        if (device is null) return false;

        project.Connections.RemoveAll(x => x.FromDeviceId == deviceId || x.ToDeviceId == deviceId);
        project.Devices.Remove(device);
        return true;
    }
}
