using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectConsistencyIssue(string Code, string Message);

/// <summary>Checks cross-area consistency without silently modifying user data.</summary>
public sealed class ProjectConsistencyService
{
    public IReadOnlyList<ProjectConsistencyIssue> Check(AutomationProject project)
    {
        var issues = new List<ProjectConsistencyIssue>();
        var deviceIds = project.Devices.Select(x => x.Id).ToHashSet();
        foreach (var connection in project.Connections)
        {
            if (!deviceIds.Contains(connection.FromDeviceId) || !deviceIds.Contains(connection.ToDeviceId))
                issues.Add(new("ORPHAN_CONNECTION", "A connection references a device that is no longer in the project."));
            if (connection.FromDeviceId == connection.ToDeviceId)
                issues.Add(new("SELF_CONNECTION", "A device cannot be connected to itself."));
        }
        var duplicateTags = project.Tags.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Count() > 1);
        foreach (var duplicate in duplicateTags)
            issues.Add(new("DUPLICATE_TAG", $"Duplicate tag name: {duplicate.Key}"));
        return issues;
    }
}
