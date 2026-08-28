namespace Neutrivox.Models;

public enum DeviceTransport { None, Usb, SerialRs485, Ethernet }
public enum DeviceProtocolKind { None, ModbusRtu, ModbusAscii, ModbusTcp, VendorSpecific }
public enum DeviceSupportLevel { ModelProfiled, Discoverable, ReadWriteSupported }

public sealed class DeviceProfile
{
    public string Id { get; init; } = string.Empty;
    public string Manufacturer { get; init; } = string.Empty;
    public string ModelFamily { get; init; } = string.Empty;
    public string VariantPattern { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DeviceSupportLevel SupportLevel { get; init; }
    public List<DeviceTransport> Transports { get; init; } = [];
    public List<DeviceProtocolKind> Protocols { get; init; } = [];
    public List<DeviceProfileChannel> Channels { get; init; } = [];
    public List<DeviceProtocolCapability> Capabilities { get; init; } = [];
    public string DocumentationReference { get; init; } = string.Empty;
}

public sealed record DeviceProfileChannel(string Name, string Type, string Direction, string SignalDescription);
public sealed record DeviceProtocolCapability(string Name, bool Readable, bool Writable, string Notes);

public sealed record DeviceProfileMatch(
    DeviceProfile Profile,
    double Confidence,
    string Reason);
