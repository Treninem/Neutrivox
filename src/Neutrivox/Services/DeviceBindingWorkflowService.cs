using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceBindingCandidate(
    DiscoveredDevice Device,
    DeviceProfileMatch? ProfileMatch,
    DeviceCompatibilityResult? Compatibility);

public sealed class DeviceBindingWorkflowService
{
    private readonly DeviceProfileRegistry _profiles;
    private readonly DeviceIdentificationService _identification;
    private readonly DeviceCompatibilityService _compatibility = new();
    private readonly ProjectBindingService _binding = new();

    public DeviceBindingWorkflowService(DeviceProfileRegistry profiles)
    {
        _profiles = profiles;
        _identification = new DeviceIdentificationService(profiles);
    }

    public IReadOnlyList<DeviceBindingCandidate> BuildCandidates(ProjectDevice projectDevice, IEnumerable<DiscoveredDevice> discovered)
    {
        var candidates = new List<DeviceBindingCandidate>();
        foreach (var item in discovered)
        {
            var identity = _identification.Identify([new DiscoveryObservation(
                item.Endpoint, item.Protocol, item.Manufacturer, item.Model, item.Protocol, string.Empty, DateTime.UtcNow)]).First();
            var match = identity.Observation.Manufacturer is null && identity.Observation.Model is null
                ? null
                : _profiles.Match(identity.Observation.Manufacturer ?? string.Empty, identity.Observation.Model ?? string.Empty).FirstOrDefault();
            var compatibility = match is null ? null : _compatibility.Check(projectDevice, match.Profile);
            candidates.Add(new DeviceBindingCandidate(item, match, compatibility));
        }
        return candidates;
    }

    public BindingResult Confirm(ProjectDevice projectDevice, DeviceBindingCandidate candidate)
    {
        if (candidate.ProfileMatch is null) return new(false, "No documented profile matched the discovered device.");
        if (candidate.Compatibility is { Compatible: false }) return new(false, "The project device is not compatible with the selected documented profile.");
        return _binding.Bind(projectDevice, candidate.Device, true);
    }
}
