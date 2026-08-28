using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceSupportMatrixRow(
    string Manufacturer,
    string Model,
    string SupportLevel,
    bool Ethernet,
    bool Rs485,
    bool Modbus,
    bool ProgramTransfer,
    string Notes,
    string Documentation);

public sealed class DeviceSupportMatrixService
{
    public IReadOnlyList<DeviceSupportMatrixRow> Build(DeviceProfileRegistry registry)
        => registry.Profiles.OrderBy(x => x.Manufacturer).ThenBy(x => x.ModelFamily).Select(x => new DeviceSupportMatrixRow(
            x.Manufacturer,
            x.ModelFamily,
            x.SupportLevel.ToString(),
            x.Transports.Contains(DeviceTransport.Ethernet),
            x.Transports.Contains(DeviceTransport.SerialRs485),
            x.Protocols.Contains(DeviceProtocolKind.ModbusRtu) || x.Protocols.Contains(DeviceProtocolKind.ModbusAscii) || x.Protocols.Contains(DeviceProtocolKind.ModbusTcp),
            x.Capabilities.Any(c => c.Name.Contains("Program transfer", StringComparison.OrdinalIgnoreCase) && c.Writable),
            string.Join(" ", x.Capabilities.Select(c => c.Notes)),
            x.DocumentationReference)).ToList();
}
