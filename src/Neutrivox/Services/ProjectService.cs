using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task SaveAsync(AutomationProject project, string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, project, JsonOptions, cancellationToken);
    }

    public async Task<AutomationProject?> OpenAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<AutomationProject>(stream, JsonOptions, cancellationToken);
    }
}
