using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ModbusRequest(byte UnitId, byte Function, ushort StartAddress, ushort Quantity, ushort[]? Values = null);
public sealed record ModbusResponse(bool Success, byte[] Payload, string? Error);

/// <summary>
/// Protocol-level contract for Modbus transports. Device-specific identity and program transfer
/// must remain above this layer because Modbus itself does not identify a specific vendor/model.
/// </summary>
public interface IModbusClient
{
    Task<ModbusResponse> ReadAsync(ModbusRequest request, CancellationToken cancellationToken = default);
    Task<ModbusResponse> WriteAsync(ModbusRequest request, CancellationToken cancellationToken = default);
}

public interface IModbusTransportFactory
{
    bool Supports(DeviceTransport transport, DeviceProtocolKind protocol);
    IModbusClient Create(DeviceTransport transport, DeviceProtocolKind protocol);
}

public sealed class ModbusClientRegistry
{
    private readonly List<IModbusTransportFactory> _factories = [];
    public void Register(IModbusTransportFactory factory) => _factories.Add(factory);

    public bool TryCreate(DeviceTransport transport, DeviceProtocolKind protocol, out IModbusClient? client)
    {
        var factory = _factories.LastOrDefault(x => x.Supports(transport, protocol));
        client = factory?.Create(transport, protocol);
        return client is not null;
    }
}
