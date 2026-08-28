using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record RecoveryPoint(DateTime CreatedAtUtc, string Reason, string ProjectData);

/// <summary>Maintains bounded in-memory recovery points. Persistent storage is intentionally delegated to the host application.</summary>
public sealed class ProjectRecoveryService
{
    private readonly ProjectPersistenceService _persistence = new();
    private readonly LinkedList<RecoveryPoint> _points = [];
    public int Capacity { get; }
    public IReadOnlyList<RecoveryPoint> Points => _points.ToList();

    public ProjectRecoveryService(int capacity = 10) => Capacity = Math.Max(1, capacity);

    public RecoveryPoint Capture(AutomationProject project, string reason)
    {
        var point = new RecoveryPoint(DateTime.UtcNow, reason, _persistence.Serialize(project));
        _points.AddLast(point);
        while (_points.Count > Capacity) _points.RemoveFirst();
        return point;
    }

    public ProjectLoadResult RestoreLatest()
    {
        var point = _points.Last?.Value;
        return point is null
            ? new(false, null, "No recovery point is available.")
            : _persistence.Deserialize(point.ProjectData);
    }
}
