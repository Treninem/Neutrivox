using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectLoadResult(bool Success, AutomationProject? Project, string? Error);

/// <summary>Handles explicit project serialization and validation of basic file content.</summary>
public sealed class ProjectPersistenceService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string Serialize(AutomationProject project) => JsonSerializer.Serialize(project, Options);

    public ProjectLoadResult Deserialize(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return new(false, null, "Project file is empty.");
        try
        {
            var project = JsonSerializer.Deserialize<AutomationProject>(content, Options);
            if (project is null) return new(false, null, "Project file does not contain a project.");
            return new(true, project, null);
        }
        catch (JsonException ex)
        {
            return new(false, null, $"Project file cannot be read: {ex.Message}");
        }
    }
}
