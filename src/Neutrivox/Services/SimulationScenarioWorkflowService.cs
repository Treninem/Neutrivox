using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ScenarioRunReport(
    string ScenarioName,
    bool Success,
    int ExecutedInstructions,
    IReadOnlyList<string> Errors,
    IReadOnlyList<SimulationTraceEntry> Trace);

/// <summary>Runs repeatable scenarios through the same validation and simulation pipeline as manual runs.</summary>
public sealed class SimulationScenarioWorkflowService
{
    private readonly SimulationScenarioService _scenarios = new();
    private readonly SimulationScenarioCatalog _catalog;
    private readonly SimulationSessionService _sessions = new();
    private readonly SimulationTraceService _trace = new();

    public SimulationScenarioWorkflowService(SimulationScenarioCatalog catalog) => _catalog = catalog;

    public ScenarioRunReport RunNamed(AutomationProject project, string scenarioName)
    {
        var scenario = _catalog.Scenarios.FirstOrDefault(x => x.Name.Equals(scenarioName, StringComparison.OrdinalIgnoreCase));
        if (scenario is null)
            return new(scenarioName, false, 0, ["Simulation scenario was not found."], []);

        var session = _sessions.Create(project);
        _sessions.Start(session);
        var trace = _trace.Create();
        var result = _scenarios.Run(project, session, scenario);
        trace.Add("Scenario", $"Scenario '{scenario.Name}' finished: {(result.Success ? "success" : "failed")}.");
        return new(result.ScenarioName, result.Success, result.ExecutedInstructions, result.Errors, trace.Entries);
    }
}
