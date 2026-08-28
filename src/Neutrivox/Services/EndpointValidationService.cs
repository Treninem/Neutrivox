using System.Net;

namespace Neutrivox.Services;

public sealed record EndpointValidationResult(bool Valid, string Normalized, string? Error);

/// <summary>Validates user-supplied IP:port or serial endpoints without opening a connection.</summary>
public sealed class EndpointValidationService
{
    public EndpointValidationResult ValidateNetworkEndpoint(string endpoint, int defaultPort = 502)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return new(false, string.Empty, "Endpoint is empty.");
        var value = endpoint.Trim();
        var separator = value.LastIndexOf(':');
        var host = separator > 0 ? value[..separator] : value;
        var port = separator > 0 && int.TryParse(value[(separator + 1)..], out var parsed) ? parsed : defaultPort;
        if (!IPAddress.TryParse(host, out var address)) return new(false, value, "Endpoint host is not a valid IP address.");
        if (port is < 1 or > 65535) return new(false, value, "Port must be between 1 and 65535.");
        return new(true, $"{address}:{port}", null);
    }

    public EndpointValidationResult ValidateSerialEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return new(false, string.Empty, "Serial endpoint is empty.");
        var value = endpoint.Trim();
        if (!value.StartsWith("COM", StringComparison.OrdinalIgnoreCase)) return new(false, value, "Serial endpoint must use a COM port name.");
        return new(true, value.ToUpperInvariant(), null);
    }
}
