using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SequentialDeploymentResult(
    bool Success,
    IReadOnlyList<DeploymentStepResult> Steps,
    IReadOnlyList<string> Errors);

/// <summary>Processes deployment targets strictly one at a time and stops on a failed target.</summary>
public sealed class SequentialDeploymentService
{
    private readonly DeploymentAdapterRegistry _adapters;
    private readonly DeviceCompatibilityService _compatibility = new();

    public SequentialDeploymentService(DeploymentAdapterRegistry adapters) => _adapters = adapters;

    public async Task<SequentialDeploymentResult> ExecuteAsync(
        AutomationProject project,
        IReadOnlyList<(ProjectDevice Device, DeviceProfile Profile)> targets,
        string confirmedByUser,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(confirmedByUser))
            return new(false, [], ["Deployment requires explicit user confirmation."]);

        var steps = new List<DeploymentStepResult>();
        var errors = new List<string>();
        foreach (var (device, profile) in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var compatibility = _compatibility.Check(device, profile);
            if (!compatibility.Compatible)
            {
                errors.Add($"{device.Name}: project device is incompatible with {profile.Manufacturer} {profile.ModelFamily}.");
                break;
            }

            var endpoint = device.PhysicalBinding?.Endpoint ?? device.Network.IpAddress ?? device.Network.SerialPort;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                errors.Add($"{device.Name}: no physical endpoint is assigned.");
                break;
            }

            var adapter = _adapters.Find(profile);
            if (adapter is null)
            {
                errors.Add($"{device.Name}: no deployment adapter is registered for {profile.Manufacturer} {profile.ModelFamily}.");
                break;
            }

            var results = await adapter.ExecuteAsync(new DeploymentContext(
                project, device, profile, endpoint, true,
                new Dictionary<string, object?> { ["ConfirmedByUser"] = confirmedByUser.Trim() }), cancellationToken);
            steps.AddRange(results);
            if (results.Any(x => !x.Success))
            {
                errors.Add($"{device.Name}: deployment adapter reported a failure.");
                break;
            }
        }

        return new(errors.Count == 0, steps, errors);
    }
}
