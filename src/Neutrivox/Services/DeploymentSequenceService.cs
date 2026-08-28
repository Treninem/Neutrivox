using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentSequenceItem(int Order, DeploymentTarget Target, DeploymentState State, string Message);

public sealed record DeploymentSequencePlan(IReadOnlyList<DeploymentSequenceItem> Items)
{
    public bool HasWork => Items.Count > 0;
}

/// <summary>Builds a deterministic one-device-at-a-time sequence. It does not perform I/O itself.</summary>
public sealed class DeploymentSequenceService
{
    public DeploymentSequencePlan Build(DeploymentPlan plan)
    {
        var items = plan.Targets
            .OrderBy(x => x.DeviceName, StringComparer.OrdinalIgnoreCase)
            .Select((target, index) => new DeploymentSequenceItem(
                index + 1,
                target,
                DeploymentState.Pending,
                $"Step {index + 1}: verify and deploy only to {target.DeviceName} at {target.Endpoint ?? "no endpoint"}."))
            .ToList();
        return new DeploymentSequencePlan(items);
    }
}
