using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Central registry for profiles whose capabilities have been explicitly documented.</summary>
public sealed class DeviceProfileRegistry
{
    private readonly Dictionary<string, DeviceProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<DeviceProfile> Profiles => _profiles.Values;

    public void Register(DeviceProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Id)) throw new ArgumentException("Profile id is required.", nameof(profile));
        _profiles[profile.Id] = profile;
    }

    public bool TryGet(string id, out DeviceProfile? profile) => _profiles.TryGetValue(id, out profile);

    public IReadOnlyList<DeviceProfileMatch> Match(string manufacturer, string model)
    {
        var result = new List<DeviceProfileMatch>();
        foreach (var profile in _profiles.Values)
        {
            var score = 0d;
            if (string.Equals(profile.Manufacturer, manufacturer, StringComparison.OrdinalIgnoreCase)) score += 0.5;
            if (!string.IsNullOrWhiteSpace(model) && model.Contains(profile.ModelFamily, StringComparison.OrdinalIgnoreCase)) score += 0.4;
            if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(profile.VariantPattern) && model.Contains(profile.VariantPattern, StringComparison.OrdinalIgnoreCase)) score += 0.1;
            if (score > 0) result.Add(new(profile, Math.Min(1, score), "Matched against documented manufacturer/model identifiers."));
        }
        return result.OrderByDescending(x => x.Confidence).ToList();
    }
}
