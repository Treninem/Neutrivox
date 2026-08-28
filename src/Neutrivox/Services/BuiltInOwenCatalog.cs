using Neutrivox.Models;

namespace Neutrivox.Services;

public static class BuiltInOwenCatalog
{
    public static void Register(DocumentedDeviceCatalog catalog)
    {
        catalog.Register(new DocumentedDevice(
            "owen.pr100.230.0804.01.0",
            "ОВЕН",
            "ПР100-230.0804.01.0",
            "Programmable relay",
            "230 V supply; 8 digital inputs (DF); 4 relay outputs; no RS-485."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.230.0804.01.1",
            "ОВЕН",
            "ПР100-230.0804.01.1",
            "Programmable relay",
            "230 V supply; 8 digital inputs (DF); 4 relay outputs; RS-485 available."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.230.1208.01.0",
            "ОВЕН",
            "ПР100-230.1208.01.0",
            "Programmable relay",
            "230 V supply; 12 digital inputs (DF); 8 relay outputs; no RS-485."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.230.1208.01.1",
            "ОВЕН",
            "ПР100-230.1208.01.1",
            "Programmable relay",
            "230 V supply; 12 digital inputs (DF); 8 relay outputs; RS-485 available."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.24.0804.03.0",
            "ОВЕН",
            "ПР100-24.0804.03.0",
            "Programmable relay",
            "24 V supply; 4 digital inputs; 4 universal analog inputs; 4 relay outputs; no RS-485."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.24.0804.03.1",
            "ОВЕН",
            "ПР100-24.0804.03.1",
            "Programmable relay",
            "24 V supply; 4 digital inputs; 4 universal analog inputs; 4 relay outputs; RS-485 available."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.24.1208.03.0",
            "ОВЕН",
            "ПР100-24.1208.03.0",
            "Programmable relay",
            "24 V supply; 8 digital inputs; 4 universal analog inputs; 8 relay outputs; no RS-485."));
        catalog.Register(new DocumentedDevice(
            "owen.pr100.24.1208.03.1",
            "ОВЕН",
            "ПР100-24.1208.03.1",
            "Programmable relay",
            "24 V supply; 8 digital inputs; 4 universal analog inputs; 8 relay outputs; RS-485 available."));
    }
}

public sealed class DocumentedDeviceCatalog
{
    private readonly Dictionary<string, DocumentedDevice> _devices = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<DocumentedDevice> Devices => _devices.Values;
    public void Register(DocumentedDevice device) => _devices[device.Id] = device;
}

public sealed record DocumentedDevice(string Id, string Manufacturer, string Model, string Category, string Capabilities);