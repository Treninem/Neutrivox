using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectBindingService
{
    public BindingResult Bind(ProjectDevice projectDevice, DiscoveredDevice physicalDevice, bool confirmedByUser)
    {
        if (!confirmedByUser)
            return new(false, "Binding requires explicit user confirmation.");
        if (string.IsNullOrWhiteSpace(physicalDevice.Endpoint))
            return new(false, "The discovered device has no usable endpoint.");

        projectDevice.PhysicalBinding = new Models.PhysicalDeviceBinding
        {
            Endpoint = physicalDevice.Endpoint,
            Manufacturer = physicalDevice.Manufacturer,
            Model = physicalDevice.Model,
            IdentificationState = physicalDevice.IdentificationState,
            LastSeenUtc = DateTime.UtcNow
        };
        return new(true, "Physical device bound to project device.");
    }

    public void Unbind(ProjectDevice projectDevice) => projectDevice.PhysicalBinding = null;
}

public sealed record BindingResult(bool Success, string Message);