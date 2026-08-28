using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationBatchResult(int Total, int Passed, int Failed, IReadOnlyList<SimulationScenarioResult> Results)
{
    public bool Success => Failed == 0;
}

/// <summary>Runs multiple reproducible simulation scenarios against the same project safely and independently.</summary>
public sealed class SimulationBatchService
{
    private readonly SimulationScenarioService _scenarios = new();

    public SimulationBatchResult Run(AutomationProject project, SimulationSession session, IEnumerable<SimulationScenario> scenarios)
    {
        var results = new List<SimulationScenarioResult>();
        foreach (var scenario in scenarios)
            results.Add(_scenarios.Run(project, session, scenario));
        return new(results.Count, results.Count(x => x.Success), results.Count(x => !x.Success), results);
    }
}
