using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentAuditEntry(
    DateTime TimestampUtc,
    Guid PlanId,
    Guid ProjectDeviceId,
    string DeviceName,
    DeploymentState State,
    string Message);

/// <summary>Records an append-only in-memory audit trail for deployment operations.</summary>
public sealed class DeploymentAuditService
{
    private readonly List<DeploymentAuditEntry> _entries = [];
    public IReadOnlyList<DeploymentAuditEntry> Entries => _entries;

    public void Record(DeploymentPlan plan, DeploymentExecutionItem item)
        => _entries.Add(new(DateTime.UtcNow, plan.Id, item.DeviceId, item.DeviceName, item.State, item.Message));

    public IReadOnlyList<DeploymentAuditEntry> ForPlan(Guid planId)
        => _entries.Where(x => x.PlanId == planId).ToList();
}
