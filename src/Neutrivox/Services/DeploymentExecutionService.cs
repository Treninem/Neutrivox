using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentExecutionItem(
    int Order,
    Guid DeviceId,
    string DeviceName,
    DeploymentState State,
    string Message,
    IReadOnlyList<DeploymentStepResult> Steps);

public sealed record DeploymentExecutionResult(
    bool Success,
    bool UserCancelled,
    IReadOnlyList<DeploymentExecutionItem> Items,
    string Summary);

/// <summary>
/// Executes a prepared deployment sequence strictly one target at a time.
/// The service itself does not discover devices and never treats a profile as a writable adapter.
/// </summary>
public sealed class DeploymentExecutionService
{
    private readonly DeploymentAdapterRegistry _adapters;

    public DeploymentExecutionService(DeploymentAdapterRegistry adapters) => _adapters = adapters;

    public async Task<DeploymentExecutionResult> ExecuteAsync(
        AutomationProject project,
        DeploymentPlan plan,
        Func<Guid, DeploymentContext?> contextFactory,
        Func<int, DeploymentExecutionItem, Task>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DeploymentExecutionItem>();
        var index = 0;
        foreach (var target in plan.Targets.OrderBy(x => x.ProjectDeviceId))
        {
            index++;
            cancellationToken.ThrowIfCancellationRequested();
            var context = contextFactory(target.ProjectDeviceId);
            if (context is null)
            {
                var failed = new DeploymentExecutionItem(index, target.ProjectDeviceId, target.DeviceName, DeploymentState.Failed, "Deployment context is missing.", []);
                results.Add(failed);
                if (progress is not null) await progress(index, failed);
                continue;
            }

            if (!context.UserConfirmed)
            {
                var blocked = new DeploymentExecutionItem(index, target.ProjectDeviceId, target.DeviceName, DeploymentState.Cancelled, "User confirmation is required.", []);
                results.Add(blocked);
                if (progress is not null) await progress(index, blocked);
                return new(false, true, results, "Deployment stopped because explicit user confirmation was not provided.");
            }

            var adapter = _adapters.Find(context.Profile);
            if (adapter is null)
            {
                var unsupported = new DeploymentExecutionItem(index, target.ProjectDeviceId, target.DeviceName, DeploymentState.Skipped, "No tested deployment adapter is registered for this device profile.", []);
                results.Add(unsupported);
                if (progress is not null) await progress(index, unsupported);
                continue;
            }

            var steps = await adapter.ExecuteAsync(context, cancellationToken);
            var success = steps.Count > 0 && steps.All(x => x.Success);
            var state = success ? DeploymentState.Completed : DeploymentState.Failed;
            var message = success ? "Device deployment completed." : "Device deployment failed.";
            var item = new DeploymentExecutionItem(index, target.ProjectDeviceId, target.DeviceName, state, message, steps);
            results.Add(item);
            if (progress is not null) await progress(index, item);
            if (!success) break;
        }

        var overall = results.Count > 0 && results.All(x => x.State == DeploymentState.Completed);
        return new(overall, false, results, overall ? "All deployment targets completed successfully." : "Deployment completed with failures or skipped targets.");
    }
}
