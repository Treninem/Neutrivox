using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Adds deployment-specific checks to the canonical project readiness model.
/// There is intentionally no second readiness-report type in Services.
/// </summary>
public sealed class DeploymentReadinessService
{
    private readonly LogicCompilationService _logic = new();
    private readonly ProjectReadinessService _projectReadiness = new();

    public ProjectReadinessReport Analyze(AutomationProject project)
    {
        var report = _projectReadiness.Evaluate(project);

        var compilation = _logic.Compile(project);
        foreach (var error in compilation.Errors)
        {
            report.Items.Add(new(
                ProjectReadinessLevel.Blocking,
                "LOGIC_INVALID",
                "Ошибка логики",
                error,
                "Исправьте логику и выполните проверку повторно."));
        }

        foreach (var device in project.Devices)
        {
            if (device.PhysicalBinding is null)
            {
                report.Items.Add(new(
                    ProjectReadinessLevel.Warning,
                    "DEVICE_NOT_BOUND",
                    "Прибор не привязан",
                    $"Устройство «{device.Name}» пока существует только как цифровая модель.",
                    "Для физической передачи выполните обнаружение и сопоставление прибора."));
                continue;
            }

            if (!string.Equals(device.PhysicalBinding.IdentificationState, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                report.Items.Add(new(
                    ProjectReadinessLevel.Warning,
                    "DEVICE_UNVERIFIED",
                    "Оборудование не подтверждено",
                    $"Физический прибор «{device.Name}» найден, но его идентификация ещё не подтверждена.",
                    "Проверьте модель, модификацию и endpoint перед передачей."));
            }
        }

        return report;
    }
}
