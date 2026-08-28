namespace Neutrivox.Services;

public sealed record DiscoveryObservation(
    string Endpoint,
    string Transport,
    string? Manufacturer,
    string? Model,
    string? Protocol,
    string RawIdentity,
    DateTime TimestampUtc);

public sealed record DeviceIdentificationResult(
    DiscoveryObservation Observation,
    string Status,
    double Confidence,
    IReadOnlyList<string> Reasons);

public interface IDeviceProbe
{
    string TransportName { get; }
    Task<IReadOnlyList<DiscoveryObservation>> ProbeAsync(DiscoveryRequest request, CancellationToken cancellationToken);
}
