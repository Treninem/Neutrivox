using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ProjectWorkflowIssue(string Area, LogicValidationSeverity Severity, string Message);
public sealed record ProjectWorkflowValidationResult(bool IsReadyForSimulation, IReadOnlyList<ProjectWorkflowIssue> Issues);

/// <summary>
/// Produces one validation result for the user workflow instead of requiring
/// separate screens to invent their own readiness rules.
/// </summary>
public sealed class ProjectValidationWorkflowService
{
    private readonly LogicValidationService _logicValidation = new();

    public ProjectWorkflowValidationResult ValidateForSimulation(AutomationProject project)
    {
        var issues = new List<ProjectWorkflowIssue>();
        if (project.Devices.Count == 0)
            issues.Add(new("Project", LogicValidationSeverity.Warning, "No equipment has been added to the project."));

        foreach (var message in _logicValidation.Validate(project))
            issues.Add(new("Logic", message.Severity, message.Message));

        var ready = !issues.Any(x => x.Severity == LogicValidationSeverity.Error);
        return new ProjectWorkflowValidationResult(ready, issues);
    }
}
