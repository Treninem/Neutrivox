using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DiscoveryReport(
    IReadOnlyList<DiscoveryObservation> Observations,
    IReadOnlyList<DeviceIdentificationResult> Identifications);

/// <summary>Coordinates discovery probes and profile identification without auto-binding devices.</summary>
public sealed class DiscoveryCoordinatorService
{
    private readonly List<IDeviceProbe> _probes = [];
    private readonly DeviceIdentificationService _identification;

    public DiscoveryCoordinatorService(DeviceIdentificationService identification) => _identification = identification;

    public void Register(IDeviceProbe probe) => _probes.Add(probe);

    public async Task<DiscoveryReport> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken = default)
    {
        var observations = new List<DiscoveryObservation>();
        foreach (var probe in _probes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            observations.AddRange(await probe.ProbeAsync(request, cancellationToken));
        }

        var unique = observations
            .GroupBy(x => $"{x.Transport}|{x.Endpoint}", StringComparer.OrdinalIgnoreCase)
            .Select(x => x.OrderByDescending(v => v.TimestampUtc).First())
            .ToList();
        return new(unique, _identification.Identify(unique));
    }
}
