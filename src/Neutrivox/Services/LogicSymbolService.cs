using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LogicSymbol(string Name, string Category, TagDataType DataType, bool Writable);

public sealed class LogicSymbolService
{
    public IReadOnlyList<LogicSymbol> GetSymbols(AutomationProject project)
    {
        var result = new List<LogicSymbol>();
        foreach (var device in project.Devices)
        foreach (var channel in device.Channels)
            result.Add(new(channel.Name, $"{device.Name} / I/O", InferType(channel.Type), channel.Direction.Equals("Output", StringComparison.OrdinalIgnoreCase)));
        foreach (var tag in project.Tags)
            result.Add(new(tag.Name, "Tags", tag.DataType, true));
        foreach (var variable in project.Logic.Variables)
            result.Add(new(variable.Name, "Logic variables", variable.DataType, true));
        return result.OrderBy(x => x.Category).ThenBy(x => x.Name).ToList();
    }

    private static TagDataType InferType(string type) => type.Contains("Analog", StringComparison.OrdinalIgnoreCase)
        ? TagDataType.Number : TagDataType.Boolean;
}
