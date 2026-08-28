using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationUiState(
    bool CanRun,
    bool IsRunning,
    int Cycle,
    IReadOnlyList<SimulationValue> Values,
    IReadOnlyList<SimulationTraceEntry> Events,
    IReadOnlyList<ProjectWorkflowIssue> ValidationIssues,
    string Summary);

/// <summary>Single UI-facing facade for the simulation workflow.</summary>
public sealed class SimulationUiWorkflowService
{
    private readonly ProjectValidationWorkflowService _validation = new();
    private readonly SimulationValueService _values = new();
    private readonly SimulationTrace _trace = new();
    private readonly SimulationWorkflowService _workflow = new();

    public SimulationUiState Build(AutomationProject project, SimulationSession session)
    {
        var validation = _validation.Validate(project);
        var values = _values.GetValues(project, session);
        return new(
            validation.IsReadyForSimulation,
            session.State == SimulationRunState.Running,
            _trace.Cycle,
            values,
            _trace.Entries,
            validation.Issues,
            validation.IsReadyForSimulation ? "Project is ready for simulation." : "Fix blocking validation errors before simulation.");
    }

    public SimulationWorkflowResult RunCycle(AutomationProject project, SimulationSession session)
    {
        var result = _workflow.RunCycle(project, session, _trace);
        return result;
    }

    public void ClearTrace() => _trace.Clear();
}
