namespace Neutrivox.Services;

/// <summary>Performs the protocol-neutral part of Modbus identification. Register addresses are profile data, not guesses.</summary>
public sealed class ModbusIdentificationService
{
    private readonly ModbusRtuFrameService _frames = new();
    private readonly ModbusRtuTransportService _transport = new();

    public async Task<ModbusIdentificationResult> IdentifyAsync(
        SerialConnectionSettings settings,
        byte slaveAddress,
        ushort register,
        int registerCount,
        CancellationToken cancellationToken = default)
    {
        if (registerCount is < 1 or > 32)
            return new(false, slaveAddress, [], "Identification register count is outside the allowed range.");
        try
        {
            var request = _frames.BuildReadHoldingRegisters(slaveAddress, register, (ushort)registerCount);
            var result = await _transport.ExecuteAsync(settings, request, cancellationToken);
            if (!result.Success)
                return new(false, slaveAddress, [], result.Error ?? "No response.");
            if (!_frames.TryParse(result.Response, out var frame) || frame is null)
                return new(false, slaveAddress, [], "The response is not a valid Modbus RTU frame.");
            if (frame.Address != slaveAddress)
                return new(false, slaveAddress, [], "The response slave address does not match the requested address.");
            if (frame.Function != 0x03)
                return new(false, slaveAddress, [], $"Unexpected Modbus function: 0x{frame.Function:X2}.");
            return new(true, slaveAddress, frame.Payload, null);
        }
        catch (Exception ex)
        {
            return new(false, slaveAddress, [], ex.Message);
        }
    }
}

public sealed record ModbusIdentificationResult(bool Success, byte SlaveAddress, IReadOnlyList<byte> Payload, string? Error);
