using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectValidationService
{
    public ProjectValidationResult Validate(AutomationProject project, DeviceCatalogService catalog)
    {
        var result = new ProjectValidationResult();
        if (string.IsNullOrWhiteSpace(project.Name))
            result.Issues.Add(new(ValidationSeverity.Error, "PROJECT_NAME", "Project name is required."));
        if (project.Devices.Count == 0)
            result.Issues.Add(new(ValidationSeverity.Warning, "NO_DEVICES", "No equipment has been added to the project."));

        foreach (var device in project.Devices)
        {
            if (catalog.Find(device.DefinitionId) is null)
                result.Issues.Add(new(ValidationSeverity.Error, "UNKNOWN_DEVICE", $"Unknown device definition: {device.DefinitionId}", device.Id));
            if (string.IsNullOrWhiteSpace(device.Name))
                result.Issues.Add(new(ValidationSeverity.Warning, "DEVICE_NAME", "Device has no display name.", device.Id));
            if (device.Channels.Count == 0)
                result.Issues.Add(new(ValidationSeverity.Warning, "NO_CHANNELS", "Device has no configured channels.", device.Id));
        }

        foreach (var connection in project.Connections)
        {
            if (!project.Devices.Any(x => x.Id == connection.FromDeviceId) || !project.Devices.Any(x => x.Id == connection.ToDeviceId))
                result.Issues.Add(new(ValidationSeverity.Error, "BROKEN_CONNECTION", "Connection references a device that is not in the project."));
            if (connection.FromDeviceId == connection.ToDeviceId)
                result.Issues.Add(new(ValidationSeverity.Error, "SELF_CONNECTION", "A device cannot be connected to itself."));
        }
        return result;
    }
}
