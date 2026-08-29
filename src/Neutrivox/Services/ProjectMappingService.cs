using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectMappingService
{
    public ProjectMappingSuggestionsResult SuggestMappings(AutomationProject project, IReadOnlyList<DiscoveredDevice> discovered)
    {
        var result = new ProjectMappingSuggestionsResult();
        foreach (var device in project.Devices)
        {
            var matches = discovered.Where(d => !string.IsNullOrWhiteSpace(d.Model) &&
                                               device.Name.Contains(d.Model!, StringComparison.OrdinalIgnoreCase))
                                    .ToList();
            if (matches.Count == 1)
                result.Suggestions.Add(new DeviceMappingSuggestion(device.Id, matches[0], MappingConfidence.Possible));
            else if (matches.Count > 1)
                result.AmbiguousDevices.Add(device.Id);
        }
        return result;
    }

    public void Bind(ProjectDevice projectDevice, DiscoveredDevice physicalDevice)
    {
        projectDevice.PhysicalBinding = new Models.PhysicalDeviceBinding
        {
            Endpoint = physicalDevice.Endpoint,
            Manufacturer = physicalDevice.Manufacturer,
            Model = physicalDevice.Model,
            IdentificationState = physicalDevice.IdentificationState,
            LastSeenUtc = DateTime.UtcNow
        };
    }

    public void Unbind(ProjectDevice projectDevice) => projectDevice.PhysicalBinding = null;
}

public sealed class ProjectMappingSuggestionsResult
{
    public List<DeviceMappingSuggestion> Suggestions { get; } = [];
    public List<Guid> AmbiguousDevices { get; } = [];
}

public sealed record DeviceMappingSuggestion(Guid ProjectDeviceId, DiscoveredDevice PhysicalDevice, MappingConfidence Confidence);
public enum MappingConfidence { Possible, Verified }
