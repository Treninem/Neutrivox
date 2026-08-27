using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LogicEditorService
{
    public IReadOnlyList<LogicBlockDefinition> Toolbox { get; } =
    [
        new() { Kind = LogicInstructionKind.Copy, Category = "Data", DisplayName = "Copy", Description = "Copies a value to the target.", InputCount = 1 },
        new() { Kind = LogicInstructionKind.Not, Category = "Boolean", DisplayName = "NOT", Description = "Inverts a boolean value.", InputCount = 1 },
        new() { Kind = LogicInstructionKind.And, Category = "Boolean", DisplayName = "AND", Description = "True when both inputs are true.", InputCount = 2 },
        new() { Kind = LogicInstructionKind.Or, Category = "Boolean", DisplayName = "OR", Description = "True when either input is true.", InputCount = 2 },
        new() { Kind = LogicInstructionKind.Xor, Category = "Boolean", DisplayName = "XOR", Description = "True when inputs differ.", InputCount = 2 },
        new() { Kind = LogicInstructionKind.Set, Category = "Boolean", DisplayName = "SET", Description = "Sets the target to true.", InputCount = 0 },
        new() { Kind = LogicInstructionKind.Reset, Category = "Boolean", DisplayName = "RESET", Description = "Sets the target to false.", InputCount = 0 },
        new() { Kind = LogicInstructionKind.CompareEqual, Category = "Compare", DisplayName = "Equal", Description = "Compares two values for equality.", InputCount = 2 },
        new() { Kind = LogicInstructionKind.CompareGreater, Category = "Compare", DisplayName = "Greater", Description = "True when A is greater than B.", InputCount = 2 },
        new() { Kind = LogicInstructionKind.CompareLess, Category = "Compare", DisplayName = "Less", Description = "True when A is less than B.", InputCount = 2 }
    ];

    public LogicNetwork AddNetwork(LogicProgram program, string? name = null)
    {
        var network = new LogicNetwork { Name = string.IsNullOrWhiteSpace(name) ? $"Network {program.Networks.Count + 1}" : name };
        program.Networks.Add(network);
        return network;
    }

    public LogicInstruction AddInstruction(LogicNetwork network, LogicInstructionKind kind)
    {
        var instruction = new LogicInstruction { Kind = kind, Comment = Describe(kind) };
        network.Instructions.Add(instruction);
        return instruction;
    }

    public bool RemoveInstruction(LogicNetwork network, Guid instructionId)
    {
        var instruction = network.Instructions.FirstOrDefault(x => x.Id == instructionId);
        if (instruction is null) return false;
        network.Instructions.Remove(instruction);
        return true;
    }

    public void MoveInstruction(LogicNetwork network, Guid instructionId, int delta)
    {
        var index = network.Instructions.FindIndex(x => x.Id == instructionId);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= network.Instructions.Count) return;
        var item = network.Instructions[index];
        network.Instructions.RemoveAt(index);
        network.Instructions.Insert(target, item);
    }

    private static string Describe(LogicInstructionKind kind) => kind switch
    {
        LogicInstructionKind.Copy => "Copy source value to target",
        LogicInstructionKind.Not => "Invert source value",
        LogicInstructionKind.And => "Logical AND",
        LogicInstructionKind.Or => "Logical OR",
        LogicInstructionKind.Xor => "Logical XOR",
        LogicInstructionKind.Set => "Set target",
        LogicInstructionKind.Reset => "Reset target",
        LogicInstructionKind.CompareEqual => "Compare equality",
        LogicInstructionKind.CompareGreater => "Compare greater than",
        LogicInstructionKind.CompareLess => "Compare less than",
        _ => "Logic instruction"
    };
}
