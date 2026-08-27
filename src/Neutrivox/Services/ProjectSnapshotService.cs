using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectSnapshot(
    DateTime CreatedAtUtc,
    string ProjectName,
    IReadOnlyDictionary<Guid, object?> ChannelValues,
    IReadOnlyDictionary<Guid, object?> TagValues);

/// <summary>
/// Lightweight in-memory snapshot for simulation experiments. It does not modify project configuration.
/// </summary>
public sealed class ProjectSnapshotService
{
    public ProjectSnapshot Capture(AutomationProject project, SimulationSession session) => new(
        DateTime.UtcNow,
        project.Name,
        new Dictionary<Guid, object?>(session.ChannelValues),
        new Dictionary<Guid, object?>(session.TagValues));

    public void Restore(SimulationSession session, ProjectSnapshot snapshot)
    {
        session.ChannelValues.Clear();
        session.TagValues.Clear();
        foreach (var value in snapshot.ChannelValues) session.ChannelValues[value.Key] = value.Value;
        foreach (var value in snapshot.TagValues) session.TagValues[value.Key] = value.Value;
    }
}
