using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentContext(
    AutomationProject Project,
    ProjectDevice Target,
    DeviceProfile Profile,
    string Endpoint,
    bool UserConfirmed,
    IReadOnlyDictionary<string, object?> Parameters);

public sealed record DeploymentStepResult(
    bool Success,
    string Step,
    string Message,
    TimeSpan Duration,
    bool ConfigurationWritten);

public interface IDeviceDeploymentAdapter
{
    string AdapterId { get; }
    bool Supports(DeviceProfile profile);
    Task<IReadOnlyList<DeploymentStepResult>> ExecuteAsync(DeploymentContext context, CancellationToken cancellationToken = default);
}

/// <summary>Finds one adapter for one verified profile. Unsupported transfer is an explicit result.</summary>
public sealed class DeploymentAdapterRegistry
{
    private readonly List<IDeviceDeploymentAdapter> _adapters = [];
    public void Register(IDeviceDeploymentAdapter adapter) => _adapters.Add(adapter);
    public IDeviceDeploymentAdapter? Find(DeviceProfile profile) => _adapters.LastOrDefault(x => x.Supports(profile));
}
