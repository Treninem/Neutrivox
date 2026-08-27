using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeploymentPlanningService
{
    private readonly DeploymentReadinessService _readiness = new();

    public DeploymentPreview CreatePreview(AutomationProject project, IEnumerable<Guid> selectedDeviceIds)
    {
        var plan = new DeploymentPlan { ProjectId = project.Id };
        var requested = selectedDeviceIds.Distinct().ToHashSet();
        foreach (var device in project.Devices.Where(x => requested.Contains(x.Id)))
        {
            plan.Targets.Add(new DeploymentTarget
            {
                ProjectDeviceId = device.Id,
                DeviceName = device.Name,
                DefinitionId = device.DefinitionId,
                Endpoint = device.PhysicalBinding?.Endpoint ?? device.Network.IpAddress ?? device.Network.SerialPort,
                IdentificationState = device.PhysicalBinding?.IdentificationState ?? "NotBound"
            });
        }

        var report = _readiness.Analyze(project);
        foreach (var item in report.Items.Where(x => x.Level != ProjectReadinessLevel.Information))
            plan.ValidationMessages.Add($"{item.Code}: {item.Description}");

        if (plan.Targets.Count == 0)
            plan.ValidationMessages.Add("No target devices were selected.");

        plan.State = report.BlockingCount == 0 && plan.Targets.Count > 0
            ? DeploymentState.ReadyForConfirmation
            : DeploymentState.Draft;

        var summary = plan.Targets.Count == 0
            ? "No physical device selected. Nothing will be transferred."
            : $"Prepared sequential plan for {plan.Targets.Count} selected device(s). No transfer has been performed.";

        return new DeploymentPreview(plan, report.Items, true, summary);
    }

    public bool Confirm(DeploymentPlan plan, string confirmedByUser)
    {
        if (plan.State != DeploymentState.ReadyForConfirmation || string.IsNullOrWhiteSpace(confirmedByUser)) return false;
        plan.State = DeploymentState.Confirmed;
        plan.ConfirmedByUser = confirmedByUser.Trim();
        plan.ConfirmedAtUtc = DateTime.UtcNow;
        return true;
    }
}
