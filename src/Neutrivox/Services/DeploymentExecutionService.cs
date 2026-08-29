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
/// The plan must still match the current project immediately before physical I/O starts.
/// </summary>
public sealed class DeploymentExecutionService
{
    private readonly DeploymentAdapterRegistry _adapters;
    private readonly DeploymentPlanGuardService _guard;
    private readonly DeploymentSafetyGateService _safetyGate;

    public DeploymentExecutionService(
        DeploymentAdapterRegistry adapters,
        DeploymentPlanGuardService guard,
        DeploymentSafetyGateService safetyGate)
    {
        _adapters = adapters;
        _guard = guard;
        _safetyGate = safetyGate;
    }

    public async Task<DeploymentExecutionResult> ExecuteAsync(
        AutomationProject project,
        DeploymentPlan plan,
        Func<Guid, DeploymentContext?> contextFactory,
        string confirmedByUser,
        Func<int, DeploymentExecutionItem, Task>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var orderedTargets = plan.Targets
            .OrderBy(x => x.Order)
            .ThenBy(x => x.ProjectDeviceId)
            .ToList();

        if (orderedTargets.Count == 0)
            return new(false, false, [], "Deployment plan contains no targets.");

        var expectedOrders = Enumerable.Range(1, orderedTargets.Count).ToArray();
        if (!orderedTargets.Select(x => x.Order).SequenceEqual(expectedOrders))
            return new(false, false, [], "Deployment plan order is invalid. Targets must be numbered consecutively from 1.");

        if (plan.ProjectId != project.Id)
            return new(false, false, [], "Deployment plan belongs to a different project.");

        if (string.IsNullOrWhiteSpace(plan.PlanFingerprint))
            return new(false, false, [], "Deployment plan has no integrity fingerprint. Prepare the plan again.");

        var snapshot = new DeploymentPlanSnapshot(plan.PlanFingerprint, plan.CreatedAtUtc, orderedTargets.Select(x => x.ProjectDeviceId).ToList());
        var guardResult = _guard.Validate(project, snapshot);
        if (!guardResult.IsCurrent)
            return new(false, false, [], guardResult.MessageEn + " " + string.Join(" ", guardResult.Errors));

        var selectedIds = orderedTargets.Select(x => x.ProjectDeviceId).ToList();
        var safety = _safetyGate.Evaluate(project, selectedIds, explicitConfirmation: true);
        if (!safety.Allowed)
            return new(false, false, [], "Deployment blocked: " + string.Join(" ", safety.Errors));

        plan.State = DeploymentState.Confirmed;
        plan.ConfirmedByUser = confirmedByUser;
        plan.ConfirmedAtUtc = DateTime.UtcNow;

        var results = new List<DeploymentExecutionItem>();
        foreach (var target in orderedTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentGuard = _guard.Validate(project, snapshot);
            if (!currentGuard.IsCurrent)
            {
                var blocked = new DeploymentExecutionItem(
                    target.Order,
                    target.ProjectDeviceId,
                    target.DeviceName,
                    DeploymentState.Failed,
                    currentGuard.MessageEn,
                    []);
                results.Add(blocked);
                if (progress is not null) await progress(target.Order, blocked);
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
                var unsupported = new DeploymentExecutionItem(
                    target.Order,
                    target.ProjectDeviceId,
                    target.DeviceName,
                    DeploymentState.Failed,
                    "No tested deployment adapter is registered for this device profile.",
                    []);
                results.Add(unsupported);
                if (progress is not null) await progress(target.Order, unsupported);
                break;
            }

            var steps = await adapter.ExecuteAsync(context, cancellationToken);
            var success = steps.Count > 0 && steps.All(x => x.Success);
            var state = success ? DeploymentState.Completed : DeploymentState.Failed;
            var message = success
                ? $"Device deployment completed for target #{target.Order}."
                : $"Device deployment failed for target #{target.Order}.";
            var item = new DeploymentExecutionItem(target.Order, target.ProjectDeviceId, target.DeviceName, state, message, steps);
            results.Add(item);
            if (progress is not null) await progress(target.Order, item);
            if (!success) break;
        }

        var overall = orderedTargets.Count == results.Count && results.All(x => x.State == DeploymentState.Completed);
        if (overall) plan.State = DeploymentState.Completed;
        return new(overall, false, results,
            overall
                ? "All deployment targets completed successfully in the planned order."
                : "Deployment completed with a failure or stopped target.");
    }
}
