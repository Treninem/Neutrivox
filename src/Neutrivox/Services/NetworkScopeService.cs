using System.Net;
using System.Net.Sockets;

namespace Neutrivox.Services;

public sealed record NetworkScopeResult(bool Valid, string Normalized, string? Error, int AddressCount);

public sealed class NetworkScopeService
{
    public NetworkScopeResult Validate(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope)) return new(false, string.Empty, "Network scope is empty.", 0);
        var value = scope.Trim();
        if (IPAddress.TryParse(value, out var ip)) return new(true, ip.ToString(), null, 1);

        var slash = value.IndexOf('/');
        if (slash <= 0 || slash == value.Length - 1) return new(false, value, "Use an IP address or CIDR network such as 192.168.1.0/24.", 0);
        if (!IPAddress.TryParse(value[..slash], out var network) || !int.TryParse(value[(slash + 1)..], out var prefix))
            return new(false, value, "Invalid CIDR network.", 0);
        var max = network.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
        if (prefix < 0 || prefix > max) return new(false, value, $"CIDR prefix must be between 0 and {max}.", 0);
        var count = prefix >= 31 ? (1 << Math.Max(0, 32 - prefix)) : Math.Min(1 << (32 - prefix), 1_048_576);
        return new(true, value, null, count);
    }
}
