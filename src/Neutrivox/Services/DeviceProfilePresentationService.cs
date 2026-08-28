using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceProfileCard(
    string ProfileId,
    string Manufacturer,
    string Model,
    string Support,
    IReadOnlyList<string> Transports,
    IReadOnlyList<string> Protocols,
    IReadOnlyList<string> Channels,
    IReadOnlyList<string> Capabilities,
    string DocumentationReference);

public sealed class DeviceProfilePresentationService
{
    public IReadOnlyList<DeviceProfileCard> Build(DeviceProfileRegistry registry)
        => registry.Profiles.OrderBy(x => x.Manufacturer).ThenBy(x => x.ModelFamily)
            .Select(x => new DeviceProfileCard(
                x.Id,
                x.Manufacturer,
                x.ModelFamily,
                x.SupportLevel.ToString(),
                x.Transports.Select(t => t.ToString()).ToList(),
                x.Protocols.Select(p => p.ToString()).ToList(),
                x.Channels.Select(c => $"{c.Name} — {c.Type} / {c.Direction}").ToList(),
                x.Capabilities.Select(c => $"{c.Name}: {(c.Readable ? "read" : "no read")}, {(c.Writable ? "write" : "no write")}").ToList(),
                x.DocumentationReference))
            .ToList();
}
