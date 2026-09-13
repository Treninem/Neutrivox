namespace Neutrivox.Models;

public sealed class LogicProgram
{
    public string Name { get; set; } = "Main";
    public LogicEditorMode EditorMode { get; set; } = LogicEditorMode.Fbd;
    public string StructuredTextSource { get; set; } = string.Empty;
    public List<LogicNetwork> Networks { get; } = [];
    public List<LogicVariable> Variables { get; } = [];
}

public enum LogicEditorMode
{
    Fbd,
    Ladder,
    StructuredText
}

public sealed class LogicNetwork
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Network";
    public bool Enabled { get; set; } = true;
    public List<LogicInstruction> Instructions { get; } = [];
}

public sealed class LogicVariable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TagDataType DataType { get; set; } = TagDataType.Boolean;
    public object? InitialValue { get; set; }
}

public sealed class LogicInstruction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public LogicInstructionKind Kind { get; set; }
    public string? Target { get; set; }
    public string? SourceA { get; set; }
    public string? SourceB { get; set; }
    public double? Constant { get; set; }
    public string? Comment { get; set; }
}

public enum LogicInstructionKind
{
    Copy,
    Not,
    And,
    Or,
    Xor,
    Set,
    Reset,
    CompareEqual,
    CompareGreater,
    CompareLess
}
