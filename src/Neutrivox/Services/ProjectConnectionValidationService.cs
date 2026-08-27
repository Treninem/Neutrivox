using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ConnectionValidationResult(bool Valid, string Message);

public sealed class ProjectConnectionValidationService
{
    public ConnectionValidationResult Validate(AutomationProject project, Guid fromDeviceId, Guid toDeviceId, string? @interface)
    {
        if (fromDeviceId == toDeviceId) return new(false, "A device cannot be connected to itself.");
        var from = project.Devices.FirstOrDefault(x => x.Id == fromDeviceId);
        var to = project.Devices.FirstOrDefault(x => x.Id == toDeviceId);
        if (from is null || to is null) return new(false, "Both devices must exist in the current project.");
        if (string.IsNullOrWhiteSpace(@interface)) return new(false, "Connection interface must be specified.");
        if (project.Connections.Any(x => x.FromDeviceId == fromDeviceId && x.ToDeviceId == toDeviceId && x.Interface.Equals(@interface, StringComparison.OrdinalIgnoreCase)))
            return new(false, "An identical connection already exists.");
        return new(true, "Connection is valid.");
    }
}
