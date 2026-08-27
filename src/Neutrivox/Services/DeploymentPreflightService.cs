using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Performs non-destructive checks before a deployment can be started.</summary>
public sealed class DeploymentPreflightService
{
    private readonly ProjectValidationService _validation;
    private readonly PhysicalDeviceMappingService _mapping;

    public DeploymentPreflightService(ProjectValidationService validation, PhysicalDeviceMappingService mapping)
    {
        _validation = validation;
        _mapping = mapping;
    }

    public PreflightReport Check(AutomationProject project, IEnumerable<Guid> selectedDeviceIds)
    {
        var checks = new List<PreflightCheck>();
        var validation = _validation.Validate(project);
        foreach (var error in validation.Results.Where(x => x.Level == ValidationLevel.Error))
            checks.Add(new PreflightCheck(PreflightSeverity.Error, "PROJECT_VALIDATION", error.Message));

        foreach (var deviceId in selectedDeviceIds.Distinct())
        {
            var device = project.Devices.FirstOrDefault(x => x.Id == deviceId);
            if (device is null)
            {
                checks.Add(new PreflightCheck(PreflightSeverity.Error, "UNKNOWN_PROJECT_DEVICE", "A selected device is not part of the current project."));
                continue;
            }

            if (!_mapping.TryGetBinding(deviceId, out var binding) || binding is null)
                checks.Add(new PreflightCheck(PreflightSeverity.Error, "NOT_MAPPED", $"{device.Name} is not mapped to a physical device."));
            else
                checks.Add(new PreflightCheck(PreflightSeverity.Info, "TARGET", $"{device.Name} → {binding.Endpoint} via {binding.Protocol}"));
        }

        if (!checks.Any(x => x.Severity == PreflightSeverity.Error))
            checks.Add(new PreflightCheck(PreflightSeverity.Info, "READY", "Preflight checks completed. Review targets before deployment."));

        return new PreflightReport(checks);
    }
}

public sealed record PreflightReport(IReadOnlyList<PreflightCheck> Checks)
{
    public bool CanDeploy => Checks.All(x => x.Severity != PreflightSeverity.Error);
}
public sealed record PreflightCheck(PreflightSeverity Severity, string Code, string Message);
public enum PreflightSeverity { Info, Warning, Error }
