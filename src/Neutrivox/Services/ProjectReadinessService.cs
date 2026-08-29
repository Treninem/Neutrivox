using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Produces the canonical project readiness report used by the whole application.
/// It intentionally returns Neutrivox.Models.ProjectReadinessReport so readiness is not duplicated
/// in the Services namespace.
/// </summary>
public sealed class ProjectReadinessService
{
    public ProjectReadinessReport Evaluate(AutomationProject project)
    {
        var report = new ProjectReadinessReport();

        if (project.Devices.Count == 0)
        {
            report.Items.Add(new(
                ProjectReadinessLevel.Blocking,
                "NO_DEVICES",
                "Нет оборудования",
                "В проекте нет устройств.",
                "Добавьте хотя бы одно поддерживаемое устройство."));
        }
        else
        {
            foreach (var device in project.Devices)
            {
                if (string.IsNullOrWhiteSpace(device.Name))
                {
                    report.Items.Add(new(
                        ProjectReadinessLevel.Warning,
                        "DEVICE_NO_NAME",
                        "Устройство без имени",
                        "Для понятной идентификации в проекте и при последовательной передаче устройству требуется имя.",
                        "Задайте понятное имя устройства."));
                }

                if (string.IsNullOrWhiteSpace(device.DefinitionId))
                {
                    report.Items.Add(new(
                        ProjectReadinessLevel.Blocking,
                        "DEVICE_NO_PROFILE",
                        "Нет профиля устройства",
                        "Устройство проекта не связано с профилем каталога.",
                        "Выберите поддерживаемую модель устройства."));
                }
            }
        }

        var channelCount = project.Devices.Sum(x => x.Channels.Count);
        if (channelCount == 0)
        {
            report.Items.Add(new(
                ProjectReadinessLevel.Warning,
                "NO_CHANNELS",
                "Нет каналов I/O",
                "В проекте пока нет настроенных каналов ввода/вывода.",
                "Добавьте оборудование с каналами I/O."));
        }

        report.Items.Add(new(
            ProjectReadinessLevel.Information,
            "SIMULATION_AVAILABLE",
            "Симуляция доступна",
            "Проект можно разрабатывать и проверять без физического оборудования.",
            "Переходите к симуляции до подключения приборов."));

        report.Items.Add(new(
            ProjectReadinessLevel.Information,
            "DEPLOYMENT_REQUIRES_VERIFICATION",
            "Физическая передача требует проверки",
            "Передача выполняется только после проверки профиля, физической привязки и адаптера.",
            "Сначала выполните discovery и подтверждение привязки."));

        return report;
    }
}
