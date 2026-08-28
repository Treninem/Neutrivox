using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectChange(DateTime TimestampUtc, string Area, string Description);

/// <summary>Tracks user-visible changes during a session for diagnostics and future undo/audit integration.</summary>
public sealed class ProjectChangeTracker
{
    private readonly List<ProjectChange> _changes = [];
    public IReadOnlyList<ProjectChange> Changes => _changes;
    public void Record(string area, string description) => _changes.Add(new(DateTime.UtcNow, area, description));
    public IReadOnlyList<ProjectChange> Recent(int count = 100) => _changes.TakeLast(count).ToList();
    public void Clear() => _changes.Clear();
}
