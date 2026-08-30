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
/// It combines canonical readiness and diagnostics so UI pages do not invent separate rules.
/// </summary>
public sealed class ProjectHealthService
{
    private readonly ProjectReadinessService _readiness = new();
    private readonly ProjectDiagnosticsService _diagnostics = new();

    public ProjectHealthReport Assess(AutomationProject project)
    {
        var items = new List<ProjectHealthItem>();
        foreach (var check in _readiness.Evaluate(project).Items)
        {
            var level = check.Level switch
            {
                ProjectReadinessLevel.Blocking => ProjectHealthLevel.Blocked,
                ProjectReadinessLevel.Warning => ProjectHealthLevel.Attention,
                _ => ProjectHealthLevel.Healthy
            };
            var action = check.Level == ProjectReadinessLevel.Information
                ? "Действий не требуется."
                : check.SuggestedAction;
            items.Add(new(level, "Readiness", check.Description, action));
        }

        foreach (var diagnostic in _diagnostics.Analyze(project))
        {
            var level = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => ProjectHealthLevel.Blocked,
                DiagnosticSeverity.Warning => ProjectHealthLevel.Attention,
                _ => ProjectHealthLevel.Healthy
            };
            var action = level == ProjectHealthLevel.Blocked
                ? "Исправьте ошибку проекта перед симуляцией."
                : level == ProjectHealthLevel.Attention
                    ? "Проверьте конфигурацию проекта."
                    : "Действий не требуется.";
            items.Add(new(level, diagnostic.Code, diagnostic.Message, action));
        }

        if (items.Count == 0)
            items.Add(new(ProjectHealthLevel.Healthy, "Project", "В проекте не обнаружено проблем.", "Действий не требуется."));

        var overall = items.Any(x => x.Level == ProjectHealthLevel.Blocked)
            ? ProjectHealthLevel.Blocked
            : items.Any(x => x.Level == ProjectHealthLevel.Attention)
                ? ProjectHealthLevel.Attention
                : ProjectHealthLevel.Healthy;

        return new(overall, items);
    }
}
