using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceMatchCandidate(
    Guid ProjectDeviceId,
    string ProjectDeviceName,
    string Endpoint,
    string Status,
    double Confidence,
    string Reason);

public sealed class DeviceMatchWorkflowService
{
    private readonly DeviceProfileRegistry _profiles;

    public DeviceMatchWorkflowService(DeviceProfileRegistry profiles) => _profiles = profiles;

    public IReadOnlyList<DeviceMatchCandidate> BuildCandidates(ProjectDevice projectDevice, IEnumerable<DiscoveredDevice> discovered)
    {
        var result = new List<DeviceMatchCandidate>();
        foreach (var physical in discovered)
        {
            if (string.IsNullOrWhiteSpace(physical.Manufacturer) || string.IsNullOrWhiteSpace(physical.Model))
            {
                result.Add(new(projectDevice.Id, projectDevice.Name, physical.Endpoint, "Unknown", 0, "The device did not report enough model information."));
                continue;
            }

            var matches = _profiles.Match(physical.Manufacturer, physical.Model);
            var best = matches.FirstOrDefault();
            result.Add(best is null
                ? new(projectDevice.Id, projectDevice.Name, physical.Endpoint, "Unmatched", 0, "No documented profile matched the reported identity.")
                : new(projectDevice.Id, projectDevice.Name, physical.Endpoint, best.Confidence >= .9 ? "CandidateVerified" : "Candidate", best.Confidence, best.Reason));
        }
        return result.OrderByDescending(x => x.Confidence).ToList();
    }
}
