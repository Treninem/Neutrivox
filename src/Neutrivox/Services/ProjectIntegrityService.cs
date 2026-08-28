using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectIntegrityIssue(string Severity, string Message);
public sealed record ProjectIntegrityResult(bool IsValid, IReadOnlyList<ProjectIntegrityIssue> Issues);

/// <summary>Performs lightweight structural checks after loading or before saving a project.</summary>
public sealed class ProjectIntegrityService
{
    public ProjectIntegrityResult Check(AutomationProject project)
    {
        var issues = new List<ProjectIntegrityIssue>();
        if (project.Id == Guid.Empty) issues.Add(new("Error", "Project identifier is missing."));
        if (project.Devices.Select(x => x.Id).Distinct().Count() != project.Devices.Count)
            issues.Add(new("Error", "Project contains duplicate device identifiers."));
        if (project.Devices.Any(x => string.IsNullOrWhiteSpace(x.DefinitionId)))
            issues.Add(new("Warning", "Some devices do not have a device definition assigned."));
        var deviceIds = project.Devices.Select(x => x.Id).ToHashSet();
        foreach (var connection in project.Connections)
            if (!deviceIds.Contains(connection.FromDeviceId) || !deviceIds.Contains(connection.ToDeviceId))
                issues.Add(new("Error", "A connection references a device that is not in the project."));
        return new(!issues.Any(x => x.Severity == "Error"), issues);
    }
}
