using System.IO.Ports;

namespace Neutrivox.Services;

/// <summary>
/// Performs conservative read-only Modbus RTU discovery over one selected serial endpoint.
/// It reports an endpoint and slave address only; it never invents a model identity.
/// </summary>
public sealed class ModbusSerialDiscoveryProvider : IDeviceDiscoveryProvider
{
    private readonly ModbusRtuFrameService _frames = new();
    private readonly int _fromAddress;
    private readonly int _toAddress;
    private readonly int _baudRate;
    private readonly int _timeoutMs;

    public ModbusSerialDiscoveryProvider(int fromAddress = 1, int toAddress = 32, int baudRate = 9600, int timeoutMs = 120)
    {
        if (fromAddress is < 1 or > 247 || toAddress is < 1 or > 247 || fromAddress > toAddress)
            throw new ArgumentOutOfRangeException(nameof(fromAddress));
        _fromAddress = fromAddress;
        _toAddress = toAddress;
        _baudRate = baudRate;
        _timeoutMs = Math.Clamp(timeoutMs, 30, 2000);
    }

    public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken)
    {
        if (!request.IncludeSerial) return [];
        var portName = NormalizePort(request.NetworkScope);
        if (portName is null) return [];

        return await Task.Run(() => Scan(portName, cancellationToken), cancellationToken);
    }

    private IReadOnlyList<DiscoveredDevice> Scan(string portName, CancellationToken cancellationToken)
    {
        var found = new List<DiscoveredDevice>();
        using var port = new SerialPort(portName, _baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = _timeoutMs,
            WriteTimeout = _timeoutMs,
            DtrEnable = false,
            RtsEnable = false
        };
        port.Open();

        for (var address = _fromAddress; address <= _toAddress; address++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = _frames.BuildReadHoldingRegisters((byte)address, 0, 1);
            port.DiscardInBuffer();
            port.Write(request, 0, request.Length);
            if (!TryReadFrame(port, cancellationToken, out var response)) continue;
            if (response.Address != address || response.Function is not (0x03 or 0x83)) continue;
            if (response.Function == 0x83) continue;
            found.Add(new DiscoveredDevice(
                $"{portName}@{address}",
                "Modbus RTU",
                null,
                null,
                "AddressVerified"));
        }
        return found;
    }

    private bool TryReadFrame(SerialPort port, CancellationToken token, out ModbusRtuFrame response)
    {
        response = null!;
        var buffer = new List<byte>(32);
        var started = DateTime.UtcNow;
        while ((DateTime.UtcNow - started).TotalMilliseconds < _timeoutMs)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                if (port.BytesToRead > 0)
                {
                    var chunk = new byte[Math.Min(port.BytesToRead, 64)];
                    var read = port.Read(chunk, 0, chunk.Length);
                    for (var i = 0; i < read; i++) buffer.Add(chunk[i]);
                    if (buffer.Count >= 5 && _frames.TryParse(CollectionsMarshal.AsSpan(buffer), out var parsed) && parsed is not null)
                    {
                        response = parsed;
                        return true;
                    }
                }
            }
            catch (TimeoutException) { return false; }
            Thread.Sleep(2);
        }
        return false;
    }

    private static string? NormalizePort(string scope)
    {
        var value = scope.Trim();
        return value.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ? value.ToUpperInvariant() : null;
    }
}
