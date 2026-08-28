using System.IO.Ports;

namespace Neutrivox.Services;

public sealed record SerialEndpointInfo(string PortName, string Description);

public sealed class SerialPortInventoryService
{
    public IReadOnlyList<SerialEndpointInfo> Enumerate()
    {
        return SerialPort.GetPortNames()
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => new SerialEndpointInfo(x.ToUpperInvariant(), "Serial/RS-485 endpoint"))
            .ToList();
    }
}
