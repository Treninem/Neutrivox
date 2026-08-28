using Neutrivox.Models;

namespace Neutrivox.Services;

public enum ProjectHealthLevel { Healthy, Attention, Blocked }

public sealed record ProjectHealthItem(ProjectHealthLevel Level, string Area, string Message, string SuggestedAction);
public sealed record ProjectHealthReport(ProjectHealthLevel Level, IReadOnlyList<ProjectHealthItem> Items)
{
    public bool CanSimulate => Level != ProjectHealthLevel.Blocked;
}

/// <summary>
/// Produces one consistent, user-facing health assessment without changing the project.
/// It combines readiness and existing diagnostics so UI pages do not invent separate rules.
/// </summary>
public sealed class ProjectHealthService
{
    private readonly ProjectReadinessService _readiness = new();
    private readonly ProjectDiagnosticsService _diagnostics = new();

    public ProjectHealthReport Assess(AutomationProject project)
    {
        var items = new List<ProjectHealthItem>();
        foreach (var check in _readiness.Evaluate(project).Checks)
        {
            var level = check.Informational ? ProjectHealthLevel.Healthy : check.Passed ? ProjectHealthLevel.Healthy : ProjectHealthLevel.Attention;
            items.Add(new(level, "Readiness", check.Message, check.Passed ? "No action required." : "Complete the missing project configuration."));
        }
        foreach (var diagnostic in _diagnostics.Analyze(project))
        {
            var level = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => ProjectHealthLevel.Blocked,
                DiagnosticSeverity.Warning => ProjectHealthLevel.Attention,
                _ => ProjectHealthLevel.Healthy
            };
            var action = level == ProjectHealthLevel.Blocked ? "Correct the project reference before simulation." :
                level == ProjectHealthLevel.Attention ? "Review the project configuration." : "No action required.";
            items.Add(new(level, diagnostic.Code, diagnostic.Message, action));
        }
        if (items.Count == 0)
            items.Add(new(ProjectHealthLevel.Healthy, "Project", "Project has no detected issues.", "No action required."));
        var overall = items.Any(x => x.Level == ProjectHealthLevel.Blocked) ? ProjectHealthLevel.Blocked :
            items.Any(x => x.Level == ProjectHealthLevel.Attention) ? ProjectHealthLevel.Attention : ProjectHealthLevel.Healthy;
        return new(overall, items);
    }
}
