using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceDiscoveryCandidate(
    DiscoveredDevice Device,
    IReadOnlyList<DeviceProfileMatch> ProfileMatches,
    bool CanBind,
    string Message);

public sealed record DeviceDiscoveryScanResult(
    bool Success,
    string Scope,
    IReadOnlyList<DeviceDiscoveryCandidate> Candidates,
    IReadOnlyList<string> Errors);

/// <summary>
/// Coordinates endpoint validation, discovery and documented profile matching.
/// It deliberately does not bind devices automatically.
/// </summary>
public sealed class DeviceDiscoveryCoordinator
{
    private readonly NetworkScopeService _scope = new();
    private readonly DeviceDiscoveryWorkflowService _discovery;
    private readonly DeviceProfileRegistry _profiles;

    public DeviceDiscoveryCoordinator(DeviceDiscoveryWorkflowService discovery, DeviceProfileRegistry profiles)
    {
        _discovery = discovery;
        _profiles = profiles;
    }

    public async Task<DeviceDiscoveryScanResult> ScanAsync(string scope, CancellationToken cancellationToken = default)
    {
        var scopeResult = _scope.Validate(scope);
        if (!scopeResult.Valid)
            return new(false, scope, [], [scopeResult.Error ?? "Invalid network scope."]);

        var result = await _discovery.RunAsync(scopeResult.Normalized, true, true, cancellationToken);
        if (!result.Success)
            return new(false, scopeResult.Normalized, [], result.Errors);

        var candidates = result.Devices.Select(device =>
        {
            var matches = _profiles.Match(device.Manufacturer ?? string.Empty, device.Model ?? string.Empty);
            var best = matches.FirstOrDefault();
            return new DeviceDiscoveryCandidate(
                device,
                matches,
                best is not null && best.Confidence >= 0.9 && device.IdentificationState.Equals("Verified", StringComparison.OrdinalIgnoreCase),
                best is null ? "No documented profile matched this device." : $"Best documented profile confidence: {best.Confidence:P0}.");
        }).ToList();

        return new(true, scopeResult.Normalized, candidates, []);
    }
}
