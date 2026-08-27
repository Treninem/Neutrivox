namespace Neutrivox.Models;

public enum SimulationRunState { Stopped, Running, Paused, Faulted }

public sealed class SimulationSession
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid ProjectId { get; init; }
    public SimulationRunState State { get; set; } = SimulationRunState.Stopped;
    public long Cycle { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public Dictionary<Guid, object?> ChannelValues { get; } = [];
    public Dictionary<Guid, object?> TagValues { get; } = [];
    public List<SimulationEvent> Events { get; } = [];
}

public sealed record SimulationEvent(DateTime TimestampUtc, string Kind, string Message);
public sealed record SimulationResult(bool Success, string Message, long CyclesExecuted);