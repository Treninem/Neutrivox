namespace Neutrivox.Services;

public sealed record ModbusAddressProbeResult(byte SlaveAddress, bool Responded, string? Error);

/// <summary>Probes a bounded, explicitly requested Modbus RTU slave-address range. It never writes registers.</summary>
public sealed class ModbusAddressProbeService
{
    private readonly ModbusRtuFrameService _frames = new();
    private readonly ModbusRtuTransportService _transport = new();

    public async Task<IReadOnlyList<ModbusAddressProbeResult>> ProbeAsync(
        SerialConnectionSettings settings,
        byte firstAddress = 1,
        byte lastAddress = 32,
        ushort probeRegister = 0,
        CancellationToken cancellationToken = default)
    {
        if (firstAddress < 1 || lastAddress > 247 || firstAddress > lastAddress)
            throw new ArgumentOutOfRangeException(nameof(firstAddress));
        if (lastAddress - firstAddress > 32)
            throw new ArgumentOutOfRangeException(nameof(lastAddress), "A single probe operation is limited to 33 addresses.");

        var results = new List<ModbusAddressProbeResult>();
        for (var address = firstAddress; address <= lastAddress; address++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = _frames.BuildReadHoldingRegisters(address, probeRegister, 1);
            var response = await _transport.ExecuteAsync(settings, request, cancellationToken);
            results.Add(new((byte)address, response.Success, response.Error));
        }
        return results;
    }
}
