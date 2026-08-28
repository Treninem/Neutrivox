using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Profiles backed by currently available official OWEN product documentation.
/// A profile describes documented facts; it does not claim program upload support
/// unless that operation has been separately implemented and tested.
/// </summary>
public static class VerifiedOwenProfiles
{
    public static void Register(DeviceProfileRegistry registry)
    {
        RegisterPr100(registry, "230.0804.01.0", 8, 0, 4, false);
        RegisterPr100(registry, "230.0804.01.1", 8, 0, 4, true);
        RegisterPr100(registry, "230.1208.01.0", 12, 0, 8, false);
        RegisterPr100(registry, "230.1208.01.1", 12, 0, 8, true);
        RegisterPr100(registry, "24.0804.03.0", 4, 4, 4, false);
        RegisterPr100(registry, "24.0804.03.1", 4, 4, 4, true);
        RegisterPr100(registry, "24.1208.03.0", 8, 4, 8, false);
        RegisterPr100(registry, "24.1208.03.1", 8, 4, 8, true);

        registry.Register(new DeviceProfile
        {
            Id = "owen.pm210",
            Manufacturer = "ОВЕН",
            ModelFamily = "ПМ210",
            VariantPattern = "ПМ210-",
            Description = "Сетевой шлюз для подключения приборов по RS-485/Modbus и работы с OwenCloud.",
            SupportLevel = DeviceSupportLevel.ModelProfiled,
            Transports = [DeviceTransport.SerialRs485, DeviceTransport.Usb],
            Protocols = [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            Capabilities =
            [
                new("RS-485 field connection", true, true, "Official documentation describes RS-485 connection to devices using Modbus RTU/ASCII."),
                new("USB configuration", true, true, "USB is documented for configuration/maintenance."),
                new("PLC program transfer", false, false, "Not claimed. PM210 is a network gateway, not the controller-program upload target in this profile.")
            ],
            DocumentationReference = "https://owen.ru/product/pm210"
        });
    }

    private static void RegisterPr100(DeviceProfileRegistry registry, string variant, int digitalInputs, int analogInputs, int relayOutputs, bool rs485)
    {
        var id = $"owen.pr100.{variant.Replace('.', '_')}";
        var channels = new List<DeviceProfileChannel>
        {
            new($"DI1…DI{digitalInputs}", "Digital", "Input", $"Documented digital inputs for variant ПР100-{variant}."),
            new($"Q1…Q{relayOutputs}", "Digital", "Output", $"Documented relay outputs for variant ПР100-{variant}.")
        };
        if (analogInputs > 0)
            channels.Insert(1, new($"AI1…AI{analogInputs}", "Analog", "Input", $"Documented universal analog inputs for variant ПР100-{variant}."));

        registry.Register(new DeviceProfile
        {
            Id = id,
            Manufacturer = "ОВЕН",
            ModelFamily = "ПР100",
            VariantPattern = $"ПР100-{variant}",
            Description = $"Точная документированная модификация ПР100-{variant}.",
            SupportLevel = DeviceSupportLevel.ModelProfiled,
            Transports = rs485 ? [DeviceTransport.SerialRs485, DeviceTransport.Usb] : [DeviceTransport.Usb],
            Protocols = rs485 ? [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii] : [DeviceProtocolKind.None],
            Channels = channels,
            Capabilities =
            [
                new("Digital I/O", true, true, $"Variant-specific channel counts: DI={digitalInputs}, AI={analogInputs}, relay outputs={relayOutputs}."),
                new("RS-485", rs485, rs485, rs485 ? "RS-485 is documented for this variant." : "This variant is documented without RS-485."),
                new("Program transfer", false, false, "Not claimed until the documented Owen Logic transfer path is separately implemented and tested.")
            ],
            DocumentationReference = "https://owen.ru/product/pr100/documentation"
        });
    }
}
