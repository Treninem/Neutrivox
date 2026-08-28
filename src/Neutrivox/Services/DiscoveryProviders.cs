using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO.Ports;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Safe discovery provider for explicitly supplied IPv4 scopes. It performs connection attempts only to the requested range.</summary>
public sealed class EthernetEndpointDiscoveryProvider : IDeviceDiscoveryProvider
{
    private readonly int _port;
    private readonly int _timeoutMs;

    public EthernetEndpointDiscoveryProvider(int port = 502, int timeoutMs = 250)
    {
        _port = port is >= 1 and <= 65535 ? port : 502;
        _timeoutMs = Math.Clamp(timeoutMs, 50, 5000);
    }

    public async Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken)
    {
        if (!request.IncludeEthernet || !NetworkScopeParser.TryParse(request.NetworkScope, out var addresses)) return [];
        var found = new List<DiscoveredDevice>();
        using var gate = new SemaphoreSlim(32);
        var tasks = addresses.Select(async address =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                using var client = new TcpClient();
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linked.CancelAfter(_timeoutMs);
                try
                {
                    await client.ConnectAsync(address, _port, linked.Token);
                    lock (found)
                        found.Add(new DiscoveredDevice($"{address}:{_port}", "TCP", null, null, "EndpointReachable"));
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { }
                catch (SocketException) { }
            }
            finally { gate.Release(); }
        });
        await Task.WhenAll(tasks);
        return found;
    }
}

/// <summary>Enumerates local serial interfaces. It does not transmit data during inventory discovery.</summary>
public sealed class SerialPortInventoryDiscoveryProvider : IDeviceDiscoveryProvider
{
    public Task<IReadOnlyList<DiscoveredDevice>> DiscoverAsync(DiscoveryRequest request, CancellationToken cancellationToken)
    {
        if (!request.IncludeSerial) return Task.FromResult<IReadOnlyList<DiscoveredDevice>>([]);
        var ports = SerialPort.GetPortNames()
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(port => new DiscoveredDevice(port, "SERIAL", null, null, "PortPresent"))
            .ToList();
        return Task.FromResult<IReadOnlyList<DiscoveredDevice>>(ports);
    }
}

internal static class NetworkScopeParser
{
    public static bool TryParse(string scope, out IReadOnlyList<IPAddress> addresses)
    {
        addresses = [];
        if (string.IsNullOrWhiteSpace(scope)) return false;
        var value = scope.Trim();

        if (IPAddress.TryParse(value, out var single) && single.AddressFamily == AddressFamily.InterNetwork)
        {
            addresses = [single];
            return true;
        }

        var slash = value.IndexOf('/');
        if (slash <= 0 || !IPAddress.TryParse(value[..slash], out var network) || network.AddressFamily != AddressFamily.InterNetwork || !int.TryParse(value[(slash + 1)..], out var prefix) || prefix is < 16 or > 32)
            return false;

        var net = ToUInt32(network);
        var mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
        var first = net & mask;
        var count = 1UL << (32 - prefix);
        if (count > 4096) return false;

        var start = first + (count > 2 ? 1u : 0u);
        var end = first + (uint)count - (count > 2 ? 2u : 1u);
        var result = new List<IPAddress>((int)Math.Min(count, 4096));
        for (var value32 = start; value32 <= end; value32++) result.Add(FromUInt32(value32));
        addresses = result;
        return true;
    }

    private static uint ToUInt32(IPAddress address)
    {
        var b = address.GetAddressBytes();
        return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
    }

    private static IPAddress FromUInt32(uint value) => new([(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value]);
}
