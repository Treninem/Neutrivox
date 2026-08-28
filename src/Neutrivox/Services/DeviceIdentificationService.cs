using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeviceIdentificationService
{
    private readonly DeviceProfileRegistry _registry;

    public DeviceIdentificationService(DeviceProfileRegistry registry) => _registry = registry;

    public IReadOnlyList<DeviceIdentificationResult> Identify(IEnumerable<DiscoveryObservation> observations)
    {
        var results = new List<DeviceIdentificationResult>();
        foreach (var observation in observations)
        {
            var reasons = new List<string>();
            if (string.IsNullOrWhiteSpace(observation.Manufacturer) && string.IsNullOrWhiteSpace(observation.Model))
            {
                results.Add(new(observation, "Unknown", 0, ["The device did not provide enough identity information."]));
                continue;
            }

            var matches = _registry.Match(observation.Manufacturer ?? string.Empty, observation.Model ?? string.Empty);
            if (matches.Count == 0)
            {
                results.Add(new(observation, "Unmatched", 0, ["No documented profile matched the returned identity."]));
                continue;
            }

            var best = matches[0];
            reasons.Add(best.Reason);
            var status = best.Confidence >= 0.9 ? "VerifiedCandidate" : "Candidate";
            results.Add(new(observation, status, best.Confidence, reasons));
        }
        return results;
    }
}
