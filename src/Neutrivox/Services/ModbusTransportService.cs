using System.IO.Ports;
using System.Runtime.Versioning;

namespace Neutrivox.Services;

public sealed record SerialConnectionSettings(string PortName, int BaudRate, Parity Parity, int DataBits, StopBits StopBits, int TimeoutMs = 500);
public sealed record ModbusTransactionResult(bool Success, byte[] Response, string? Error, TimeSpan Duration);

/// <summary>Low-level Modbus RTU transport. It only communicates with the endpoint explicitly supplied by the caller.</summary>
[SupportedOSPlatform("windows")]
public sealed class ModbusRtuTransportService
{
    private readonly ModbusRtuFrameService _frames = new();

    public async Task<ModbusTransactionResult> ExecuteAsync(SerialConnectionSettings settings, byte[] request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.PortName)) return new(false, [], "Serial port is empty.", TimeSpan.Zero);
        if (settings.BaudRate <= 0 || settings.DataBits is < 5 or > 8) return new(false, [], "Serial communication settings are invalid.", TimeSpan.Zero);
        var started = DateTime.UtcNow;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var port = new SerialPort(settings.PortName, settings.BaudRate, settings.Parity, settings.DataBits, settings.StopBits)
            {
                ReadTimeout = Math.Clamp(settings.TimeoutMs, 50, 5000),
                WriteTimeout = Math.Clamp(settings.TimeoutMs, 50, 5000)
            };
            port.Open();
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            await port.BaseStream.WriteAsync(request.AsMemory(), cancellationToken);
            await port.BaseStream.FlushAsync(cancellationToken);

            // Modbus RTU response lengths vary by function; this transport reads the currently available
            // bytes and waits briefly for the frame to settle. A higher protocol layer validates the frame.
            var buffer = new byte[256];
            var count = 0;
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked.CancelAfter(Math.Clamp(settings.TimeoutMs, 50, 5000));
            while (!linked.IsCancellationRequested && count < buffer.Length)
            {
                if (port.BytesToRead > 0)
                {
                    count += port.Read(buffer, count, Math.Min(port.BytesToRead, buffer.Length - count));
                    if (count >= 5) await Task.Delay(10, linked.Token);
                }
                else await Task.Delay(5, linked.Token);
            }
            return new(count > 0, buffer[..count], count == 0 ? "No response received." : null, DateTime.UtcNow - started);
        }
        catch (OperationCanceledException)
        {
            return new(false, [], "Operation cancelled or timed out.", DateTime.UtcNow - started);
        }
        catch (Exception ex)
        {
            return new(false, [], ex.Message, DateTime.UtcNow - started);
        }
    }

    public Task<ModbusTransactionResult> ExecuteReadHoldingRegistersAsync(SerialConnectionSettings settings, byte address, ushort start, ushort count, CancellationToken cancellationToken = default)
        => ExecuteAsync(settings, _frames.BuildReadHoldingRegisters(address, start, count), cancellationToken);
}
