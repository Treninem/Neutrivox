using System.Runtime.CompilerServices;
using Neutrivox.Models;
using Neutrivox.Services;

internal static class DeviceCatalogSmokeChecks
{
    [ModuleInitializer]
    internal static void Run()
    {
        var profiles = VerifiedOwenCatalogBootstrap.CreateRegistry();

        Require(profiles.TryGet("owen.pm210", out var pm210), "PM210 profile is missing.");
        Require(pm210!.Transports.Contains(DeviceTransport.Cellular), "PM210 must expose a cellular uplink.");
        Require(!pm210.Transports.Contains(DeviceTransport.Ethernet), "PM210 must not be modeled as the Ethernet gateway.");

        Require(profiles.TryGet("owen.pe210-230", out var pe210), "PE210 profile is missing.");
        Require(pe210!.Transports.Contains(DeviceTransport.Ethernet), "PE210 Ethernet transport is missing.");

        Require(profiles.TryGet("owen.pv210-24", out var pv210), "PV210 profile is missing.");
        Require(pv210!.Transports.Contains(DeviceTransport.Wifi), "PV210 Wi-Fi transport is missing.");

        Require(profiles.TryGet("owen.plc210", out _), "OWEN PLC210 family profile is missing.");
        Require(profiles.TryGet("siemens.s7-1200-g2", out _), "Siemens S7-1200 G2 family profile is missing.");
        Require(profiles.TryGet("schneider.m221", out _), "Schneider Modicon M221 family profile is missing.");
        Require(profiles.TryGet("mitsubishi.fx5", out _), "Mitsubishi FX5 family profile is missing.");
        Require(profiles.TryGet("omron.cp2e", out _), "OMRON CP2E family profile is missing.");
        Require(profiles.TryGet("delta.dvp", out _), "Delta DVP family profile is missing.");
        Require(profiles.TryGet("wago.pfc200", out _), "WAGO PFC200 family profile is missing.");
        Require(profiles.TryGet("rockwell.micro800", out _), "Allen-Bradley Micro800 family profile is missing.");
        Require(profiles.TryGet("beckhoff.cx", out _), "Beckhoff CX family profile is missing.");

        var catalog = new DeviceCatalogService();
        BuiltInDeviceCatalog.RegisterDefaults(catalog);
        Require(catalog.Find("owen.pm210")?.Category == DeviceCategory.CommunicationModule,
            "PM210 must be exposed in the project catalog as a communication module.");
        Require(catalog.Devices.Select(x => x.Manufacturer).Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 8,
            "The project catalog must remain multi-vendor.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
