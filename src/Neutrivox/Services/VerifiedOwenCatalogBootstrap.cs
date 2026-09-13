using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Creates the device-profile registry used by discovery and compatibility services.
/// Multi-vendor family profiles are registered first; exact documented OWEN profiles
/// are registered afterwards so they override broader family entries when ids overlap.
/// </summary>
public static class VerifiedOwenCatalogBootstrap
{
    public static DeviceProfileRegistry CreateRegistry()
    {
        var registry = new DeviceProfileRegistry();
        BuiltInDeviceProfiles.RegisterVerifiedProfiles(registry);
        VerifiedOwenProfiles.Register(registry);
        VerifiedOwenGatewayProfiles.Register(registry);
        return registry;
    }
}
