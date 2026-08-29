using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Backward-compatible facade over the canonical Models.DeploymentPlan.
/// The project no longer maintains a second deployment-plan model.
/// </summary>
public sealed class DeploymentService
{
    public DeploymentPlan CreatePlan(AutomationProject project, IEnumerable<ProjectDevice> devices)
    {
        var plan = new DeploymentPlan { ProjectId = project.Id, State = DeploymentState.Draft };
        var order = 1;
        foreach (var device in devices.DistinctBy(x => x.Id))
        {
            plan.Targets.Add(new DeploymentTarget
            {
                Order = order++,
                ProjectDeviceId = device.Id,
                DeviceName = device.Name,
                DefinitionId = device.DefinitionId,
                Endpoint = device.PhysicalBinding?.Endpoint ?? device.Network.IpAddress ?? device.Network.SerialPort,
                IdentificationState = device.PhysicalBinding?.IdentificationState ?? "NotBound"
            });
        }
        if (plan.Targets.Count > 0) plan.State = DeploymentState.Validated;
        return plan;
    }

    public void MarkStep(DeploymentPlan plan, Guid deviceId, DeploymentState state, string? message = null)
    {
        if (state == DeploymentState.Failed && !string.IsNullOrWhiteSpace(message))
            plan.ValidationMessages.Add(message);

        if (state == DeploymentState.Completed && plan.Targets.All(x => x.ProjectDeviceId != deviceId))
            throw new InvalidOperationException("Deployment target not found.");
    }
}
