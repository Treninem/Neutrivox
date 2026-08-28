using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationScenario(string Name, string Description, IReadOnlyDictionary<Guid, object?> ChannelInputs, IReadOnlyDictionary<Guid, object?> TagInputs);

public sealed record SimulationScenarioResult(string ScenarioName, bool Success, int ExecutedInstructions, IReadOnlyList<string> Errors);

/// <summary>
/// Applies reproducible virtual inputs and executes scenarios without communicating with physical equipment.
/// </summary>
public sealed class SimulationScenarioService
{
    private readonly SimulationWorkflowService _workflow = new();
    private readonly ProjectSnapshotService _snapshots = new();

    public SimulationScenarioResult Run(AutomationProject project, SimulationSession session, SimulationScenario scenario)
    {
        var snapshot = _snapshots.Capture(project, session);
        try
        {
            foreach (var input in scenario.ChannelInputs) session.ChannelValues[input.Key] = input.Value;
            foreach (var input in scenario.TagInputs) session.TagValues[input.Key] = input.Value;
            var result = _workflow.RunCycle(project, session);
            return new(scenario.Name, result.Success, result.ExecutedInstructions, result.Errors);
        }
        finally
        {
            _snapshots.Restore(session, snapshot);
        }
    }
}
