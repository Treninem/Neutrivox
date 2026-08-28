using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentUserPlanItem(
    int Order,
    Guid DeviceId,
    string DeviceName,
    string Endpoint,
    string ProfileId,
    bool Ready,
    string StatusRu,
    string StatusEn);

public sealed record DeploymentUserPlan(
    string Fingerprint,
    IReadOnlyList<DeploymentUserPlanItem> Items,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings,
    string SummaryRu,
    string SummaryEn);

/// <summary>
/// Converts the existing deployment workflow into a stable user-facing plan.
/// It does not perform physical I/O.
/// </summary>
public sealed class DeploymentUserPlanService
{
    private readonly DeploymentWorkflowService _workflow;
    private readonly DeploymentPlanFingerprintService _fingerprinter;

    public DeploymentUserPlanService(
        DeploymentWorkflowService workflow,
        DeploymentPlanFingerprintService fingerprinter)
    {
        _workflow = workflow;
        _fingerprinter = fingerprinter;
    }

    public DeploymentUserPlan Build(AutomationProject project, IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds.Distinct().ToList();
        var preview = _workflow.BuildPreview(project, ids);
        var fingerprint = _fingerprinter.Compute(project, ids);

        var items = preview.Items
            .Select(x => new DeploymentUserPlanItem(
                x.Order,
                x.DeviceId,
                x.DeviceName,
                x.Endpoint,
                x.ProfileId,
                x.CanProceed,
                x.StatusRu,
                x.StatusEn))
            .ToList();

        var ready = items.Count(x => x.Ready);
        var summaryRu = preview.CanProceed
            ? $"Готово к передаче: {ready} из {items.Count}. Передача выполняется строго по порядку."
            : $"План заблокирован: {preview.Errors.Count} ошибок. Физическая передача не разрешена.";
        var summaryEn = preview.CanProceed
            ? $"Ready for deployment: {ready} of {items.Count}. Devices will be processed strictly in order."
            : $"Plan blocked: {preview.Errors.Count} errors. Physical deployment is not allowed.";

        return new(fingerprint, items, preview.Errors, preview.Warnings, summaryRu, summaryEn);
    }
}
