using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LogicExecutionService
{
    public LogicExecutionResult Execute(AutomationProject project, SimulationSession session)
    {
        var errors = new List<string>();
        var values = BuildValueIndex(project, session);
        var executed = 0;

        foreach (var network in project.Logic.Networks.Where(x => x.Enabled))
        {
            foreach (var instruction in network.Instructions)
            {
                try { ExecuteInstruction(instruction, values); executed++; }
                catch (Exception ex) { errors.Add($"{network.Name}: {ex.Message}"); }
            }
        }

        ApplyValues(project, session, values);
        return new LogicExecutionResult(errors.Count == 0, executed, errors);
    }

    private static Dictionary<string, object?> BuildValueIndex(AutomationProject project, SimulationSession session)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in project.Tags) values[tag.Name] = session.TagValues.TryGetValue(tag.Id, out var value) ? value : tag.InitialValue;
        foreach (var variable in project.Logic.Variables) values[variable.Name] = variable.InitialValue;
        foreach (var channel in project.Devices.SelectMany(x => x.Channels)) values[channel.Name] = session.ChannelValues.TryGetValue(channel.Id, out var value) ? value : false;
        return values;
    }

    private static void ExecuteInstruction(LogicInstruction i, Dictionary<string, object?> v)
    {
        if (string.IsNullOrWhiteSpace(i.Target)) throw new InvalidOperationException("Instruction target is missing.");
        object? a = Get(v, i.SourceA);
        object? b = Get(v, i.SourceB);
        v[i.Target] = i.Kind switch
        {
            LogicInstructionKind.Copy => a,
            LogicInstructionKind.Not => !AsBool(a),
            LogicInstructionKind.And => AsBool(a) && AsBool(b),
            LogicInstructionKind.Or => AsBool(a) || AsBool(b),
            LogicInstructionKind.Xor => AsBool(a) ^ AsBool(b),
            LogicInstructionKind.Set => true,
            LogicInstructionKind.Reset => false,
            LogicInstructionKind.CompareEqual => Equals(a, b),
            LogicInstructionKind.CompareGreater => AsNumber(a) > AsNumber(b),
            LogicInstructionKind.CompareLess => AsNumber(a) < AsNumber(b),
            _ => throw new InvalidOperationException("Unsupported instruction.")
        };
    }

    private static object? Get(Dictionary<string, object?> v, string? key) => string.IsNullOrWhiteSpace(key) ? null : v.TryGetValue(key, out var value) ? value : throw new InvalidOperationException($"Unknown value '{key}'.");
    private static bool AsBool(object? value) => value switch { bool b => b, int i => i != 0, double d => Math.Abs(d) > double.Epsilon, _ => false };
    private static double AsNumber(object? value) => value switch { int i => i, long l => l, float f => f, double d => d, bool b => b ? 1 : 0, _ => 0 };

    private static void ApplyValues(AutomationProject project, SimulationSession session, Dictionary<string, object?> values)
    {
        foreach (var tag in project.Tags) if (values.TryGetValue(tag.Name, out var value)) session.TagValues[tag.Id] = value;
        foreach (var channel in project.Devices.SelectMany(x => x.Channels)) if (values.TryGetValue(channel.Name, out var value)) session.ChannelValues[channel.Id] = value;
    }
}

public sealed record LogicExecutionResult(bool Success, int ExecutedInstructions, IReadOnlyList<string> Errors);