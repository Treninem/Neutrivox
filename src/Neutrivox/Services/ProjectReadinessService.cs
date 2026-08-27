using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectReadinessService
{
    public ProjectReadinessReport Evaluate(AutomationProject project)
    {
        var checks = new List<ReadinessCheck>();
        checks.Add(project.Devices.Count > 0
            ? ReadinessCheck.Pass("equipment", "Equipment has been added")
            : ReadinessCheck.Fail("equipment", "Add at least one device"));

        var channelCount = project.Devices.Sum(x => x.Channels.Count);
        checks.Add(channelCount > 0
            ? ReadinessCheck.Pass("io", $"{channelCount} channels are available")
            : ReadinessCheck.Fail("io", "No I/O channels are available"));

        checks.Add(ReadinessCheck.Info("simulation", "The project can be tested without physical equipment"));
        checks.Add(ReadinessCheck.Info("deployment", "Physical deployment requires a supported and verified device adapter"));
        return new ProjectReadinessReport(checks);
    }
}

public sealed class ProjectReadinessReport(IReadOnlyList<ReadinessCheck> checks)
{
    public IReadOnlyList<ReadinessCheck> Checks { get; } = checks;
    public bool IsReadyForSimulation => Checks.Where(x => x.Required).All(x => x.Passed);
}

public sealed record ReadinessCheck(string Id, string Message, bool Passed, bool Required, bool Informational)
{
    public static ReadinessCheck Pass(string id, string message) => new(id, message, true, true, false);
    public static ReadinessCheck Fail(string id, string message) => new(id, message, false, true, false);
    public static ReadinessCheck Info(string id, string message) => new(id, message, true, false, true);
}