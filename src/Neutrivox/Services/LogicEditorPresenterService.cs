using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LogicEditorViewModel(
    IReadOnlyList<LogicNetwork> Networks,
    IReadOnlyList<LogicVariable> Variables,
    IReadOnlyList<LogicSymbol> Symbols,
    IReadOnlyList<LogicBlockDefinition> Toolbox,
    IReadOnlyList<LogicValidationMessage> Validation,
    LogicCompilationResult Readiness);

/// <summary>Creates a complete editor view model from one AutomationProject without duplicating project state.</summary>
public sealed class LogicEditorPresenterService
{
    private readonly LogicEditorWorkflowService _workflow = new();

    public LogicEditorViewModel Build(AutomationProject project) => new(
        project.Logic.Networks,
        project.Logic.Variables,
        _workflow.GetAvailableSymbols(project),
        _workflow.Toolbox,
        _workflow.Validate(project),
        _workflow.CheckReadiness(project));
}
