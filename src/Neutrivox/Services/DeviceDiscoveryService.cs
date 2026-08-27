namespace Neutrivox.Services;

public sealed class DeviceDiscoveryService
{
    // Discovery providers are registered per officially supported protocol.
    // The application never assumes that an IP address identifies a device model.
    private readonly List<IDeviceDiscoveryProvider> _providers = [];

    public void Register(IDeviceDiscoveryProvider provider) => _providers.Add(provider);

    public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken = default)
    {
        var results = new List<DiscoveredDevice>();
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.AddRange(await provider.DiscoverAsync(request, cancellationToken));
        }
        return results.GroupBy(x => $"{x.Endpoint}|{x.Protocol}").Select(x => x.First()).ToList();
    }
}

public sealed record DiscoveryRequest(string NetworkScope, bool IncludeEthernet = true, bool IncludeSerial = true);
public sealed record DiscoveredDevice(string Endpoint, string Protocol, string? Manufacturer, string? Model, string IdentificationState);

public interface IDeviceDiscoveryProvider
{
    Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken);
}