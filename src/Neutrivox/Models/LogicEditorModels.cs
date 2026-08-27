namespace Neutrivox.Models;

public sealed class LogicBlockDefinition
{
    public LogicInstructionKind Kind { get; init; }
    public string Category { get; init; } = "General";
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int InputCount { get; init; }
    public bool RequiresTarget { get; init; } = true;
}

public sealed record LogicValidationMessage(
    Guid? NetworkId,
    Guid? InstructionId,
    LogicValidationSeverity Severity,
    string Message);

public enum LogicValidationSeverity { Information, Warning, Error }

public sealed class LogicEditorSelection
{
    public Guid? NetworkId { get; private set; }
    public Guid? InstructionId { get; private set; }
    public void SelectNetwork(Guid id) { NetworkId = id; InstructionId = null; }
    public void SelectInstruction(Guid networkId, Guid instructionId) { NetworkId = networkId; InstructionId = instructionId; }
    public void Clear() { NetworkId = null; InstructionId = null; }
}
