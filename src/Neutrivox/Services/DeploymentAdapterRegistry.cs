using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentContext(ProjectDevice Device, DeviceProfile Profile, bool UserConfirmed, string ProjectPayload);
public sealed record DeploymentStepResult(string Name, bool Success, string Message);

public interface IDeploymentAdapter
{
    string AdapterId { get; }
    bool CanHandle(DeviceProfile profile);
    Task<IReadOnlyList<DeploymentStepResult>> ExecuteAsync(DeploymentContext context, CancellationToken cancellationToken);
}

public sealed class DeploymentAdapterRegistry
{
    private readonly List<IDeploymentAdapter> _adapters = [];

    public void Register(IDeploymentAdapter adapter) => _adapters.Add(adapter);

    public IDeploymentAdapter? Find(DeviceProfile profile) => _adapters.FirstOrDefault(x => x.CanHandle(profile));

    public IReadOnlyList<IDeploymentAdapter> Adapters => _adapters;
}

/// <summary>
/// Non-destructive adapter used for preflight and integration tests. It never writes to hardware.
/// </summary>
public sealed class DryRunDeploymentAdapter : IDeploymentAdapter
{
    public string AdapterId => "dry-run";
    public bool CanHandle(DeviceProfile profile) => false;

    public Task<IReadOnlyList<DeploymentStepResult>> ExecuteAsync(DeploymentContext context, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<DeploymentStepResult>>([
            new("Preflight", true, $"Dry-run only for {context.Device.Name}. No physical write was performed.")
        ]);
}
