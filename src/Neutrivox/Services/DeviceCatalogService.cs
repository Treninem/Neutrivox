using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeviceCatalogService
{
    private readonly List<DeviceDefinition> _devices = [];

    public IReadOnlyList<DeviceDefinition> Devices => _devices;

    public void Register(DeviceDefinition definition)
    {
        if (_devices.Any(x => x.Id == definition.Id))
            throw new InvalidOperationException($"Device '{definition.Id}' is already registered.");
        _devices.Add(definition);
    }

    public DeviceDefinition? Find(string id) => _devices.FirstOrDefault(x => x.Id == id);
}
