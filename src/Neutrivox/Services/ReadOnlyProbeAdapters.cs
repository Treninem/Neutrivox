namespace Neutrivox.Services;

/// <summary>
/// Safe placeholders for transports that do not yet have a documented identification procedure.
/// They deliberately refuse to invent an identity or perform any write operation.
/// </summary>
public sealed class UnsupportedProbeAdapter : IDeviceTransportAdapter
{
    public TransportKind Kind { get; }
    public UnsupportedProbeAdapter(TransportKind kind) => Kind = kind;
    public bool Supports(ConnectionEndpoint endpoint) => endpoint.Kind == Kind;

    public Task<TransportProbeResult> ProbeAsync(ConnectionEndpoint endpoint, CancellationToken cancellationToken = default)
        => Task.FromResult(new TransportProbeResult(false, null,
            $"Read-only identification for transport {Kind} is not implemented yet."));
}
