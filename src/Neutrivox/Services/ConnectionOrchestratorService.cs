using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ConnectionAttemptResult(
    bool Success,
    ConnectionEndpoint Endpoint,
    TransportIdentity? Identity,
    string Message);

/// <summary>
/// Selects one registered read-only probe adapter and checks a physical endpoint.
/// It never writes configuration to the physical device.
/// </summary>
public sealed class ConnectionOrchestratorService
{
    private readonly ProbeTransportRegistry _registry;

    public ConnectionOrchestratorService(ProbeTransportRegistry registry) => _registry = registry;

    public async Task<ConnectionAttemptResult> ProbeAsync(ConnectionEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var adapter = _registry.Find(endpoint);
        if (adapter is null)
            return new(false, endpoint, null, "No registered adapter supports this endpoint.");

        try
        {
            var result = await adapter.ProbeAsync(endpoint, cancellationToken);
            return result.Connected
                ? new(true, endpoint, result.Identity, "Device probe completed successfully.")
                : new(false, endpoint, result.Identity, result.Error ?? "Device probe failed.");
        }
        catch (OperationCanceledException)
        {
            return new(false, endpoint, null, "Device probe was cancelled.");
        }
        catch (Exception ex)
        {
            return new(false, endpoint, null, $"Device probe failed: {ex.Message}");
        }
    }
}
