using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentPlanSnapshot(string Fingerprint, DateTimeOffset CreatedAt, IReadOnlyList<Guid> DeviceIds);
public sealed record DeploymentPlanGuardResult(bool IsCurrent, IReadOnlyList<string> Errors, string MessageRu, string MessageEn);

/// <summary>Prevents a previously prepared deployment plan from being silently reused after project changes.</summary>
public sealed class DeploymentPlanGuardService
{
    private readonly DeploymentPlanFingerprintService _fingerprinter;

    public DeploymentPlanGuardService(DeploymentPlanFingerprintService fingerprinter) => _fingerprinter = fingerprinter;

    public DeploymentPlanSnapshot Capture(AutomationProject project, IEnumerable<Guid> deviceIds) =>
        new(_fingerprinter.Compute(project, deviceIds), DateTimeOffset.UtcNow, deviceIds.Distinct().ToList());

    public DeploymentPlanGuardResult Validate(AutomationProject project, DeploymentPlanSnapshot snapshot)
    {
        var current = _fingerprinter.Compute(project, snapshot.DeviceIds);
        if (StringComparer.Ordinal.Equals(current, snapshot.Fingerprint))
            return new(true, [], "План передачи актуален.", "The deployment plan is current.");

        var errors = new[]
        {
            "Проект, устройство, привязка или логика изменились после подготовки плана.",
            "Подготовьте новый план перед физической передачей."
        };
        return new(false, errors,
            "План передачи устарел. Подготовьте его заново перед записью в прибор.",
            "The deployment plan is stale. Prepare it again before writing to the device.");
    }
}
