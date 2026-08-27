using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectDiagnosticsService
{
    public IReadOnlyList<ProjectDiagnostic> Analyze(AutomationProject project)
    {
        var diagnostics = new List<ProjectDiagnostic>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var channel in project.Devices.SelectMany(x => x.Channels))
            if (!names.Add(channel.Name)) diagnostics.Add(new(DiagnosticSeverity.Warning, "NAME_COLLISION", $"Duplicate channel name: {channel.Name}"));
        foreach (var tag in project.Tags)
            if (!names.Add(tag.Name)) diagnostics.Add(new(DiagnosticSeverity.Warning, "NAME_COLLISION", $"Name conflicts with another project value: {tag.Name}"));

        foreach (var network in project.Logic.Networks)
        foreach (var instruction in network.Instructions)
        {
            foreach (var reference in new[] { instruction.Target, instruction.SourceA, instruction.SourceB }.Where(x => !string.IsNullOrWhiteSpace(x)))
                if (!names.Contains(reference!)) diagnostics.Add(new(DiagnosticSeverity.Error, "UNKNOWN_REFERENCE", $"{network.Name} references unknown value '{reference}'."));
        }

        if (project.Logic.Networks.Count == 0)
            diagnostics.Add(new(DiagnosticSeverity.Information, "NO_LOGIC", "The project does not contain logic networks yet."));
        return diagnostics;
    }
}

public sealed record ProjectDiagnostic(DiagnosticSeverity Severity, string Code, string Message);
public enum DiagnosticSeverity { Information, Warning, Error }