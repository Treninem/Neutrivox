namespace Neutrivox.Services;

public sealed record DiscoveryListItem(
    string Endpoint,
    string Transport,
    string Protocol,
    string Manufacturer,
    string Model,
    string Status,
    double Confidence,
    string Reason);

/// <summary>Converts discovery results into stable, user-readable items for the UI.</summary>
public sealed class DeviceDiscoveryPresenterService
{
    private readonly DiscoveryOrchestrationService _orchestrator;

    public DeviceDiscoveryPresenterService(DiscoveryOrchestrationService orchestrator) => _orchestrator = orchestrator;

    public async Task<IReadOnlyList<DiscoveryListItem>> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken = default)
    {
        var run = await _orchestrator.RunAsync(request, cancellationToken);
        var byEndpoint = run.Identifications.ToDictionary(x => x.Observation.Endpoint, StringComparer.OrdinalIgnoreCase);
        return run.Devices.Select(device =>
        {
            byEndpoint.TryGetValue(device.Endpoint, out var identity);
            return new DiscoveryListItem(
                device.Endpoint,
                identity?.Observation.Transport ?? "Unknown",
                device.Protocol,
                device.Manufacturer ?? "Unknown",
                device.Model ?? "Unknown",
                identity?.Status ?? device.IdentificationState,
                identity?.Confidence ?? 0,
                identity is null ? "No identity result." : string.Join(" ", identity.Reasons));
        }).ToList();
    }
}
