using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Built-in equipment families known to Neutrivox.
/// A profile describes a family and its integration surface; it does NOT imply
/// that vendor-specific program download is implemented unless explicitly stated.
/// </summary>
public static class BuiltInDeviceProfiles
{
    public static void RegisterVerifiedProfiles(DeviceProfileRegistry registry)
    {
        // OWEN / ОВЕН
        Register(registry, "owen.pr100", "ОВЕН", "ПР100", "ПР100-",
            "Программируемое реле ПР100; точный состав I/O зависит от модификации.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/product/pr100/documentation");
        Register(registry, "owen.pr200", "ОВЕН", "ПР200", "ПР200-",
            "Линейка программируемых реле ПР200.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");
        Register(registry, "owen.plc", "ОВЕН", "ПЛК", "ПЛК",
            "Семейство программируемых логических контроллеров ОВЕН; конкретные интерфейсы определяются моделью.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/");
        Register(registry, "owen.io", "ОВЕН", "Модули ввода/вывода", "М",
            "Модули дискретного и аналогового ввода/вывода ОВЕН. Подбираются по точной модели.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp],
            "https://owen.ru/");
        Register(registry, "owen.pm210", "ОВЕН", "ПМ210", "ПМ210",
            "Коммуникационный модуль. Используется как часть сети оборудования, а не как ПЛК с универсальным I/O.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp],
            "https://owen.ru/");

        // Siemens
        Register(registry, "siemens.s7-1200", "Siemens", "SIMATIC S7-1200", "S7-12",
            "Компактные ПЛК SIMATIC S7-1200; точный CPU и сигнальные модули выбираются отдельно.",
            [DeviceTransport.Ethernet], [DeviceProtocolKind.VendorSpecific], "https://www.siemens.com/");
        Register(registry, "siemens.logo", "Siemens", "LOGO!", "LOGO",
            "Логические модули Siemens LOGO!.", [DeviceTransport.Ethernet],
            [DeviceProtocolKind.VendorSpecific], "https://www.siemens.com/");

        // Schneider Electric
        Register(registry, "schneider.m221", "Schneider Electric", "Modicon M221", "TM221",
            "Компактные контроллеры Modicon M221.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.se.com/");

        // Mitsubishi Electric
        Register(registry, "mitsubishi.fx5", "Mitsubishi Electric", "MELSEC iQ-F", "FX5",
            "Контроллеры MELSEC iQ-F/FX5; возможности зависят от CPU и модулей.",
            [DeviceTransport.Usb, DeviceTransport.Ethernet], [DeviceProtocolKind.VendorSpecific],
            "https://www.mitsubishielectric.com/");

        // Delta
        Register(registry, "delta.dvp", "Delta", "DVP", "DVP",
            "Семейство компактных ПЛК Delta DVP.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.deltaww.com/");

        // Weintek HMI (important for complete automation projects even though it is not a PLC)
        Register(registry, "weintek.hmi", "Weintek", "HMI", "MT",
            "Панели оператора Weintek; используются для HMI/Modbus-связи с контроллерами.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.weintek.com/");
    }

    private static void Register(
        DeviceProfileRegistry registry,
        string id,
        string manufacturer,
        string family,
        string variantPattern,
        string description,
        List<DeviceTransport> transports,
        List<DeviceProtocolKind> protocols,
        string documentation)
    {
        registry.Register(new DeviceProfile
        {
            Id = id,
            Manufacturer = manufacturer,
            ModelFamily = family,
            VariantPattern = variantPattern,
            Description = description,
            SupportLevel = DeviceSupportLevel.ModelProfiled,
            Transports = transports,
            Protocols = protocols,
            Channels =
            [
                new("DI", "Digital", "Input", "Количество и электрический тип определяются точной модификацией."),
                new("DO", "Digital", "Output", "Количество и тип выхода определяются точной модификацией."),
                new("AI", "Analog", "Input", "Только для моделей/модулей с аналоговыми входами."),
                new("AO", "Analog", "Output", "Только для моделей/модулей с аналоговыми выходами.")
            ],
            Capabilities =
            [
                new("Device profile", true, false, "Neutrivox can represent the equipment family in a project."),
                new("Runtime data", true, protocols.Contains(DeviceProtocolKind.ModbusRtu) || protocols.Contains(DeviceProtocolKind.ModbusTcp), "Read/write is enabled only through a separately validated protocol adapter and exact model map."),
                new("Program transfer", false, false, "Vendor program upload/download is disabled until a dedicated, documented and tested adapter exists for the exact family.")
            ],
            DocumentationReference = documentation
        });
    }
}
