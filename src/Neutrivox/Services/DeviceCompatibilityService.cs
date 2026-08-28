using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record CompatibilityIssue(string Severity, string Message);
public sealed record DeviceCompatibilityResult(bool Compatible, IReadOnlyList<CompatibilityIssue> Issues);

/// <summary>Checks whether a project device can be associated with a documented profile.</summary>
public sealed class DeviceCompatibilityService
{
    public DeviceCompatibilityResult Check(ProjectDevice device, DeviceProfile profile)
    {
        var issues = new List<CompatibilityIssue>();
        if (!device.DefinitionId.Equals(profile.Id, StringComparison.OrdinalIgnoreCase))
            issues.Add(new("Error", $"Project device definition '{device.DefinitionId}' does not match profile '{profile.Id}'."));

        foreach (var required in profile.Channels)
        {
            var present = device.Channels.Any(x => x.Name.Equals(required.Name, StringComparison.OrdinalIgnoreCase) ||
                                                   x.Name.StartsWith(required.Name.Replace("…", ""), StringComparison.OrdinalIgnoreCase));
            if (!present) issues.Add(new("Warning", $"The profile expects channel group '{required.Name}', but the project does not contain a direct match."));
        }

        if (profile.SupportLevel == DeviceSupportLevel.ModelProfiled)
            issues.Add(new("Info", "The model profile is documented, but full physical discovery/read/write support is not claimed yet."));

        return new(!issues.Any(x => x.Severity == "Error"), issues);
    }
}
