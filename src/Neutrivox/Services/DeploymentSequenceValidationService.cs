using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentSequenceCheck(PreflightSeverity Severity, int Order, Guid DeviceId, string DeviceName, string Code, string Message);

public sealed class DeploymentSequenceValidationService
{
    public IReadOnlyList<DeploymentSequenceCheck> Validate(AutomationProject project, DeploymentPlan plan)
    {
        var checks = new List<DeploymentSequenceCheck>();
        var ids = new HashSet<Guid>();
        for (var index = 0; index < plan.Targets.Count; index++)
        {
            var target = plan.Targets[index];
            var order = index + 1;
            var device = project.Devices.FirstOrDefault(x => x.Id == target.ProjectDeviceId);
            if (device is null)
            {
                checks.Add(new(PreflightSeverity.Error, order, target.ProjectDeviceId, target.DeviceName, "DEVICE_MISSING", "Устройство исчезло из проекта после создания плана."));
                continue;
            }
            if (!ids.Add(device.Id))
                checks.Add(new(PreflightSeverity.Error, order, device.Id, device.Name, "DUPLICATE_TARGET", "Устройство указано в плане передачи более одного раза."));
            if (string.IsNullOrWhiteSpace(target.Endpoint))
                checks.Add(new(PreflightSeverity.Error, order, device.Id, device.Name, "ENDPOINT_MISSING", "Для выбранного устройства отсутствует конечная точка связи."));
            if (!string.Equals(target.DefinitionId, device.DefinitionId, StringComparison.OrdinalIgnoreCase))
                checks.Add(new(PreflightSeverity.Error, order, device.Id, device.Name, "PROFILE_CHANGED", "Профиль устройства изменился после создания плана."));
        }
        if (plan.Targets.Count == 0)
            checks.Add(new(PreflightSeverity.Error, 0, Guid.Empty, string.Empty, "EMPTY_PLAN", "План передачи не содержит целей."));
        return checks;
    }
}
