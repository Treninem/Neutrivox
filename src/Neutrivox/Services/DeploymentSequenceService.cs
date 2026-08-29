using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentSequenceItem(int Order, DeploymentTarget Target, DeploymentState State, string Message);

public sealed record DeploymentSequencePlan(IReadOnlyList<DeploymentSequenceItem> Items)
{
    public bool HasWork => Items.Count > 0;
}

/// <summary>Builds the exact one-device-at-a-time sequence defined by DeploymentTarget.Order.</summary>
public sealed class DeploymentSequenceService
{
    public DeploymentSequencePlan Build(DeploymentPlan plan)
    {
        var ordered = plan.Targets.OrderBy(x => x.Order).ToList();
        var items = ordered.Select((target, index) =>
        {
            var expected = index + 1;
            var validOrder = target.Order == expected;
            var message = validOrder
                ? $"Step {expected}: verify and deploy only to {target.DeviceName} at {target.Endpoint ?? "no endpoint"}."
                : $"Invalid deployment order: expected {expected}, got {target.Order} for {target.DeviceName}.";
            return new DeploymentSequenceItem(expected, target, DeploymentState.Pending, message);
        }).ToList();

        return new DeploymentSequencePlan(items);
    }
}
