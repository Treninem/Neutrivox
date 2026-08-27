using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectFileService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task SaveAsync(AutomationProject project, string path, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, project, Options, cancellationToken);
    }

    public async Task<AutomationProject> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<AutomationProject>(stream, Options, cancellationToken)
            ?? throw new InvalidDataException("Project file is empty or invalid.");
    }

    public bool IsProjectFile(string path) => string.Equals(Path.GetExtension(path), ".neutrivox", StringComparison.OrdinalIgnoreCase);
}
