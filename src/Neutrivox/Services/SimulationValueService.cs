using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationValue(Guid Id, string Name, string Kind, string DataType, object? Value, string Direction);

/// <summary>Builds one read-only view of current simulation values for the UI.</summary>
public sealed class SimulationValueService
{
    public IReadOnlyList<SimulationValue> GetValues(AutomationProject project, SimulationSession session)
    {
        var result = new List<SimulationValue>();
        foreach (var device in project.Devices)
        foreach (var channel in device.Channels)
        {
            session.ChannelValues.TryGetValue(channel.Id, out var value);
            result.Add(new(channel.Id, $"{device.Name} / {channel.Name}", "I/O", channel.Type, value, channel.Direction));
        }

        foreach (var tag in project.Tags)
        {
            session.TagValues.TryGetValue(tag.Id, out var value);
            result.Add(new(tag.Id, tag.Name, "Tag", tag.DataType.ToString(), value, "Internal"));
        }
        return result;
    }
}
