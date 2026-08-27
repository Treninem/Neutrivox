using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Stores explicit user-approved mappings between project devices and discovered physical devices.</summary>
public sealed class PhysicalDeviceMappingService
{
    private readonly Dictionary<Guid, PhysicalDeviceBinding> _bindings = [];

    public IReadOnlyCollection<PhysicalDeviceBinding> Bindings => _bindings.Values;

    public MappingResult Bind(ProjectDevice projectDevice, DiscoveredDevice physicalDevice, bool userConfirmed)
    {
        if (!userConfirmed)
            return MappingResult.Rejected("Binding requires explicit confirmation.");

        if (string.IsNullOrWhiteSpace(physicalDevice.Endpoint))
            return MappingResult.Rejected("The discovered device has no usable endpoint.");

        _bindings[projectDevice.Id] = new PhysicalDeviceBinding(
            projectDevice.Id,
            projectDevice.Name,
            physicalDevice.Endpoint,
            physicalDevice.Protocol,
            physicalDevice.Manufacturer,
            physicalDevice.Model,
            DateTime.UtcNow);

        return MappingResult.Accepted();
    }

    public bool Unbind(Guid projectDeviceId) => _bindings.Remove(projectDeviceId);

    public bool TryGetBinding(Guid projectDeviceId, out PhysicalDeviceBinding? binding) => _bindings.TryGetValue(projectDeviceId, out binding);
}

public sealed record PhysicalDeviceBinding(Guid ProjectDeviceId, string ProjectDeviceName, string Endpoint, string Protocol, string? Manufacturer, string? Model, DateTime BoundAtUtc);
public sealed record MappingResult(bool Success, string? Message)
{
    public static MappingResult Accepted() => new(true, null);
    public static MappingResult Rejected(string message) => new(false, message);
}
