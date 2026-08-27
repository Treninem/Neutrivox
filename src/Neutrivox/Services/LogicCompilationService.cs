using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LogicCompilationResult(bool Success, IReadOnlyList<string> Errors, int NetworkCount, int InstructionCount);

public sealed class LogicCompilationService
{
    private readonly LogicValidationService _validation = new();

    public LogicCompilationResult Compile(AutomationProject project)
    {
        var messages = _validation.Validate(project);
        var errors = messages.Where(x => x.Severity == LogicValidationSeverity.Error).Select(x => x.Message).ToList();
        var enabledNetworks = project.Logic.Networks.Where(x => x.Enabled).ToList();
        var instructions = enabledNetworks.Sum(x => x.Instructions.Count);
        if (enabledNetworks.Count == 0) errors.Add("No enabled logic networks to compile.");
        return new(errors.Count == 0, errors, enabledNetworks.Count, instructions);
    }
}
