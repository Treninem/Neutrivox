using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationWorkflowResult(
    bool Success,
    int ExecutedInstructions,
    IReadOnlyList<string> Errors,
    string Summary,
    SimulationTrace Trace);

/// <summary>Runs one complete simulation cycle: validation, execution and trace generation.</summary>
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

        var validation = _validation.ValidateForSimulation(project);
        _traceService.RecordValidation(trace, validation);

        if (!validation.IsReadyForSimulation)
        {
            var errors = validation.Issues
                .Where(x => x.Severity == LogicValidationSeverity.Error)
                .Select(x => $"{x.Area}: {x.Message}")
                .ToList();
            return new(false, 0, errors, _traceService.CreateSummary(trace), trace);
        }

        var execution = _execution.Execute(project, session);
        _traceService.RecordExecution(trace, execution);
        return new(execution.Success, execution.ExecutedInstructions, execution.Errors,
            _traceService.CreateSummary(trace), trace);
    }
}
