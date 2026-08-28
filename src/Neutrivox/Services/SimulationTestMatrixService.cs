using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record SimulationTestCase(
    string Name,
    string Description,
    IReadOnlyDictionary<Guid, object?> ChannelInputs,
    IReadOnlyDictionary<Guid, object?> ExpectedOutputs);

public sealed record SimulationTestCaseResult(
    string Name,
    bool Passed,
    IReadOnlyList<string> Failures,
    int ExecutedInstructions);

public sealed record SimulationTestMatrixResult(
    int Total,
    int Passed,
    int Failed,
    IReadOnlyList<SimulationTestCaseResult> Results);

/// <summary>Executes a deterministic set of virtual tests without touching physical devices.</summary>
public sealed class SimulationTestMatrixService
{
    private readonly SimulationScenarioService _scenarioRunner = new();

    public SimulationTestMatrixResult Run(AutomationProject project, IEnumerable<SimulationTestCase> tests)
    {
        var results = new List<SimulationTestCaseResult>();
        foreach (var test in tests)
        {
            var scenario = new SimulationScenario(
                test.Name,
                test.Description,
                test.ChannelInputs,
                new Dictionary<Guid, object?>());
            var session = new SimulationSessionService().Create(project);
            var result = _scenarioRunner.Run(project, session, scenario);
            var failures = new List<string>(result.Errors);

            foreach (var expected in test.ExpectedOutputs)
            {
                if (!session.ChannelValues.TryGetValue(expected.Key, out var actual) || !EqualsNormalized(actual, expected.Value))
                    failures.Add($"Expected channel {expected.Key}={expected.Value}, actual={actual ?? "<null>"}.");
            }
            results.Add(new(test.Name, failures.Count == 0, failures, result.ExecutedInstructions));
        }
        return new(results.Count, results.Count(x => x.Passed), results.Count(x => !x.Passed), results);
    }

    private static bool EqualsNormalized(object? left, object? right)
    {
        if (left is bool lb && right is bool rb) return lb == rb;
        if (left is IConvertible && right is IConvertible)
        {
            try { return Math.Abs(Convert.ToDouble(left) - Convert.ToDouble(right)) < 1e-9; }
            catch { }
        }
        return Equals(left, right);
    }
}
