using Neutrivox.Models;
using Neutrivox.Services;

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

var project = new AutomationProject { Name = "Smoke Project" };
var device = new ProjectDevice { DefinitionId = "smoke.controller", Name = "Controller" };
device.Channels.Add(new IoChannel { Name = "Start", Type = "Digital", Direction = "Input" });
device.Channels.Add(new IoChannel { Name = "Lamp", Type = "Digital", Direction = "Output" });
project.Devices.Add(device);

var network = new LogicNetwork { Name = "Start lamp" };
network.Instructions.Add(new LogicInstruction
{
    Kind = LogicInstructionKind.Copy,
    SourceA = "Start",
    Target = "Lamp"
});
project.Logic.Networks.Add(network);

var simulation = new SimulationSessionService();
var session = simulation.Create(project);
simulation.Start(session);
Assert(simulation.SetChannelValue(session, project, device.Channels[0].Id, true), "Input assignment failed.");
var run = new SimulationWorkflowService().RunCycle(project, session);
Assert(run.Success, "Simulation workflow failed: " + string.Join("; ", run.Errors));
Assert(session.ChannelValues[device.Channels[1].Id] is true, "Output did not follow input.");

var persistence = new ProjectPersistenceService();
var serialized = persistence.Serialize(project);
var loaded = persistence.Deserialize(serialized);
Assert(loaded.Success && loaded.Project is not null, "Project round-trip failed.");
Assert(loaded.Project!.Devices.Count == 1, "Project device count changed after round-trip.");

var endpoint = new EndpointValidationService().ValidateNetworkEndpoint("192.168.1.10:502");
Assert(endpoint.Valid, "Valid network endpoint was rejected.");

var registry = VerifiedOwenCatalogBootstrap.CreateRegistry();
Assert(registry.TryGet("owen.pr100.24_0804_03_1", out _), "Verified PR100 profile was not registered.");
Assert(registry.TryGet("owen.pm210", out _), "Verified PM210 profile was not registered.");
Assert(registry.TryGet("owen.pe210-230", out _), "Verified PE210 profile was not registered.");
Assert(registry.TryGet("owen.pv210-24", out _), "Verified PV210 profile was not registered.");

Console.WriteLine("Neutrivox smoke checks passed.");
