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

        // Exact documented OWEN PR100 variants.
        RegisterOwenPr100(catalog, "230.0804.01.0", 8, 0, 4, false);
        RegisterOwenPr100(catalog, "230.0804.01.1", 8, 0, 4, true);
        RegisterOwenPr100(catalog, "230.1208.01.0", 12, 0, 8, false);
        RegisterOwenPr100(catalog, "230.1208.01.1", 12, 0, 8, true);
        RegisterOwenPr100(catalog, "24.0804.03.0", 4, 4, 4, false);
        RegisterOwenPr100(catalog, "24.0804.03.1", 4, 4, 4, true);
        RegisterOwenPr100(catalog, "24.1208.03.0", 8, 4, 8, false);
        RegisterOwenPr100(catalog, "24.1208.03.1", 8, 4, 8, true);

        // OWEN communication gateways are not PLCs and intentionally have no local universal I/O channels.
        RegisterGateway(catalog, "owen.pm210", "ОВЕН", "ПМ210", ["RS-485", "USB", "Cellular 2G/4G", "OwenCloud"]);
        RegisterGateway(catalog, "owen.pe210", "ОВЕН", "ПЕ210", ["RS-485", "USB", "Ethernet", "OwenCloud"]);
        RegisterGateway(catalog, "owen.pv210", "ОВЕН", "ПВ210", ["RS-485", "USB", "Wi-Fi", "OwenCloud"]);

        // Family-level planning profiles. I/O is intentionally empty until an exact hardware variant is selected.
        RegisterFamily(catalog, "owen.pr200.family", "ОВЕН", "ПР200 — семейство", ["USB", "RS-485"]);
        RegisterFamily(catalog, "owen.plc110-m02.family", "ОВЕН", "ПЛК110 [М02] — семейство", ["RS-485", "Ethernet"]);
        RegisterFamily(catalog, "owen.plc160-m02.family", "ОВЕН", "ПЛК160 [М02] — семейство", ["RS-485", "Ethernet"]);
        RegisterFamily(catalog, "owen.plc200.family", "ОВЕН", "ПЛК200 — семейство", ["RS-485", "Ethernet"]);
        RegisterFamily(catalog, "owen.plc210.family", "ОВЕН", "ПЛК210 — семейство", ["RS-485", "Ethernet"]);

        RegisterFamily(catalog, "siemens.s7-1200-g2.family", "Siemens", "SIMATIC S7-1200 G2 — семейство", ["Ethernet", "PROFINET"]);
        RegisterFamily(catalog, "siemens.s7-1500.family", "Siemens", "SIMATIC S7-1500 — семейство", ["Ethernet", "PROFINET"]);
        RegisterFamily(catalog, "schneider.m221.family", "Schneider Electric", "Modicon M221 — семейство", ["USB", "RS-485", "Ethernet"]);
        RegisterFamily(catalog, "schneider.m241.family", "Schneider Electric", "Modicon M241 — семейство", ["USB", "RS-485", "Ethernet"]);
        RegisterFamily(catalog, "mitsubishi.fx5.family", "Mitsubishi Electric", "MELSEC iQ-F / FX5 — семейство", ["USB", "Ethernet"]);
        RegisterFamily(catalog, "omron.cp2e.family", "OMRON", "CP2E — семейство", ["USB", "RS-485", "Ethernet"]);
        RegisterFamily(catalog, "omron.nx1p2.family", "OMRON", "NX1P2 — семейство", ["Ethernet", "EtherCAT"]);
        RegisterFamily(catalog, "delta.dvp.family", "Delta", "DVP — семейство", ["RS-485", "Ethernet"]);
        RegisterFamily(catalog, "delta.as.family", "Delta", "AS Series — семейство", ["RS-485", "Ethernet", "CANopen"]);
        RegisterFamily(catalog, "wago.pfc200.family", "WAGO", "PFC200 — семейство", ["Ethernet", "Fieldbus"]);
        RegisterFamily(catalog, "rockwell.micro800.family", "Allen-Bradley", "Micro800 — семейство", ["USB", "RS-485", "Ethernet"]);
        RegisterFamily(catalog, "beckhoff.cx.family", "Beckhoff", "CX Embedded PC — семейство", ["Ethernet", "EtherCAT"]);
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

    private static void RegisterGateway(
        DeviceCatalogService catalog,
        string id,
        string manufacturer,
        string model,
        IReadOnlyList<string> interfaces)
    {
        catalog.Register(new DeviceDefinition(
            id,
            manufacturer,
            model,
            DeviceCategory.CommunicationModule,
            [],
            interfaces));
    }

    private static void RegisterFamily(
        DeviceCatalogService catalog,
        string id,
        string manufacturer,
        string model,
        IReadOnlyList<string> interfaces)
    {
        catalog.Register(new DeviceDefinition(
            id,
            manufacturer,
            model,
            DeviceCategory.Controller,
            [],
            interfaces));
    }
}
