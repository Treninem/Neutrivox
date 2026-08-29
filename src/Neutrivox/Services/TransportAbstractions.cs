namespace Neutrivox.Services;

public enum TransportKind
{
    Ethernet,
    SerialRs485,
    Usb
}

public sealed record ConnectionEndpoint(TransportKind Kind, string Address, int? Port = null);
public sealed record TransportIdentity(string? Manufacturer, string? Model, string? SerialNumber, IReadOnlyDictionary<string, string> Attributes);
public sealed record TransportProbeResult(bool Connected, TransportIdentity? Identity, string? Error);

public interface IDeviceTransportAdapter
{
    TransportKind Kind { get; }
    bool Supports(ConnectionEndpoint endpoint);
    Task<TransportProbeResult> ProbeAsync(ConnectionEndpoint endpoint, CancellationToken cancellationToken = default);
}

/// <summary>
/// Registry for the low-level probe adapters used by the discovery/connection layer.
/// The write-capable device transport registry remains defined in DeviceTransportAbstractions.cs.
/// </summary>
public sealed class ProbeTransportRegistry
{
    private readonly List<IDeviceTransportAdapter> _adapters = [];
    public IReadOnlyList<IDeviceTransportAdapter> Adapters => _adapters;
    public void Register(IDeviceTransportAdapter adapter) => _adapters.Add(adapter);
    public IDeviceTransportAdapter? Find(ConnectionEndpoint endpoint) => _adapters.FirstOrDefault(x => x.Kind == endpoint.Kind && x.Supports(endpoint));
}
