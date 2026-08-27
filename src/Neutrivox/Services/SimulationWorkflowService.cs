using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationWorkflowResult(
    bool Success,
    int ExecutedInstructions,
    IReadOnlyList<string> Errors,
    string Summary,
    SimulationTrace Trace);

/// <summary>
/// Runs one complete safe simulation cycle: validation, execution and diagnostics.
/// This service never communicates with physical equipment.
/// </summary>
public sealed class SimulationWorkflowService
{
    private readonly ProjectValidationWorkflowService _validation = new();
    private readonly LogicExecutionService _execution = new();
    private readonly SimulationTraceService _traceService = new();

    public SimulationWorkflowResult RunCycle(AutomationProject project, SimulationSession session, SimulationTrace? trace = null)
    {
        trace ??= new SimulationTrace();
        var cycle = trace.Cycle + 1;
        _traceService.RecordCycleStart(trace, cycle);

        var validation = _validation.Validate(project);
        foreach (var item in validation.Items)
        {
            if (item.Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                _traceService.RecordError(trace, $"Validation: {item.Title} — {item.Description}");
            else
                trace.Add("Validation", $"{item.Title}: {item.Description}");
        }

        if (validation.Items.Any(x => x.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)))
        {
            var errors = validation.Items.Where(x => x.Level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                .Select(x => $"{x.Title}: {x.Description}").ToList();
            return new(false, 0, errors, _traceService.CreateSummary(trace), trace);
        }

        var execution = _execution.Execute(project, session);
        foreach (var error in execution.Errors) _traceService.RecordError(trace, error);
        trace.Add("Simulation", execution.Success
            ? $"Cycle completed. Executed {execution.ExecutedInstructions} instructions."
            : $"Cycle completed with {execution.Errors.Count} errors.");

        return new(execution.Success, execution.ExecutedInstructions, execution.Errors,
            _traceService.CreateSummary(trace), trace);
    }
}
