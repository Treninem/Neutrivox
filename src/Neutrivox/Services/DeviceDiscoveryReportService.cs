using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceDiscoveryReportItem(
    string Endpoint,
    string Protocol,
    string IdentificationState,
    string Status,
    string TitleRu,
    string TitleEn,
    string DetailsRu,
    string DetailsEn);

public sealed class DeviceDiscoveryReportService
{
    private readonly DeviceBindingExplanationService _explanations = new();
    private readonly DeviceProfileRegistry _profiles;

    public DeviceDiscoveryReportService(DeviceProfileRegistry profiles) => _profiles = profiles;

    public IReadOnlyList<DeviceDiscoveryReportItem> Build(
        ProjectDevice? target,
        IEnumerable<DiscoveredDevice> discovered)
    {
        var result = new List<DeviceDiscoveryReportItem>();
        foreach (var device in discovered)
        {
            DeviceBindingCandidate candidate;
            if (target is null)
            {
                candidate = new(device, null, null);
            }
            else
            {
                var identity = new DeviceIdentificationService(_profiles)
                    .Identify([new DiscoveryObservation(device.Endpoint, device.Protocol, device.Manufacturer, device.Model, device.Protocol, string.Empty, DateTime.UtcNow)])
                    .FirstOrDefault();
                DeviceProfileMatch? match = null;
                if (identity is not null && (!string.IsNullOrWhiteSpace(identity.Observation.Manufacturer) || !string.IsNullOrWhiteSpace(identity.Observation.Model)))
                {
                    match = _profiles.Match(identity.Observation.Manufacturer ?? string.Empty, identity.Observation.Model ?? string.Empty).FirstOrDefault();
                }
                var compatibility = match is null ? null : new DeviceCompatibilityService().Check(target, match.Profile);
                candidate = new(device, match, compatibility);
            }

            var explanation = _explanations.Explain(candidate);
            result.Add(new(
                device.Endpoint,
                device.Protocol,
                device.IdentificationState,
                explanation.Status,
                explanation.TitleRu,
                explanation.TitleEn,
                explanation.DetailsRu,
                explanation.DetailsEn));
        }
        return result;
    }
}
