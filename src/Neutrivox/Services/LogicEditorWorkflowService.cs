using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Coordinates editing operations so UI code does not duplicate project rules.
/// It intentionally works only with the current digital project and performs no
/// communication with physical equipment.
/// </summary>
public sealed class LogicEditorWorkflowService
{
    private readonly LogicEditorService _editor = new();
    private readonly LogicProjectService _projectOperations = new();
    private readonly LogicSymbolService _symbols = new();
    private readonly LogicValidationService _validation = new();
    private readonly LogicCompilationService _compilation = new();

    public IReadOnlyList<LogicBlockDefinition> Toolbox => _editor.Toolbox;

    public LogicNetwork CreateNetwork(AutomationProject project, string? name = null)
        => _editor.AddNetwork(project.Logic, name);

    public LogicVariable CreateVariable(AutomationProject project, string name, TagDataType type)
        => _projectOperations.AddVariable(project.Logic, name, type);

    public IReadOnlyList<LogicSymbol> GetAvailableSymbols(AutomationProject project)
        => _symbols.GetSymbols(project);

    public LogicInstruction AddInstruction(LogicNetwork network, LogicInstructionKind kind)
        => _editor.AddInstruction(network, kind);

    public void ConfigureInstruction(LogicInstruction instruction, string? target, string? sourceA, string? sourceB, string? comment = null)
    {
        instruction.Target = target;
        instruction.SourceA = sourceA;
        instruction.SourceB = sourceB;
        if (comment is not null) instruction.Comment = comment;
    }

    public IReadOnlyList<LogicValidationMessage> Validate(AutomationProject project)
        => _validation.Validate(project);

    public LogicCompilationResult CheckReadiness(AutomationProject project)
        => _compilation.Compile(project);
}
