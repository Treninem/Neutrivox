using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox.Smoke;

public static class DeploymentPlanTests
{
    public static void Run()
    {
        var project = new AutomationProject { Name = "Plan test" };
        var device = new ProjectDevice { DefinitionId = "OWEN.PR100", Name = "PR100-1" };
        project.Devices.Add(device);

        var fingerprint = new DeploymentPlanFingerprintService();
        var guard = new DeploymentPlanGuardService(fingerprint);
        var snapshot = guard.Capture(project, [device.Id]);

        var initial = guard.Validate(project, snapshot);
        SmokeAssert.True(initial.IsCurrent, "A freshly captured deployment plan must be current.");

        device.Name = "PR100-changed";
        var changed = guard.Validate(project, snapshot);
        SmokeAssert.False(changed.IsCurrent, "Changing the target project device must invalidate the old plan.");
        SmokeAssert.True(changed.Errors.Count > 0, "A stale plan must provide an actionable error.");
    }
}
