using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class SimulationService
{
    private readonly Dictionary<Guid, bool> _digitalInputs = new();
    private readonly Dictionary<Guid, bool> _digitalOutputs = new();

    public void SetDigitalInput(IoChannel channel, bool value) => _digitalInputs[channel.Id] = value;
    public bool GetDigitalInput(IoChannel channel) => _digitalInputs.TryGetValue(channel.Id, out var value) && value;
    public void SetDigitalOutput(IoChannel channel, bool value) => _digitalOutputs[channel.Id] = value;
    public bool GetDigitalOutput(IoChannel channel) => _digitalOutputs.TryGetValue(channel.Id, out var value) && value;

    public SimulationSnapshot CreateSnapshot(AutomationProject project)
    {
        var states = project.Devices.SelectMany(d => d.Channels).Select(channel => new ChannelState(
            channel.Id,
            channel.Name,
            channel.Type,
            channel.Direction,
            channel.Direction.Equals("Input", StringComparison.OrdinalIgnoreCase) ? GetDigitalInput(channel) : GetDigitalOutput(channel)))
            .ToList();
        return new SimulationSnapshot(DateTime.UtcNow, states);
    }

    public void Reset()
    {
        _digitalInputs.Clear();
        _digitalOutputs.Clear();
    }
}

public sealed record SimulationSnapshot(DateTime TimestampUtc, IReadOnlyList<ChannelState> Channels);
public sealed record ChannelState(Guid ChannelId, string Name, string Type, string Direction, bool DigitalValue);