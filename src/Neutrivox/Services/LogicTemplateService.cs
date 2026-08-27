using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LogicTemplateService
{
    public LogicNetwork CreateBasicStartStop(AutomationProject project, string input, string output)
    {
        var network = new LogicNetwork { Name = "Start / Stop" };
        network.Instructions.Add(new LogicInstruction
        {
            Kind = LogicInstructionKind.Copy,
            Target = output,
            SourceA = input,
            Comment = "Basic input-to-output control"
        });
        project.Logic.Networks.Add(network);
        return network;
    }

    public LogicNetwork CreateBooleanGate(AutomationProject project, LogicInstructionKind kind, string inputA, string inputB, string output)
    {
        var network = new LogicNetwork { Name = $"{kind} network" };
        network.Instructions.Add(new LogicInstruction
        {
            Kind = kind,
            Target = output,
            SourceA = inputA,
            SourceB = inputB,
            Comment = $"Generated {kind} template"
        });
        project.Logic.Networks.Add(network);
        return network;
    }
}
