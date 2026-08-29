using System.Text.Json;
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
var workflow = new SimulationWorkflowService();
var run = workflow.RunCycle(project, session);
Assert(run.Success, "Simulation workflow failed: " + string.Join("; ", run.Errors));
Assert(run.ExecutedInstructions == 1, "Unexpected executed instruction count.");
Assert(session.ChannelValues[device.Channels[1].Id] is true, "Output did not follow input.");
Assert(run.Trace.Entries.Count > 0, "Simulation trace is empty.");

var traceSummary = new SimulationTraceService().CreateSummary(run.Trace);
Assert(traceSummary.Contains("errors: 0", StringComparison.OrdinalIgnoreCase), "Unexpected simulation errors in trace summary.");

var persistence = new ProjectPersistenceService();
var serialized = persistence.Serialize(project);
var loaded = persistence.Deserialize(serialized);
Assert(loaded.Success && loaded.Project is not null, "Project round-trip failed.");
Assert(loaded.Project!.Devices.Count == 1, "Project device count changed after round-trip.");

var integrity = new ProjectIntegrityService().Check(loaded.Project);
Assert(integrity.IsValid, "Project integrity check failed after round-trip.");

var recovery = new ProjectRecoveryService();
recovery.Capture(project, "Smoke test");
project.Name = "Changed after recovery point";
var restored = recovery.RestoreLatest();
Assert(restored.Success && restored.Project?.Name == "Smoke Project", "Recovery point restore failed.");

var scenarioCatalog = new SimulationScenarioCatalog();
Assert(scenarioCatalog.Add(new SimulationScenario("Start ON", "Turn start on", new Dictionary<Guid, object?> { [device.Channels[0].Id] = true }, new Dictionary<Guid, object?>())), "Scenario was not added.");
Assert(scenarioCatalog.Scenarios.Count == 1, "Scenario catalog count mismatch.");

var endpoint = new EndpointValidationService().ValidateNetworkEndpoint("192.168.1.10:502");
Assert(endpoint.Valid, "Valid network endpoint was rejected.");
Assert(new EndpointValidationService().ValidateSerialEndpoint("COM3").Valid, "Valid serial endpoint was rejected.");
Assert(!new EndpointValidationService().ValidateNetworkEndpoint("not-an-ip:502").Valid, "Invalid network endpoint was accepted.");

var registry = VerifiedOwenCatalogBootstrap.CreateRegistry();
Assert(registry.TryGet("owen.pr100.24_0804_03_1", out var pr100), "Verified PR100 profile was not registered.");
Assert(pr100!.Transports.Contains(DeviceTransport.SerialRs485), "PR100 RS-485 capability missing.");
Assert(registry.TryGet("owen.pm210", out var pm210), "Verified PM210 profile was not registered.");
Assert(pm210!.SupportLevel == DeviceSupportLevel.ModelProfiled, "PM210 support level is incorrect.");
Assert(registry.TryGet("owen.pe210-230", out _), "Verified PE210 profile was not registered.");
Assert(registry.TryGet("owen.pv210-24", out _), "Verified PV210 profile was not registered.");

var compatibility = new DeviceCompatibilityService().Check(device, pr100!);
Assert(!compatibility.Compatible, "Unrelated smoke device was incorrectly accepted as PR100.");

var preflight = new DeploymentPreflightService();
var preflightReport = preflight.Check(project, [device.Id]);
Assert(preflightReport.Checks.Any(x => x.Code == "NOT_MAPPED" && x.Severity == PreflightSeverity.Error), "Unmapped deployment target was not blocked.");

var device2 = new ProjectDevice { DefinitionId = "generic-controller-8io", Name = "Second Controller" };
device.PhysicalBinding = new PhysicalDeviceBinding
{
    Endpoint = "COM3",
    Manufacturer = "ОВЕН",
    Model = "ПР100-24.0804.03.1",
    IdentificationState = "Verified"
};
device2.PhysicalBinding = new PhysicalDeviceBinding
{
    Endpoint = "192.168.1.20:502",
    Manufacturer = "Neutrivox Demo",
    Model = "Controller 8 I/O",
    IdentificationState = "Verified"
};
project.Devices.Add(device2);

var planning = new DeploymentPlanningService();
var orderedPlan = planning.CreatePreview(project, [device2.Id, device.Id]).Plan;
Assert(orderedPlan.Targets.Count == 2, "Deployment plan did not preserve two selected targets.");
Assert(orderedPlan.Targets[0].Order == 1 && orderedPlan.Targets[0].ProjectDeviceId == device2.Id, "First deployment target order is wrong.");
Assert(orderedPlan.Targets[1].Order == 2 && orderedPlan.Targets[1].ProjectDeviceId == device.Id, "Second deployment target order is wrong.");

var fingerprintService = new DeploymentPlanFingerprintService();
var forwardFingerprint = fingerprintService.Compute(project, [device2.Id, device.Id]);
var reverseFingerprint = fingerprintService.Compute(project, [device.Id, device2.Id]);
Assert(forwardFingerprint != reverseFingerprint, "Deployment fingerprint ignored target order.");

var guardService = new DeploymentPlanGuardService(fingerprintService);
var snapshot = guardService.Capture(project, [device2.Id, device.Id]);
device2.Name = "Second Controller renamed";
var guardResult = guardService.Validate(project, snapshot);
Assert(!guardResult.IsCurrent, "Stale deployment plan was accepted after project changes.");

var plans = new CommercialPlanCatalogService();
Assert(plans.Find("free")?.PriceRub == 0m, "Free plan price is not zero RUB.");
Assert(plans.Find("owner-lifetime")?.Edition == ProductEdition.Owner, "Owner lifetime plan is missing.");
Assert(plans.GetPublicPlans().All(x => x.Edition != ProductEdition.Owner), "Owner plan must not be publicly sellable.");

var verifier = new SmokeLicenseVerifier();
var licenseService = new LicenseActivationService(plans, verifier);
var licensePayload = new LicenseKeyPayload(
    "smoke-key",
    "professional-30d",
    "smoke-test",
    "machine-A",
    DateTimeOffset.UtcNow,
    DateTimeOffset.UtcNow.AddDays(30),
    "SMOKE");
var licenseJson = JsonSerializer.Serialize(licensePayload);
var activation = licenseService.Activate(new LicenseActivationRequest(licenseJson, "machine-A"), DateTimeOffset.UtcNow);
Assert(activation.Success && activation.Edition == ProductEdition.Professional, "Bound professional license activation failed.");
var wrongMachine = licenseService.Activate(new LicenseActivationRequest(licenseJson, "machine-B"), DateTimeOffset.UtcNow);
Assert(!wrongMachine.Success, "A device-bound license was accepted on another machine.");

Console.WriteLine("Neutrivox smoke checks passed.");

sealed class SmokeLicenseVerifier : ILicenseSignatureVerifier
{
    public bool Verify(LicenseKeyPayload payload) => payload.Signature == "SMOKE";
}
