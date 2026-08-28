using System.IO.Ports;

namespace Neutrivox.Services;

public sealed record ModbusRtuRequest(byte Address, byte Function, byte[] Payload);
public sealed record ModbusRtuFrame(byte Address, byte Function, byte[] Payload);

/// <summary>Builds and validates standard Modbus RTU frames. It does not open serial ports.</summary>
public sealed class ModbusRtuFrameService
{
    public byte[] BuildReadHoldingRegisters(byte address, ushort start, ushort count)
        => Build(address, 0x03, [(byte)(start >> 8), (byte)start, (byte)(count >> 8), (byte)count]);

    public byte[] BuildReadInputRegisters(byte address, ushort start, ushort count)
        => Build(address, 0x04, [(byte)(start >> 8), (byte)start, (byte)(count >> 8), (byte)count]);

    public byte[] BuildWriteSingleRegister(byte address, ushort register, ushort value)
        => Build(address, 0x06, [(byte)(register >> 8), (byte)register, (byte)(value >> 8), (byte)value]);

    public byte[] BuildWriteMultipleRegisters(byte address, ushort start, IReadOnlyList<ushort> values)
    {
        if (values.Count is < 1 or > 123) throw new ArgumentOutOfRangeException(nameof(values));
        var payload = new byte[5 + values.Count * 2];
        payload[0] = (byte)(start >> 8); payload[1] = (byte)start;
        payload[2] = (byte)(values.Count >> 8); payload[3] = (byte)values.Count;
        payload[4] = (byte)(values.Count * 2);
        for (var i = 0; i < values.Count; i++)
        {
            payload[5 + i * 2] = (byte)(values[i] >> 8);
            payload[6 + i * 2] = (byte)values[i];
        }
        return Build(address, 0x10, payload);
    }

    public bool TryParse(ReadOnlySpan<byte> frame, out ModbusRtuFrame? result)
    {
        result = null;
        if (frame.Length < 4) return false;
        var expected = Crc16(frame[..^2]);
        var actual = (ushort)(frame[^2] | (frame[^1] << 8));
        if (expected != actual) return false;
        result = new(frame[0], frame[1], frame[2..^2].ToArray());
        return true;
    }

    private static byte[] Build(byte address, byte function, byte[] payload)
    {
        if (address is 0 or > 247) throw new ArgumentOutOfRangeException(nameof(address));
        var frame = new byte[payload.Length + 4]; frame[0] = address; frame[1] = function;
        payload.CopyTo(frame, 2);
        var crc = Crc16(frame.AsSpan(0, payload.Length + 2));
        frame[^2] = (byte)crc; frame[^1] = (byte)(crc >> 8);
        return frame;
    }

    private static ushort Crc16(ReadOnlySpan<byte> bytes)
    {
        ushort crc = 0xFFFF;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++) crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
        }
        return crc;
    }
}
