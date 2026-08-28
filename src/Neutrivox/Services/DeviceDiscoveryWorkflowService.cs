namespace Neutrivox.Services;

public sealed record DeviceDiscoveryResult(
    bool Success,
    IReadOnlyList<DiscoveredDevice> Devices,
    IReadOnlyList<string> Errors);

/// <summary>Runs registered discovery providers for the explicitly requested scope.</summary>
public sealed class DeviceDiscoveryWorkflowService
{
    private readonly DeviceDiscoveryService _discovery;

    public DeviceDiscoveryWorkflowService(DeviceDiscoveryService discovery) => _discovery = discovery;

    public async Task<DeviceDiscoveryResult> RunAsync(
        string networkScope,
        bool includeEthernet,
        bool includeSerial,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(networkScope))
            return new(false, [], ["A network or serial scope must be specified."]);

        try
        {
            var request = new DiscoveryRequest(networkScope.Trim(), includeEthernet, includeSerial);
            var devices = await _discovery.DiscoverAsync(request, cancellationToken);
            return new(true, devices, []);
        }
        catch (OperationCanceledException)
        {
            return new(false, [], ["Device discovery was cancelled."]);
        }
        catch (Exception ex)
        {
            return new(false, [], [$"Device discovery failed: {ex.Message}"]);
        }
    }
}
