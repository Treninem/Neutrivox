using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LogicSimulationEngine
{
    public SimulationRunResult ExecuteCycle(AutomationProject project, IDictionary<string, object?> values)
    {
        var result = new SimulationRunResult();
        foreach (var variable in project.Logic.Variables)
            if (!values.ContainsKey(variable.Name)) values[variable.Name] = variable.InitialValue ?? Default(variable.DataType);
        foreach (var tag in project.Tags)
            if (!values.ContainsKey(tag.Name)) values[tag.Name] = tag.InitialValue ?? Default(tag.DataType);

        foreach (var network in project.Logic.Networks.Where(x => x.Enabled))
        {
            foreach (var instruction in network.Instructions)
            {
                try { ExecuteInstruction(instruction, values); }
                catch (Exception ex) { result.Errors.Add($"{network.Name}: {ex.Message}"); }
            }
        }
        result.Values = new Dictionary<string, object?>(values);
        return result;
    }

    private static void ExecuteInstruction(LogicInstruction i, IDictionary<string, object?> values)
    {
        if (string.IsNullOrWhiteSpace(i.Target)) throw new InvalidOperationException("Instruction target is not specified.");
        var a = Resolve(i.SourceA, i.Constant, values);
        var b = Resolve(i.SourceB, null, values);
        values[i.Target] = i.Kind switch
        {
            LogicInstructionKind.Copy => a,
            LogicInstructionKind.Not => !ToBool(a),
            LogicInstructionKind.And => ToBool(a) && ToBool(b),
            LogicInstructionKind.Or => ToBool(a) || ToBool(b),
            LogicInstructionKind.Xor => ToBool(a) ^ ToBool(b),
            LogicInstructionKind.Set => true,
            LogicInstructionKind.Reset => false,
            LogicInstructionKind.CompareEqual => Equals(a, b),
            LogicInstructionKind.CompareGreater => ToNumber(a) > ToNumber(b),
            LogicInstructionKind.CompareLess => ToNumber(a) < ToNumber(b),
            _ => throw new InvalidOperationException("Unsupported instruction.")
        };
    }

    private static object? Resolve(string? name, double? constant, IDictionary<string, object?> values)
        => constant ?? (name is not null && values.TryGetValue(name, out var value) ? value : false);
    private static bool ToBool(object? value) => value switch { bool b => b, int i => i != 0, double d => Math.Abs(d) > double.Epsilon, _ => false };
    private static double ToNumber(object? value) => value switch { int i => i, long l => l, float f => f, double d => d, bool b => b ? 1 : 0, _ => 0 };
    private static object Default(TagDataType type) => type switch { TagDataType.Boolean => false, TagDataType.Integer => 0, TagDataType.Number => 0d, _ => string.Empty };
}

public sealed class SimulationRunResult
{
    public Dictionary<string, object?> Values { get; set; } = [];
    public List<string> Errors { get; } = [];
    public bool Success => Errors.Count == 0;
}