using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LogicProjectService
{
    public LogicVariable AddVariable(LogicProgram program, string name, TagDataType type)
    {
        var baseName = string.IsNullOrWhiteSpace(name) ? "Variable" : name.Trim();
        var candidate = baseName;
        var number = 1;
        while (program.Variables.Any(x => x.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
            candidate = $"{baseName}{++number}";
        var variable = new LogicVariable { Name = candidate, DataType = type, InitialValue = DefaultValue(type) };
        program.Variables.Add(variable);
        return variable;
    }

    public bool RemoveVariable(LogicProgram program, Guid id)
    {
        var variable = program.Variables.FirstOrDefault(x => x.Id == id);
        if (variable is null) return false;
        program.Variables.Remove(variable);
        return true;
    }

    public void RenameNetwork(LogicNetwork network, string? name)
    {
        if (!string.IsNullOrWhiteSpace(name)) network.Name = name.Trim();
    }

    private static object DefaultValue(TagDataType type) => type switch
    {
        TagDataType.Boolean => false,
        TagDataType.Integer => 0,
        TagDataType.Number => 0d,
        _ => string.Empty
    };
}
