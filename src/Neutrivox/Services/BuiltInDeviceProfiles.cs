using Neutrivox.Models;

namespace Neutrivox.Services;

public static class BuiltInDeviceProfiles
{
    public static void RegisterVerifiedProfiles(DeviceProfileRegistry registry)
    {
        registry.Register(new DeviceProfile
        {
            Id = "owen.pr100",
            Manufacturer = "ОВЕН",
            ModelFamily = "ПР100",
            VariantPattern = "ПР100-",
            Description = "Компактное программируемое реле. Набор I/O зависит от конкретной модификации.",
            SupportLevel = DeviceSupportLevel.ModelProfiled,
            Transports = [DeviceTransport.Usb, DeviceTransport.SerialRs485],
            Protocols = [DeviceProtocolKind.ModbusRtu, DeviceProtocolKind.ModbusAscii],
            Channels =
            [
                new("I1…I12", "Digital", "Input", "Дискретные входы; количество и тип зависят от модификации."),
                new("AI", "Analog", "Input", "Универсальные аналоговые входы присутствуют только у соответствующих модификаций."),
                new("Q1…Q8", "Digital", "Output", "Релейные выходы; количество зависит от модификации."),
                new("F1/F2", "Digital", "Output", "Дополнительные дискретные выходы для поддерживаемых модификаций.")
            ],
            Capabilities =
            [
                new("Modbus registers", true, true, "Register access must follow the documented function/register map and device state constraints."),
                new("Digital I/O", true, true, "Exact channels depend on the selected PR100 variant."),
                new("Program transfer", false, false, "Not claimed here: program upload format/protocol requires dedicated documented integration and test." )
            ],
            DocumentationReference = "https://owen.ru/product/pr100/documentation"
        });
    }
}
