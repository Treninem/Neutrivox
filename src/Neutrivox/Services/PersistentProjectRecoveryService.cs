using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record PersistentRecoveryEntry(
    DateTimeOffset SavedAtUtc,
    string? SourcePath,
    AutomationProject Project);

public sealed record PersistentRecoveryLoadResult(
    bool Success,
    PersistentRecoveryEntry? Entry,
    string Message);

/// <summary>Keeps the latest crash-recovery snapshot on disk between application sessions.</summary>
public sealed class PersistentProjectRecoveryService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _recoveryPath;

    public PersistentProjectRecoveryService(string? recoveryPath = null)
    {
        _recoveryPath = recoveryPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Neutrivox", "Recovery", "latest-recovery.json");
    }

    public bool HasRecovery => File.Exists(_recoveryPath);

    public async Task SaveAsync(AutomationProject project, string? sourcePath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_recoveryPath)!;
        Directory.CreateDirectory(directory);
        var temporary = _recoveryPath + ".tmp";
        var entry = new PersistentRecoveryEntry(DateTimeOffset.UtcNow, sourcePath, project);
        await using (var stream = File.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, entry, Options, cancellationToken);
        File.Move(temporary, _recoveryPath, true);
    }

    public async Task<PersistentRecoveryLoadResult> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_recoveryPath))
            return new(false, null, "No persistent recovery snapshot is available.");
        try
        {
            await using var stream = File.OpenRead(_recoveryPath);
            var entry = await JsonSerializer.DeserializeAsync<PersistentRecoveryEntry>(stream, Options, cancellationToken);
            return entry?.Project is null
                ? new(false, null, "Recovery snapshot is empty or invalid.")
                : new(true, entry, "Recovery snapshot loaded.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new(false, null, "Recovery snapshot could not be loaded: " + ex.Message);
        }
    }

    public void Clear()
    {
        try { if (File.Exists(_recoveryPath)) File.Delete(_recoveryPath); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
