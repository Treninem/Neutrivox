using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Documented communication-gateway profiles for the OwenCloud family.</summary>
public static class VerifiedOwenGatewayProfiles
{
    public static void Register(DeviceProfileRegistry registry)
    {
        RegisterGateway(registry, "pm210", "ПМ210", "ПМ210-230", [DeviceTransport.SerialRs485, DeviceTransport.Usb]);
        RegisterGateway(registry, "pm210-24", "ПМ210", "ПМ210-24", [DeviceTransport.SerialRs485, DeviceTransport.Usb]);
        RegisterGateway(registry, "pm210-230-4g", "ПМ210", "ПМ210-230.4G", [DeviceTransport.SerialRs485, DeviceTransport.Usb]);
        RegisterGateway(registry, "pe210-230", "ПЕ210", "ПЕ210-230", [DeviceTransport.SerialRs485, DeviceTransport.Usb, DeviceTransport.Ethernet]);
        RegisterGateway(registry, "pe210-24", "ПЕ210", "ПЕ210-24", [DeviceTransport.SerialRs485, DeviceTransport.Usb, DeviceTransport.Ethernet]);
        RegisterGateway(registry, "pv210-230", "ПВ210", "ПВ210-230", [DeviceTransport.SerialRs485, DeviceTransport.Usb]);
        RegisterGateway(registry, "pv210-24", "ПВ210", "ПВ210-24", [DeviceTransport.SerialRs485, DeviceTransport.Usb]);
    }

    private static void RegisterGateway(DeviceProfileRegistry registry, string idSuffix, string family, string variant, IReadOnlyList<DeviceTransport> transports)
    {
        registry.Register(new DeviceProfile
        {
            Id = $"owen.{idSuffix}",
            Manufacturer = "ОВЕН",
            ModelFamily = family,
            VariantPattern = variant,
            Description = $"Документированный сетевой шлюз {variant} для связи приборов по RS-485 с OwenCloud.",
            SupportLevel = DeviceSupportLevel.ModelProfiled,
            Transports = transports.ToList(),
            Protocols = [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            Capabilities =
            [
                new("RS-485 / Modbus field bus", true, true, "The gateway documentation describes RS-485 connection to devices using Modbus RTU/ASCII."),
                new("Gateway configuration", true, true, "Configuration interface depends on the exact gateway variant."),
                new("Controller program transfer", false, false, "A gateway profile is not a claim that controller programs can be uploaded through this device.")
            ],
            DocumentationReference = "https://owen.ru/product/pm210"
        });
    }
}
