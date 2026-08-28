using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Stores named simulation scenarios in memory for the current project session.</summary>
public sealed class SimulationScenarioCatalog
{
    private readonly List<SimulationScenario> _scenarios = [];
    public IReadOnlyList<SimulationScenario> Scenarios => _scenarios;

    public bool Add(SimulationScenario scenario)
    {
        if (string.IsNullOrWhiteSpace(scenario.Name) || _scenarios.Any(x => x.Name.Equals(scenario.Name, StringComparison.OrdinalIgnoreCase))) return false;
        _scenarios.Add(scenario);
        return true;
    }

    public bool Remove(string name)
    {
        var item = _scenarios.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (item is null) return false;
        _scenarios.Remove(item);
        return true;
    }
}
