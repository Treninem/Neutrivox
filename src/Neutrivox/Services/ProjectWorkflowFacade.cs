using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectWorkflowSnapshot(
    ProjectSummary Summary,
    ProjectReadinessResult Readiness,
    ProjectDiagnosticsReport Diagnostics,
    LogicEditorViewModel Logic,
    string TextReport);

/// <summary>Single facade for the common project workflow; UI can consume one coherent snapshot.</summary>
public sealed class ProjectWorkflowFacade
{
    private readonly ProjectSummaryService _summary = new();
    private readonly ProjectReadinessService _readiness = new();
    private readonly ProjectDiagnosticsService _diagnostics = new();
    private readonly LogicEditorPresenterService _logic = new();
    private readonly ProjectReportService _report = new();

    public ProjectWorkflowSnapshot BuildSnapshot(AutomationProject project) => new(
        _summary.Create(project),
        _readiness.Assess(project),
        _diagnostics.Diagnose(project),
        _logic.Build(project),
        _report.CreateTextReport(project));
}
