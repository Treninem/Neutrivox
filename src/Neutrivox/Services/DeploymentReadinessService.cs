using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeploymentReadinessService
{
    private readonly LogicCompilationService _logic = new();

    public ProjectReadinessReport Analyze(AutomationProject project)
    {
        var report = new ProjectReadinessReport();
        if (project.Devices.Count == 0)
            report.Items.Add(new(ProjectReadinessLevel.Blocking, "NO_DEVICES", "Нет оборудования", "В проекте нет устройств.", "Добавьте поддерживаемое устройство или модуль."));

        foreach (var device in project.Devices)
        {
            if (string.IsNullOrWhiteSpace(device.Name))
                report.Items.Add(new(ProjectReadinessLevel.Warning, "DEVICE_NO_NAME", "Устройство без имени", "Для понятной последовательной загрузки каждому устройству нужно имя.", "Задайте понятное имя устройства."));
            if (string.IsNullOrWhiteSpace(device.DefinitionId))
                report.Items.Add(new(ProjectReadinessLevel.Blocking, "DEVICE_NO_PROFILE", "Нет профиля устройства", "Устройство проекта не связано с поддерживаемым профилем.", "Выберите профиль перед подготовкой передачи."));
        }

        var compilation = _logic.Compile(project);
        foreach (var error in compilation.Errors)
            report.Items.Add(new(ProjectReadinessLevel.Blocking, "LOGIC_INVALID", "Ошибка логики", error, "Исправьте логику и выполните проверку повторно."));

        if (project.Devices.Any(x => x.PhysicalBinding is not null && x.PhysicalBinding.IdentificationState != "Verified"))
            report.Items.Add(new(ProjectReadinessLevel.Warning, "DEVICE_UNVERIFIED", "Оборудование не подтверждено", "Обнаруженный или назначенный физический прибор ещё не имеет подтверждённого состояния.", "Проверьте модель и адрес прибора перед передачей."));

        return report;
    }
}
