using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Registry-backed factory for write-capable device transports.</summary>
public sealed class DeviceTransportFactory
{
    private readonly List<IDeviceTransportFactory> _factories = [];

    public void Register(IDeviceTransportFactory factory) => _factories.Add(factory);

    public IDeviceTransport? Create(DeviceTransport transport, DeviceProtocolKind protocol)
        => _factories.LastOrDefault(x => x.CanHandle(transport, protocol))?.Create(transport, protocol);
}
