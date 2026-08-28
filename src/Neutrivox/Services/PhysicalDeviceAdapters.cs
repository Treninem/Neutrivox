using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record PhysicalDeviceProbeResult(
    bool Success,
    string Endpoint,
    string? Manufacturer,
    string? Model,
    string? Firmware,
    IReadOnlyList<string> Details,
    string? Error);

public sealed record TransferRequest(
    Guid ProjectId,
    Guid ProjectDeviceId,
    string Endpoint,
    string PayloadFormat,
    byte[] Payload);

public sealed record TransferResult(bool Success, string Status, string? Error, DateTime CompletedAtUtc);

/// <summary>Transport-neutral contract for real device adapters. Implementations must be tied to documented device protocols.</summary>
public interface IPhysicalDeviceAdapter
{
    string Id { get; }
    IReadOnlyList<DeviceTransport> Transports { get; }
    Task<PhysicalDeviceProbeResult> ProbeAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<TransferResult> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Capability gate used before a transfer adapter can be invoked.</summary>
public sealed class PhysicalAdapterGate
{
    public bool CanProbe(IPhysicalDeviceAdapter adapter, ProjectDevice device)
        => adapter.Transports.Count > 0 && !string.IsNullOrWhiteSpace(device.PhysicalBinding?.Endpoint);

    public bool CanTransfer(IPhysicalDeviceAdapter adapter, ProjectDevice device)
        => CanProbe(adapter, device) && device.PhysicalBinding?.IdentificationState == "Verified";
}
