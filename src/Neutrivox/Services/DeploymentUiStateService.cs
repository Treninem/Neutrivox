using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentUiState(
    DeploymentSequencePlan Sequence,
    int Completed,
    int Total,
    bool CanStart,
    bool IsRunning,
    string StatusRu,
    string StatusEn);

/// <summary>Derives presentation state for the deployment screen without performing physical I/O.</summary>
public sealed class DeploymentUiStateService
{
    public DeploymentUiState Build(DeploymentPlan plan, DeploymentSequencePlan sequence, bool gateAllowed, bool running, int completed)
    {
        var total = sequence.Items.Count;
        var canStart = gateAllowed && total > 0 && !running;
        var statusRu = running
            ? $"Выполняется: {completed} из {total}"
            : !gateAllowed
                ? "Передача заблокирована до устранения ошибок."
                : total == 0
                    ? "Нет подготовленных устройств."
                    : completed == total
                        ? "Все подготовленные устройства обработаны."
                        : "Готово к явному запуску передачи.";
        var statusEn = running
            ? $"Running: {completed} of {total}"
            : !gateAllowed
                ? "Deployment is blocked until the errors are resolved."
                : total == 0
                    ? "No prepared devices."
                    : completed == total
                        ? "All prepared devices have been processed."
                        : "Ready for explicit deployment start.";
        return new(sequence, completed, total, canStart, running, statusRu, statusEn);
    }
}
