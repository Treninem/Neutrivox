using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Built-in equipment families known to Neutrivox.
/// A family profile is intentionally conservative: it describes the integration
/// surface but does not claim exact I/O counts or vendor program transfer support.
/// Exact-model profiles override these family entries when available.
/// </summary>
public static class BuiltInDeviceProfiles
{
    public static void RegisterVerifiedProfiles(DeviceProfileRegistry registry)
    {
        RegisterOwen(registry);
        RegisterSiemens(registry);
        RegisterSchneider(registry);
        RegisterMitsubishi(registry);
        RegisterOmron(registry);
        RegisterDelta(registry);
        RegisterWago(registry);
        RegisterRockwell(registry);
        RegisterBeckhoff(registry);
        RegisterWeintek(registry);
    }

    private static void RegisterOwen(DeviceProfileRegistry registry)
    {
        Register(registry, "owen.pr100", "ОВЕН", "ПР100", "ПР100-",
            "Моноблочное программируемое реле ПР100; точный состав I/O зависит от модификации.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/product/pr100/documentation");
        Register(registry, "owen.pr102", "ОВЕН", "ПР102", "ПР102-",
            "Модульное программируемое реле ПР102.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");
        Register(registry, "owen.pr103", "ОВЕН", "ПР103", "ПР103-",
            "Модульное программируемое реле ПР103.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");
        Register(registry, "owen.pr200", "ОВЕН", "ПР200", "ПР200-",
            "Программируемое реле ПР200 с дисплеем.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");
        Register(registry, "owen.pr205", "ОВЕН", "ПР205", "ПР205-",
            "Программируемое реле ПР205 с дисплеем.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");
        Register(registry, "owen.pr225", "ОВЕН", "ПР225", "ПР225-",
            "Программируемое реле ПР225 с дисплеем.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");

        Register(registry, "owen.plc110-m02", "ОВЕН", "ПЛК110 [М02]", "ПЛК110",
            "Общепромышленный ПЛК для малых и средних систем.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information");
        Register(registry, "owen.plc160-m02", "ОВЕН", "ПЛК160 [М02]", "ПЛК160",
            "Общепромышленный ПЛК для малых и средних систем.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information");
        Register(registry, "owen.plc200", "ОВЕН", "ПЛК200", "ПЛК200",
            "ПЛК для стандартных производственных процессов малых и средних систем.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.OpcUa, DeviceProtocolKind.Mqtt, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information");
        Register(registry, "owen.plc210", "ОВЕН", "ПЛК210", "ПЛК210",
            "ПЛК для средних и распределённых систем автоматизации.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.OpcUa, DeviceProtocolKind.Mqtt, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information");
        Register(registry, "owen.plc210-4g", "ОВЕН", "ПЛК210-4G", "ПЛК210-4G",
            "Модификация ПЛК210 со встроенной сотовой связью.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet, DeviceTransport.Cellular],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.OpcUa, DeviceProtocolKind.Mqtt, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information");
        Register(registry, "owen.spk", "ОВЕН", "СПК", "СПК",
            "Панельные контроллеры ОВЕН СПК1xx/СПК210.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/catalog/programmiruemie_logicheskie_kontrolleri/info/general_information");

        Register(registry, "owen.mx110", "ОВЕН", "Мх110", "М",
            "Семейство удалённых модулей ввода/вывода по RS-485; точная функция определяется моделью МВ/МУ/МД/МВА.",
            [DeviceTransport.SerialRs485],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            "https://owen.ru/");
        Register(registry, "owen.mx210", "ОВЕН", "Мх210", "М",
            "Семейство Ethernet-модулей ввода/вывода; точная функция определяется моделью.",
            [DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusTcp],
            "https://owen.ru/");
        Register(registry, "owen.prm", "ОВЕН", "ПРМ", "ПРМ-",
            "Модули расширения для программируемых реле ПР.",
            [DeviceTransport.Fieldbus],
            [DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/");

        Register(registry, "owen.pm210", "ОВЕН", "ПМ210", "ПМ210",
            "Сетевой шлюз OwenCloud: приборы подключаются по RS-485/Modbus, внешний канал связи — сотовая сеть.",
            [DeviceTransport.SerialRs485, DeviceTransport.Usb, DeviceTransport.Cellular],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/product/pm210");
        Register(registry, "owen.pe210", "ОВЕН", "ПЕ210", "ПЕ210",
            "Сетевой шлюз OwenCloud с Ethernet и RS-485/Modbus.",
            [DeviceTransport.SerialRs485, DeviceTransport.Usb, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/product/pm210");
        Register(registry, "owen.pv210", "ОВЕН", "ПВ210", "ПВ210",
            "Сетевой шлюз OwenCloud с Wi-Fi и RS-485/Modbus.",
            [DeviceTransport.SerialRs485, DeviceTransport.Usb, DeviceTransport.Wifi],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii, DeviceProtocolKind.VendorSpecific],
            "https://owen.ru/product/pm210");
    }

    private static void RegisterSiemens(DeviceProfileRegistry registry)
    {
        Register(registry, "siemens.logo", "Siemens", "LOGO!", "LOGO",
            "Логические модули Siemens LOGO!.",
            [DeviceTransport.Ethernet], [DeviceProtocolKind.VendorSpecific],
            "https://www.siemens.com/");
        Register(registry, "siemens.s7-1200-g2", "Siemens", "SIMATIC S7-1200 G2", "S7-12",
            "Компактные ПЛК SIMATIC S7-1200 G2 для базовой автоматизации.",
            [DeviceTransport.Ethernet], [DeviceProtocolKind.Profinet, DeviceProtocolKind.S7, DeviceProtocolKind.VendorSpecific],
            "https://www.siemens.com/en-gb/products/simatic/s7-1200-g2/");
        Register(registry, "siemens.s7-1500", "Siemens", "SIMATIC S7-1500", "S7-15",
            "Модульные высокопроизводительные ПЛК SIMATIC S7-1500.",
            [DeviceTransport.Ethernet], [DeviceProtocolKind.Profinet, DeviceProtocolKind.S7, DeviceProtocolKind.OpcUa, DeviceProtocolKind.VendorSpecific],
            "https://www.siemens.com/en-gb/products/simatic/s7-1500/");
    }

    private static void RegisterSchneider(DeviceProfileRegistry registry)
    {
        Register(registry, "schneider.zelio", "Schneider Electric", "Zelio Logic", "SR",
            "Интеллектуальные реле Zelio Logic SR2/SR3.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485], [DeviceProtocolKind.VendorSpecific],
            "https://www.se.com/");
        Register(registry, "schneider.m221", "Schneider Electric", "Modicon M221", "TM221",
            "Компактные контроллеры Modicon M221.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.se.com/ww/en/product-range/62128-modicon-m221/");
        Register(registry, "schneider.m241", "Schneider Electric", "Modicon M241", "TM241",
            "Контроллеры Modicon M241 для машин с повышенными требованиями к производительности.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.se.com/");
        Register(registry, "schneider.m251", "Schneider Electric", "Modicon M251", "TM251",
            "Контроллеры Modicon M251 для модульных и распределённых архитектур.",
            [DeviceTransport.Usb, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.se.com/");
        Register(registry, "schneider.m262", "Schneider Electric", "Modicon M262", "TM262",
            "IIoT-ready контроллеры логики и движения Modicon M262.",
            [DeviceTransport.Usb, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.OpcUa, DeviceProtocolKind.VendorSpecific],
            "https://www.se.com/");
    }

    private static void RegisterMitsubishi(DeviceProfileRegistry registry)
    {
        Register(registry, "mitsubishi.fx5", "Mitsubishi Electric", "MELSEC iQ-F / FX5", "FX5",
            "Компактные контроллеры MELSEC iQ-F, включая FX5U/FX5UC.",
            [DeviceTransport.Usb, DeviceTransport.Ethernet],
            [DeviceProtocolKind.VendorSpecific],
            "https://www.mitsubishielectric.com/fa/products/cnt/plcf/items/index.html");
    }

    private static void RegisterOmron(DeviceProfileRegistry registry)
    {
        Register(registry, "omron.cp2e", "OMRON", "CP2E", "CP2E-",
            "Компактные micro PLC OMRON CP2E; интерфейсы зависят от исполнения E/N.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.VendorSpecific],
            "https://www.ia.omron.com/products/family/3777/");
        Register(registry, "omron.nx1p2", "OMRON", "NX1P2", "NX1P2-",
            "Контроллеры OMRON NX1P2 с интегрированными функциями машинного управления.",
            [DeviceTransport.Ethernet],
            [DeviceProtocolKind.EtherCat, DeviceProtocolKind.EtherNetIp, DeviceProtocolKind.VendorSpecific],
            "https://www.ia.omron.com/products/family/3650/");
    }

    private static void RegisterDelta(DeviceProfileRegistry registry)
    {
        Register(registry, "delta.dvp", "Delta", "DVP", "DVP",
            "Семейство компактных ПЛК Delta DVP.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.VendorSpecific],
            "https://www.deltaww.com/en-US/products/PLC-Programmable-Logic-Controllers/");
        Register(registry, "delta.as", "Delta", "AS Series", "AS",
            "Компактные модульные контроллеры Delta AS Series.",
            [DeviceTransport.SerialRs485, DeviceTransport.Ethernet, DeviceTransport.CanBus],
            [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.CanOpen, DeviceProtocolKind.VendorSpecific],
            "https://www.deltaww.com/en-US/products/PLC-Programmable-Logic-Controllers/");
        Register(registry, "delta.ah", "Delta", "AH Series", "AH",
            "Модульные ПЛК Delta AH для более крупных систем.",
            [DeviceTransport.Ethernet, DeviceTransport.Fieldbus],
            [DeviceProtocolKind.EtherCat, DeviceProtocolKind.VendorSpecific],
            "https://www.deltaww.com/en-GB/products/PLC-Programmable-Logic-Controllers");
    }

    private static void RegisterWago(DeviceProfileRegistry registry)
    {
        Register(registry, "wago.pfc200", "WAGO", "PFC200", "750-82",
            "Модульные контроллеры WAGO PFC200 для промышленной, процессной и зданийной автоматизации.",
            [DeviceTransport.Ethernet, DeviceTransport.Fieldbus],
            [DeviceProtocolKind.ModbusTcp, DeviceProtocolKind.OpcUa, DeviceProtocolKind.Mqtt, DeviceProtocolKind.VendorSpecific],
            "https://www.wago.com/global/automation-technology/discover-plcs/pfc200");
    }

    private static void RegisterRockwell(DeviceProfileRegistry registry)
    {
        Register(registry, "rockwell.micro800", "Allen-Bradley", "Micro800", "2080-",
            "Семейство компактных контроллеров Allen-Bradley Micro800.",
            [DeviceTransport.Usb, DeviceTransport.SerialRs485, DeviceTransport.Ethernet],
            [DeviceProtocolKind.EtherNetIp, DeviceProtocolKind.VendorSpecific],
            "https://www.rockwellautomation.com/en-gb/support/documentation/technical/programmable-controllers/micro-800-controllers.html");
        Register(registry, "rockwell.compactlogix5380", "Allen-Bradley", "CompactLogix 5380", "5069-L",
            "Контроллеры CompactLogix 5380 для машинной автоматизации.",
            [DeviceTransport.Ethernet],
            [DeviceProtocolKind.EtherNetIp, DeviceProtocolKind.VendorSpecific],
            "https://www.rockwellautomation.com/");
    }

    private static void RegisterBeckhoff(DeviceProfileRegistry registry)
    {
        Register(registry, "beckhoff.cx", "Beckhoff", "CX Embedded PC", "CX",
            "DIN-рейковые промышленные ПК/контроллеры Beckhoff CX под TwinCAT.",
            [DeviceTransport.Ethernet, DeviceTransport.Fieldbus],
            [DeviceProtocolKind.EtherCat, DeviceProtocolKind.VendorSpecific],
            "https://www.beckhoff.com/en-en/products/ipc/embedded-pcs/");
    }

    private static void RegisterWeintek(DeviceProfileRegistry registry)
    {
        Register(registry, "weintek.hmi", "Weintek", "HMI", "MT",
            "Панели оператора Weintek для HMI и связи с контроллерами.",
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
                new("Device family profile", true, false, "Neutrivox can identify and represent this equipment family in a project."),
                new("Runtime data", true, protocols.Contains(DeviceProtocolKind.ModbusRtu) || protocols.Contains(DeviceProtocolKind.ModbusTcp), "Read/write is enabled only through a separately validated adapter and exact model map."),
                new("Program transfer", false, false, "Vendor program upload/download is disabled until a dedicated, documented and tested adapter exists for the exact family.")
            ],
            DocumentationReference = documentation
        });
    }
}
