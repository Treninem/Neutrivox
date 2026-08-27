using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class SimulationTraceService
{
    public void RecordCycleStart(SimulationTrace trace, int cycle) { trace.Cycle = cycle; trace.Add("Simulation", "Simulation cycle started."); }
    public void RecordInstruction(SimulationTrace trace, LogicInstruction instruction, string description) => trace.Add("Logic", description, instruction.Id);
    public void RecordError(SimulationTrace trace, string message, Guid? instructionId = null) => trace.Add("Error", message, instructionId);
    public string CreateSummary(SimulationTrace trace)
    {
        var errors = trace.Entries.Count(x => x.Category == "Error");
        var logic = trace.Entries.Count(x => x.Category == "Logic");
        return $"Cycles: {trace.Cycle}; logic events: {logic}; errors: {errors}.";
    }
}
