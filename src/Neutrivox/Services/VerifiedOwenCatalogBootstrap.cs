using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Single registration point for documented OWEN profiles used by discovery and compatibility services.</summary>
public static class VerifiedOwenCatalogBootstrap
{
    public static DeviceProfileRegistry CreateRegistry()
    {
        var registry = new DeviceProfileRegistry();
        VerifiedOwenProfiles.Register(registry);
        VerifiedOwenGatewayProfiles.Register(registry);
        return registry;
    }
}
