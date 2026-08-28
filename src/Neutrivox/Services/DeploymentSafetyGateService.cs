using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentSafetyResult(bool Allowed, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);

/// <summary>
/// Final non-destructive gate before a future physical deployment adapter is invoked.
/// It deliberately refuses to treat a profile as writable merely because the device is discovered.
/// </summary>
public sealed class DeploymentSafetyGateService
{
    private readonly DeploymentPreflightService _preflight;
    private readonly DeploymentAdapterRegistry _adapters;
    private readonly DeviceProfileRegistry _profiles;

    public DeploymentSafetyGateService(
        DeploymentPreflightService preflight,
        DeploymentAdapterRegistry adapters,
        DeviceProfileRegistry profiles)
    {
        _preflight = preflight;
        _adapters = adapters;
        _profiles = profiles;
    }

    public DeploymentSafetyResult Evaluate(AutomationProject project, IEnumerable<Guid> selectedDeviceIds, bool explicitConfirmation)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        if (!explicitConfirmation)
            errors.Add("Physical deployment requires explicit user confirmation.");

        var ids = selectedDeviceIds.Distinct().ToList();
        var preflight = _preflight.Check(project, ids);
        errors.AddRange(preflight.Checks.Where(x => x.Severity == PreflightSeverity.Error).Select(x => x.Message));
        warnings.AddRange(preflight.Checks.Where(x => x.Severity == PreflightSeverity.Warning).Select(x => x.Message));

        foreach (var id in ids)
        {
            var device = project.Devices.FirstOrDefault(x => x.Id == id);
            if (device is null) continue;
            if (device.PhysicalBinding is null) continue;

            var profileMatches = _profiles.Match(device.PhysicalBinding.Manufacturer ?? string.Empty, device.PhysicalBinding.Model ?? string.Empty);
            var profile = profileMatches.FirstOrDefault(x => x.Confidence >= 0.9)?.Profile;
            if (profile is null)
            {
                errors.Add($"{device.Name}: no sufficiently confident documented device profile was found.");
                continue;
            }

            if (profile.SupportLevel != DeviceSupportLevel.ReadWriteSupported)
                errors.Add($"{device.Name}: profile '{profile.Id}' is documented, but physical read/write support is not implemented and verified.");

            if (_adapters.Find(profile) is null)
                errors.Add($"{device.Name}: no deployment adapter is registered for profile '{profile.Id}'.");
        }

        return new(errors.Count == 0, errors.Distinct().ToList(), warnings.Distinct().ToList());
    }
}
