namespace Neutrivox.Models;

public sealed class SimulationTrace
{
    public List<SimulationTraceEntry> Entries { get; } = [];
    public int Cycle { get; set; }
    public void Add(string category, string message, Guid? instructionId = null) => Entries.Add(new SimulationTraceEntry(DateTime.UtcNow, Cycle, category, message, instructionId));
    public void Clear() { Entries.Clear(); Cycle = 0; }
}

public sealed record SimulationTraceEntry(DateTime TimestampUtc, int Cycle, string Category, string Message, Guid? InstructionId);
