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
/// Executes a prepared deployment sequence strictly in the explicit plan order.
/// The service does not discover devices and never treats a profile as writable by itself.
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
        var orderedTargets = plan.Targets
            .OrderBy(x => x.Order)
            .ThenBy(x => x.ProjectDeviceId)
            .ToList();

        var expectedOrder = 1;
        foreach (var target in orderedTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (target.Order != expectedOrder)
            {
                var invalid = new DeploymentExecutionItem(
                    target.Order,
                    target.ProjectDeviceId,
                    target.DeviceName,
                    DeploymentState.Failed,
                    $"Invalid deployment order. Expected {expectedOrder}, got {target.Order}.",
                    []);
                results.Add(invalid);
                if (progress is not null) await progress(target.Order, invalid);
                break;
            }

            var context = contextFactory(target.ProjectDeviceId);
            if (context is null)
            {
                var failed = new DeploymentExecutionItem(target.Order, target.ProjectDeviceId, target.DeviceName, DeploymentState.Failed, "Deployment context is missing.", []);
                results.Add(failed);
                if (progress is not null) await progress(target.Order, failed);
                break;
            }

            if (!context.UserConfirmed)
            {
                var blocked = new DeploymentExecutionItem(target.Order, target.ProjectDeviceId, target.DeviceName, DeploymentState.Cancelled, "User confirmation is required.", []);
                results.Add(blocked);
                if (progress is not null) await progress(target.Order, blocked);
                return new(false, true, results, "Deployment stopped because explicit user confirmation was not provided.");
            }

            var adapter = _adapters.Find(context.Profile);
            if (adapter is null)
            {
                var unsupported = new DeploymentExecutionItem(target.Order, target.ProjectDeviceId, target.DeviceName, DeploymentState.Skipped, "No tested deployment adapter is registered for this device profile.", []);
                results.Add(unsupported);
                if (progress is not null) await progress(target.Order, unsupported);
                break;
            }

            var steps = await adapter.ExecuteAsync(context, cancellationToken);
            var success = steps.Count > 0 && steps.All(x => x.Success);
            var state = success ? DeploymentState.Completed : DeploymentState.Failed;
            var message = success ? $"Device deployment completed for target #{target.Order}." : $"Device deployment failed for target #{target.Order}.";
            var item = new DeploymentExecutionItem(target.Order, target.ProjectDeviceId, target.DeviceName, state, message, steps);
            results.Add(item);
            if (progress is not null) await progress(target.Order, item);
            if (!success) break;
            expectedOrder++;
        }

        var overall = orderedTargets.Count > 0 && orderedTargets.Count == results.Count && results.All(x => x.State == DeploymentState.Completed);
        return new(overall, false, results, overall ? "All deployment targets completed successfully in the planned order." : "Deployment completed with a failure or stopped target.");
    }
}
