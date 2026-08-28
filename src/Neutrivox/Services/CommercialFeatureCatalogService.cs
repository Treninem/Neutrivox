using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record CommercialFeatureDefinition(
    CommercialFeature Feature,
    string NameRu,
    string NameEn,
    ProductEdition MinimumEdition,
    string DescriptionRu,
    string DescriptionEn);

public sealed class CommercialFeatureCatalogService
{
    private static readonly IReadOnlyList<CommercialFeatureDefinition> Definitions =
    [
        new(CommercialFeature.BasicProjects, "Базовые проекты", "Basic projects", ProductEdition.Free, "Создание и редактирование проекта.", "Create and edit projects."),
        new(CommercialFeature.BasicEquipment, "Базовое оборудование", "Basic equipment", ProductEdition.Free, "Добавление оборудования из доступного каталога.", "Add equipment from the available catalog."),
        new(CommercialFeature.BasicIo, "Входы и выходы", "Inputs and outputs", ProductEdition.Free, "Базовая настройка каналов.", "Basic channel configuration."),
        new(CommercialFeature.BasicLogic, "Базовая логика", "Basic logic", ProductEdition.Free, "Создание простых логических сетей.", "Create simple logic networks."),
        new(CommercialFeature.BasicSimulation, "Базовая симуляция", "Basic simulation", ProductEdition.Free, "Запуск виртуальной логики без прибора.", "Run virtual logic without hardware."),
        new(CommercialFeature.ProjectSaveLoad, "Сохранение проекта", "Project save/load", ProductEdition.Free, "Сохранение и повторное открытие проекта.", "Save and reopen a project."),
        new(CommercialFeature.DeviceDiscovery, "Обнаружение устройств", "Device discovery", ProductEdition.Standard, "Поиск поддерживаемых устройств через доступные интерфейсы.", "Discover supported devices through available interfaces."),
        new(CommercialFeature.AdvancedSimulation, "Расширенная симуляция", "Advanced simulation", ProductEdition.Standard, "Сценарии, снимки и расширенная диагностика симуляции.", "Scenarios, snapshots and advanced simulation diagnostics."),
        new(CommercialFeature.DeploymentPreparation, "Подготовка передачи", "Deployment preparation", ProductEdition.Standard, "Проверка и формирование последовательного плана передачи.", "Validate and create a sequential deployment plan."),
        new(CommercialFeature.AdvancedDiagnostics, "Расширенная диагностика", "Advanced diagnostics", ProductEdition.Professional, "Расширенные проверки проекта и оборудования.", "Extended project and equipment diagnostics."),
        new(CommercialFeature.BatchDeployment, "Последовательная загрузка", "Sequential deployment", ProductEdition.Professional, "Обработка нескольких устройств по очереди.", "Process multiple devices sequentially."),
        new(CommercialFeature.PhysicalDeployment, "Физическая передача", "Physical deployment", ProductEdition.Professional, "Реальная передача только через подтверждённый адаптер поддерживаемого устройства.", "Real transfer only through a verified adapter for a supported device."),
        new(CommercialFeature.ExtendedDocumentation, "Расширенная документация", "Extended documentation", ProductEdition.Standard, "Расширенные инструкции и отчёты.", "Extended guides and reports.")
    ];

    public IReadOnlyList<CommercialFeatureDefinition> GetAll() => Definitions;
    public CommercialFeatureDefinition? Find(CommercialFeature feature) => Definitions.FirstOrDefault(x => x.Feature == feature);
}
