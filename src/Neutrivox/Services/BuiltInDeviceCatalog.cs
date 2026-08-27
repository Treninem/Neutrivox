using Neutrivox.Models;

namespace Neutrivox.Services;

public static class BuiltInDeviceCatalog
{
    public static void RegisterDefaults(DeviceCatalogService catalog)
    {
        // Generic examples only. Real vendor-specific profiles are added after
        // verification against official technical documentation.
        catalog.Register(new DeviceDefinition(
            "generic-controller-8io",
            "Neutrivox Demo",
            "Controller 8 I/O",
            DeviceCategory.Controller,
            [
                new ChannelDefinition("DI1", "Digital", "Input"),
                new ChannelDefinition("DI2", "Digital", "Input"),
                new ChannelDefinition("DI3", "Digital", "Input"),
                new ChannelDefinition("DI4", "Digital", "Input"),
                new ChannelDefinition("DO1", "Digital", "Output"),
                new ChannelDefinition("DO2", "Digital", "Output"),
                new ChannelDefinition("AI1", "Analog", "Input"),
                new ChannelDefinition("AO1", "Analog", "Output")
            ],
            ["Ethernet", "RS-485"]));

        catalog.Register(new DeviceDefinition(
            "generic-digital-module-16",
            "Neutrivox Demo",
            "Digital Module 16",
            DeviceCategory.ExpansionModule,
            Enumerable.Range(1, 16)
                .Select(i => new ChannelDefinition($"DI{i}", "Digital", "Input"))
                .ToArray(),
            ["Expansion bus"]));
    }
}
