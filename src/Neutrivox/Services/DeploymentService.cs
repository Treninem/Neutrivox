using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeploymentService
{
    public DeploymentPlan CreatePlan(AutomationProject project, IEnumerable<ProjectDevice> devices)
    {
        var steps = devices.Select((device, index) => new DeploymentStep(index + 1, device.Id, device.Name, DeploymentState.Pending)).ToList();
        return new DeploymentPlan(project.Id, DateTime.UtcNow, steps);
    }

    public void MarkStep(DeploymentPlan plan, Guid deviceId, DeploymentState state, string? message = null)
    {
        var step = plan.Steps.FirstOrDefault(x => x.DeviceId == deviceId)
            ?? throw new InvalidOperationException("Deployment step not found.");
        step.State = state;
        step.Message = message;
    }
}

public sealed class DeploymentPlan(Guid projectId, DateTime createdAtUtc, List<DeploymentStep> steps)
{
    public Guid ProjectId { get; } = projectId;
    public DateTime CreatedAtUtc { get; } = createdAtUtc;
    public List<DeploymentStep> Steps { get; } = steps;
}

public sealed class DeploymentStep(int order, Guid deviceId, string deviceName, DeploymentState state)
{
    public int Order { get; } = order;
    public Guid DeviceId { get; } = deviceId;
    public string DeviceName { get; } = deviceName;
    public DeploymentState State { get; set; } = state;
    public string? Message { get; set; }
}

public enum DeploymentState { Pending, Verified, Ready, Running, Completed, Failed, Skipped }