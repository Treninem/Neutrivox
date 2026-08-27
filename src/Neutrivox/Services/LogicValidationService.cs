using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LogicValidationService
{
    public IReadOnlyList<LogicValidationMessage> Validate(AutomationProject project)
    {
        var result = new List<LogicValidationMessage>();
        var symbols = project.Devices.SelectMany(x => x.Channels).Select(x => x.Name)
            .Concat(project.Tags.Select(x => x.Name))
            .Concat(project.Logic.Variables.Select(x => x.Name))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var network in project.Logic.Networks)
        {
            if (string.IsNullOrWhiteSpace(network.Name))
                result.Add(new(network.Id, null, LogicValidationSeverity.Warning, "Network has no name."));
            foreach (var instruction in network.Instructions)
            {
                if (string.IsNullOrWhiteSpace(instruction.Target))
                    result.Add(new(network.Id, instruction.Id, LogicValidationSeverity.Error, "Instruction target is not selected."));
                else if (!symbols.Contains(instruction.Target))
                    result.Add(new(network.Id, instruction.Id, LogicValidationSeverity.Error, $"Unknown target '{instruction.Target}'."));

                ValidateSource(network.Id, instruction.Id, instruction.SourceA, "A", symbols, result);
                if (RequiresSecondSource(instruction.Kind)) ValidateSource(network.Id, instruction.Id, instruction.SourceB, "B", symbols, result);
            }
        }

        if (project.Logic.Networks.Count == 0)
            result.Add(new(null, null, LogicValidationSeverity.Information, "No logic networks have been created."));
        return result;
    }

    private static bool RequiresSecondSource(LogicInstructionKind kind) => kind is LogicInstructionKind.And or LogicInstructionKind.Or or LogicInstructionKind.Xor or LogicInstructionKind.CompareEqual or LogicInstructionKind.CompareGreater or LogicInstructionKind.CompareLess;

    private static void ValidateSource(Guid networkId, Guid instructionId, string? source, string label, HashSet<string> symbols, List<LogicValidationMessage> result)
    {
        if (string.IsNullOrWhiteSpace(source)) return;
        if (!symbols.Contains(source)) result.Add(new(networkId, instructionId, LogicValidationSeverity.Error, $"Unknown source {label} '{source}'."));
    }
}
