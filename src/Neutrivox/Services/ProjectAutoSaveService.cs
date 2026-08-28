using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Creates JSON snapshots for recovery. The caller controls storage and timing.</summary>
public sealed class ProjectAutoSaveService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string Serialize(AutomationProject project) => JsonSerializer.Serialize(project, Options);

    public AutomationProject? Deserialize(string json)
    {
        try { return JsonSerializer.Deserialize<AutomationProject>(json, Options); }
        catch (JsonException) { return null; }
    }
}
