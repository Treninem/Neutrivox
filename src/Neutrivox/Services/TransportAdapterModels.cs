using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record TransportEndpoint(DeviceTransport Transport, string Address, int? Port = null);

public sealed record TransportConnectionResult(bool Success, string Message, string? Identity = null);

public sealed record DeviceReadIdentityResult(
    bool Success,
    string? Manufacturer,
    string? Model,
    string RawResponse,
    string Message);

public interface ITransportAdapter
{
    DeviceTransport Transport { get; }
    Task<TransportConnectionResult> ConnectAsync(TransportEndpoint endpoint, CancellationToken cancellationToken = default);
    Task<DeviceReadIdentityResult> ReadIdentityAsync(TransportEndpoint endpoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}

public sealed class TransportAdapterRegistry
{
    private readonly Dictionary<DeviceTransport, ITransportAdapter> _adapters = [];
    public void Register(ITransportAdapter adapter) => _adapters[adapter.Transport] = adapter;
    public bool TryGet(DeviceTransport transport, out ITransportAdapter? adapter) => _adapters.TryGetValue(transport, out adapter);
}
