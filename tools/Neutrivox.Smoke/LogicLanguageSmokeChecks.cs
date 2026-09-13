using System.Runtime.CompilerServices;
using Neutrivox.Models;
using Neutrivox.Services;

internal static class LogicLanguageSmokeChecks
{
    [ModuleInitializer]
    internal static void Run()
    {
        var project = new AutomationProject { Name = "Logic language smoke" };
        var device = new ProjectDevice { DefinitionId = "smoke", Name = "Controller" };
        var start = new IoChannel { Name = "Start", Type = "Digital", Direction = "Input" };
        var safe = new IoChannel { Name = "Safe", Type = "Digital", Direction = "Input" };
        var motor = new IoChannel { Name = "Motor", Type = "Digital", Direction = "Output" };
        device.Channels.Add(start); device.Channels.Add(safe); device.Channels.Add(motor);
        project.Devices.Add(device);

        var compiler = new StructuredTextCompilerService();
        var compile = compiler.Compile(project, "Motor := Start AND Safe;");
        Require(compile.Success && project.Logic.Networks.Single().Instructions.Single().Kind == LogicInstructionKind.And,
            "Structured Text did not compile to the common logic IR.");

        var simulation = new SimulationSessionService();
        var session = simulation.Create(project);
        simulation.Start(session);
        simulation.SetChannelValue(session, project, start.Id, true);
        simulation.SetChannelValue(session, project, safe.Id, true);
        var run = new SimulationWorkflowService().RunCycle(project, session);
        Require(run.Success && session.ChannelValues[motor.Id] is true,
            "ST-compiled logic did not execute through the normal simulation runtime.");

        project.Logic.EditorMode = LogicEditorMode.Fbd;
        Require(compiler.Render(project).Contains("Motor := Start AND Safe;", StringComparison.Ordinal),
            "Common IR could not be rendered back to Structured Text from FBD/LD data.");
        project.Logic.EditorMode = LogicEditorMode.Ladder;
        Require(project.Logic.Networks.Count == 1,
            "Switching FBD/LD representation must not duplicate project logic.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
