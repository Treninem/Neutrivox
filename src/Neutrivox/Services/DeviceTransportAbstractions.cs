using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceConnectionRequest(
    string Endpoint,
    DeviceProtocolKind Protocol,
    TimeSpan ConnectTimeout,
    IReadOnlyDictionary<string, object?> Parameters);

public sealed record DeviceConnectionInfo(
    string Endpoint,
    string Transport,
    string Protocol,
    bool IsConnected,
    string StatusMessage);

public sealed record DeviceIdentityReadResult(
    bool Success,
    string? Manufacturer,
    string? Model,
    string RawIdentity,
    string Message);

public interface IDeviceTransport : IAsyncDisposable
{
    string TransportName { get; }
    Task<DeviceConnectionInfo> ConnectAsync(DeviceConnectionRequest request, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<DeviceIdentityReadResult> ReadIdentityAsync(CancellationToken cancellationToken = default);
}

public interface IDeviceTransportFactory
{
    bool CanHandle(DeviceTransport transport, DeviceProtocolKind protocol);
    IDeviceTransport Create(DeviceTransport transport, DeviceProtocolKind protocol);
}

public sealed class DeviceTransportRegistry
{
    private readonly List<IDeviceTransportFactory> _factories = [];
    public void Register(IDeviceTransportFactory factory) => _factories.Add(factory);

    public bool TryCreate(DeviceTransport transport, DeviceProtocolKind protocol, out IDeviceTransport? client)
    {
        var factory = _factories.LastOrDefault(x => x.CanHandle(transport, protocol));
        client = factory?.Create(transport, protocol);
        return client is not null;
    }
}
