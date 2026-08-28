using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentWorkflowItem(int Order, Guid DeviceId, string DeviceName, string Endpoint, string ProfileId, bool CanProceed, string StatusRu, string StatusEn);
public sealed record DeploymentWorkflowPreview(bool CanProceed, IReadOnlyList<DeploymentWorkflowItem> Items, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);

/// <summary>Builds one user-facing deployment workflow from the existing preflight and safety rules.</summary>
public sealed class DeploymentWorkflowService
{
    private readonly DeploymentPreflightService _preflight;
    private readonly DeviceProfileRegistry _profiles;
    private readonly DeploymentAdapterRegistry _adapters;

    public DeploymentWorkflowService(DeploymentPreflightService preflight, DeviceProfileRegistry profiles, DeploymentAdapterRegistry adapters)
    {
        _preflight = preflight;
        _profiles = profiles;
        _adapters = adapters;
    }

    public DeploymentWorkflowPreview BuildPreview(AutomationProject project, IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds.Distinct().ToList();
        var report = _preflight.Check(project, ids);
        var errors = report.Checks.Where(x => x.Severity == PreflightSeverity.Error).Select(x => x.Message).ToList();
        var warnings = report.Checks.Where(x => x.Severity == PreflightSeverity.Warning).Select(x => x.Message).ToList();
        var items = new List<DeploymentWorkflowItem>();
        var order = 1;
        foreach (var id in ids)
        {
            var device = project.Devices.FirstOrDefault(x => x.Id == id);
            if (device is null)
            {
                errors.Add("Selected device is not part of the current project.");
                continue;
            }
            var binding = device.PhysicalBinding;
            if (binding is null)
            {
                errors.Add($"{device.Name}: no physical device is bound.");
                continue;
            }
            var matches = _profiles.Match(binding.Manufacturer ?? string.Empty, binding.Model ?? string.Empty);
            var match = matches.FirstOrDefault(x => x.Confidence >= 0.9);
            var adapter = match is null ? null : _adapters.Find(match.Profile);
            var canProceed = match is not null && adapter is not null && match.Profile.SupportLevel == DeviceSupportLevel.ReadWriteSupported;
            var ru = canProceed ? "Готово к подтверждённой передаче" : "Передача пока недоступна";
            var en = canProceed ? "Ready for confirmed deployment" : "Deployment is not currently available";
            items.Add(new(order++, device.Id, device.Name, binding.Endpoint, match?.Profile.Id ?? "unknown", canProceed, ru, en));
            if (!canProceed) errors.Add($"{device.Name}: a verified read/write deployment adapter is not available.");
        }
        return new(errors.Count == 0 && items.Count > 0, items, errors.Distinct().ToList(), warnings.Distinct().ToList());
    }
}
