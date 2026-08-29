using Neutrivox.Models;

namespace Neutrivox.Services;

public static class BuiltInDeviceCatalog
{
    public static void RegisterDefaults(DeviceCatalogService catalog)
    {
        // Generic examples remain available for projects that do not use a vendor catalog.
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

        RegisterOwenPr100(catalog, "230.0804.01.0", 8, 0, 4, false);
        RegisterOwenPr100(catalog, "230.0804.01.1", 8, 0, 4, true);
        RegisterOwenPr100(catalog, "230.1208.01.0", 12, 0, 8, false);
        RegisterOwenPr100(catalog, "230.1208.01.1", 12, 0, 8, true);
        RegisterOwenPr100(catalog, "24.0804.03.0", 4, 4, 4, false);
        RegisterOwenPr100(catalog, "24.0804.03.1", 4, 4, 4, true);
        RegisterOwenPr100(catalog, "24.1208.03.0", 8, 4, 8, false);
        RegisterOwenPr100(catalog, "24.1208.03.1", 8, 4, 8, true);
    }

    private static void RegisterOwenPr100(
        DeviceCatalogService catalog,
        string variant,
        int digitalInputs,
        int analogInputs,
        int relayOutputs,
        bool rs485)
    {
        var channels = new List<ChannelDefinition>();
        for (var i = 1; i <= digitalInputs; i++)
            channels.Add(new ChannelDefinition($"I{i}", "Digital", "Input"));
        for (var i = 1; i <= analogInputs; i++)
            channels.Add(new ChannelDefinition($"AI{i}", "Analog", "Input"));
        for (var i = 1; i <= relayOutputs; i++)
            channels.Add(new ChannelDefinition($"Q{i}", "Digital", "Output"));

        catalog.Register(new DeviceDefinition(
            $"owen.pr100.{variant}",
            "ОВЕН",
            $"ПР100-{variant}",
            DeviceCategory.Controller,
            channels,
            rs485 ? ["USB", "RS-485"] : ["USB"]));
    }
}
