using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Runs non-destructive checks against the current project and its physical bindings.</summary>
public sealed class DeploymentPreflightService
{
    private readonly ProjectValidationService _validation;
    private readonly DeviceCatalogService _catalog;

    public DeploymentPreflightService()
        : this(new ProjectValidationService(), CreateCatalog())
    {
    }

    public DeploymentPreflightService(ProjectValidationService validation, DeviceCatalogService catalog)
    {
        _validation = validation;
        _catalog = catalog;
    }

    public PreflightReport Check(AutomationProject project, IEnumerable<Guid> selectedDeviceIds)
    {
        var checks = new List<PreflightCheck>();
        var ids = selectedDeviceIds.Distinct().ToList();

        var validation = _validation.Validate(project, _catalog);
        foreach (var issue in validation.Issues)
        {
            var severity = issue.Severity == ValidationSeverity.Error
                ? PreflightSeverity.Error
                : PreflightSeverity.Warning;
            checks.Add(new PreflightCheck(severity, issue.Code, issue.Message));
        }

        if (ids.Count == 0)
            checks.Add(new PreflightCheck(PreflightSeverity.Error, "NO_TARGETS", "At least one deployment target must be selected."));

        foreach (var deviceId in ids)
        {
            var device = project.Devices.FirstOrDefault(x => x.Id == deviceId);
            if (device is null)
            {
                checks.Add(new PreflightCheck(
                    PreflightSeverity.Error,
                    "UNKNOWN_PROJECT_DEVICE",
                    "A selected device is not part of the current project."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(device.DefinitionId))
            {
                checks.Add(new PreflightCheck(
                    PreflightSeverity.Error,
                    "MISSING_DEFINITION",
                    $"{device.Name}: device definition is not specified."));
            }
            else if (_catalog.Find(device.DefinitionId) is null)
            {
                checks.Add(new PreflightCheck(
                    PreflightSeverity.Error,
                    "UNKNOWN_DEVICE",
                    $"{device.Name}: unknown device definition '{device.DefinitionId}'."));
            }

            var binding = device.PhysicalBinding;
            if (binding is null)
            {
                checks.Add(new PreflightCheck(
                    PreflightSeverity.Error,
                    "NOT_MAPPED",
                    $"{device.Name} is not mapped to a physical device."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(binding.Endpoint))
                checks.Add(new PreflightCheck(PreflightSeverity.Error, "EMPTY_ENDPOINT", $"{device.Name}: physical endpoint is empty."));

            if (string.IsNullOrWhiteSpace(binding.Manufacturer) || string.IsNullOrWhiteSpace(binding.Model))
                checks.Add(new PreflightCheck(PreflightSeverity.Warning, "INCOMPLETE_IDENTIFICATION", $"{device.Name}: manufacturer/model identification is incomplete."));

            checks.Add(new PreflightCheck(
                PreflightSeverity.Info,
                "TARGET",
                $"{device.Name} → {binding.Endpoint} ({binding.Manufacturer ?? "?"} / {binding.Model ?? "?"}), identification={binding.IdentificationState}"));
        }

        if (!checks.Any(x => x.Severity == PreflightSeverity.Error))
            checks.Add(new PreflightCheck(PreflightSeverity.Info, "READY", "Preflight checks completed. Review targets before deployment."));

        return new PreflightReport(checks);
    }

    private static DeviceCatalogService CreateCatalog()
    {
        var catalog = new DeviceCatalogService();
        BuiltInDeviceCatalog.RegisterDefaults(catalog);
        return catalog;
    }
}

public sealed record PreflightReport(IReadOnlyList<PreflightCheck> Checks)
{
    public bool CanDeploy => Checks.All(x => x.Severity != PreflightSeverity.Error);
}

public sealed record PreflightCheck(PreflightSeverity Severity, string Code, string Message);

public enum PreflightSeverity
{
    Info,
    Warning,
    Error
}
