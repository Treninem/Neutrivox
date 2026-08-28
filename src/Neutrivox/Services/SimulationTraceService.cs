using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Centralizes human-readable simulation tracing without coupling tracing to the UI.</summary>
public sealed class SimulationTraceService
{
    public void RecordCycleStart(SimulationTrace trace, int cycle)
    {
        trace.Cycle = cycle;
        trace.Add("Simulation", $"Simulation cycle {cycle} started.");
    }

    public void RecordValidation(SimulationTrace trace, ProjectWorkflowValidationResult result)
    {
        foreach (var issue in result.Issues)
        {
            var category = issue.Severity switch
            {
                LogicValidationSeverity.Error => "Error",
                LogicValidationSeverity.Warning => "Warning",
                _ => "Validation"
            };
            trace.Add(category, $"{issue.Area}: {issue.Message}");
        }
    }

    public void RecordInstruction(SimulationTrace trace, LogicInstruction instruction, string description)
        => trace.Add("Logic", description, instruction.Id);

    public void RecordExecution(SimulationTrace trace, LogicExecutionResult result)
    {
        trace.Add(result.Success ? "Logic" : "Error", $"Executed {result.ExecutedInstructions} instruction(s).");
        foreach (var error in result.Errors) trace.Add("Error", error);
    }

    public void RecordError(SimulationTrace trace, string message, Guid? instructionId = null)
        => trace.Add("Error", message, instructionId);

    public string CreateSummary(SimulationTrace trace)
    {
        var errors = trace.Entries.Count(x => x.Category == "Error");
        var logic = trace.Entries.Count(x => x.Category == "Logic");
        var warnings = trace.Entries.Count(x => x.Category == "Warning");
        return $"Cycle: {trace.Cycle}; logic events: {logic}; warnings: {warnings}; errors: {errors}.";
    }
}