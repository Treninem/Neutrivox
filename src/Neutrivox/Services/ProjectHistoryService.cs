namespace Neutrivox.Services;

/// <summary>In-memory project activity history designed to be persisted with project metadata later.</summary>
public sealed class ProjectHistoryService
{
    private readonly List<ProjectHistoryEntry> _entries = [];

    public IReadOnlyList<ProjectHistoryEntry> Entries => _entries;

    public void Record(string category, string message, Guid? deviceId = null)
    {
        _entries.Add(new ProjectHistoryEntry(DateTime.UtcNow, category, message, deviceId));
    }

    public IReadOnlyList<ProjectHistoryEntry> GetRecent(int count = 100) => _entries.TakeLast(Math.Max(1, count)).ToList();

    public void Clear() => _entries.Clear();
}

public sealed record ProjectHistoryEntry(DateTime TimestampUtc, string Category, string Message, Guid? DeviceId);