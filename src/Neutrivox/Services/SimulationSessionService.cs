using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class SimulationSessionService
{
    public SimulationSession Create(AutomationProject project)
    {
        var session = new SimulationSession { ProjectId = project.Id };
        foreach (var device in project.Devices)
            foreach (var channel in device.Channels)
                session.ChannelValues[channel.Id] = channel.Type.Equals("Digital", StringComparison.OrdinalIgnoreCase) ? false : null;
        foreach (var tag in project.Tags)
            session.TagValues[tag.Id] = tag.InitialValue ?? GetDefault(tag.DataType);
        return session;
    }

    public void Start(SimulationSession session)
    {
        if (session.State == SimulationRunState.Stopped)
        {
            session.StartedAtUtc = DateTime.UtcNow;
            session.Events.Add(new(DateTime.UtcNow, "Started", "Simulation started."));
        }
        session.State = SimulationRunState.Running;
    }

    public void Pause(SimulationSession session)
    {
        if (session.State == SimulationRunState.Running) session.State = SimulationRunState.Paused;
    }

    public void Stop(SimulationSession session)
    {
        session.State = SimulationRunState.Stopped;
        session.Events.Add(new(DateTime.UtcNow, "Stopped", "Simulation stopped."));
    }

    public SimulationResult Step(SimulationSession session, AutomationProject project)
    {
        if (session.State is not (SimulationRunState.Running or SimulationRunState.Paused))
            return new(false, "Simulation is not running.", 0);

        session.Cycle++;
        session.Events.Add(new(DateTime.UtcNow, "Cycle", $"Simulation cycle {session.Cycle} completed."));
        return new(true, "Simulation cycle completed.", 1);
    }

    public bool SetChannelValue(SimulationSession session, AutomationProject project, Guid channelId, object? value)
    {
        var channel = project.Devices.SelectMany(x => x.Channels).FirstOrDefault(x => x.Id == channelId);
        if (channel is null || !channel.Direction.Equals("Input", StringComparison.OrdinalIgnoreCase)) return false;
        session.ChannelValues[channelId] = value;
        return true;
    }

    private static object GetDefault(TagDataType type) => type switch
    {
        TagDataType.Boolean => false,
        TagDataType.Integer => 0,
        TagDataType.Number => 0d,
        _ => string.Empty
    };
}
