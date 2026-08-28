using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceMatchCandidate(
    DiscoveredDevice Device,
    string? ProfileId,
    double Confidence,
    string Status,
    IReadOnlyList<string> Reasons);

/// <summary>
/// Converts discovery observations into explicit, reviewable project-binding candidates.
/// It never binds or writes to a device automatically.
/// </summary>
public sealed class DeviceDiscoveryMatchingService
{
    private readonly DeviceProfileRegistry _profiles;

    public DeviceDiscoveryMatchingService(DeviceProfileRegistry profiles) => _profiles = profiles;

    public IReadOnlyList<DeviceMatchCandidate> BuildCandidates(IEnumerable<DiscoveredDevice> devices)
    {
        var result = new List<DeviceMatchCandidate>();
        foreach (var device in devices)
        {
            var matches = _profiles.Match(device.Manufacturer ?? string.Empty, device.Model ?? string.Empty);
            if (matches.Count == 0)
            {
                result.Add(new(device, null, 0, "Unknown", ["No documented device profile matches the reported identity."]));
                continue;
            }

            var best = matches[0];
            var status = best.Confidence >= 0.9 ? "High confidence" : "Needs review";
            var reasons = new List<string>
            {
                $"Manufacturer: {device.Manufacturer ?? "unknown"}",
                $"Model: {device.Model ?? "unknown"}",
                $"Endpoint: {device.Endpoint}",
                best.Reason
            };
            result.Add(new(device, best.Profile.Id, best.Confidence, status, reasons));
        }
        return result;
    }
}
