namespace Neutrivox.Services;

public sealed record DiscoveryRunResult(
    IReadOnlyList<DiscoveredDevice> Devices,
    IReadOnlyList<DeviceIdentificationResult> Identifications,
    DateTime CompletedAtUtc);

/// <summary>Runs all registered discovery providers and then applies documented identity matching.</summary>
public sealed class DiscoveryOrchestrationService
{
    private readonly DeviceDiscoveryService _discovery;
    private readonly DeviceProfileRegistry _profiles;
    private readonly DeviceIdentificationService _identification;

    public DiscoveryOrchestrationService(DeviceDiscoveryService discovery, DeviceProfileRegistry profiles)
    {
        _discovery = discovery;
        _profiles = profiles;
        _identification = new DeviceIdentificationService(profiles);
    }

    public async Task<DiscoveryRunResult> RunAsync(DiscoveryRequest request, CancellationToken cancellationToken = default)
    {
        var devices = await _discovery.DiscoverAsync(request, cancellationToken);
        var observations = devices.Select(x => new DiscoveryObservation(
            x.Endpoint,
            InferTransport(x.Protocol, x.Endpoint),
            x.Manufacturer,
            x.Model,
            x.Protocol,
            x.IdentificationState,
            DateTime.UtcNow)).ToList();
        var identities = _identification.Identify(observations);
        return new(devices, identities, DateTime.UtcNow);
    }

    private static string InferTransport(string protocol, string endpoint)
    {
        if (endpoint.Contains("COM", StringComparison.OrdinalIgnoreCase)) return "RS-485";
        if (protocol.Contains("RTU", StringComparison.OrdinalIgnoreCase)) return "RS-485";
        if (endpoint.Contains(':')) return "Ethernet";
        return "Unknown";
    }
}
