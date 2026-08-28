using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ReleaseReadinessCheck(string Id, bool Passed, string MessageRu, string MessageEn);

public sealed record ReleaseReadinessReport(
    bool Ready,
    IReadOnlyList<ReleaseReadinessCheck> Checks,
    string SummaryRu,
    string SummaryEn);

/// <summary>
/// Central non-destructive release gate for the application. It checks product-level prerequisites
/// without attempting to connect to or modify physical equipment.
/// </summary>
public sealed class ReleaseReadinessService
{
    private readonly ProjectIntegrationFacade _integration = new();
    private readonly ProjectPersistenceService _persistence = new();
    private readonly ProjectIntegrityService _integrity = new();

    public ReleaseReadinessReport Evaluate(AutomationProject? project)
    {
        var checks = new List<ReleaseReadinessCheck>();

        checks.Add(new(
            "PROJECT_EXISTS",
            project is not null,
            project is null ? "Проект не открыт." : "Проект открыт.",
            project is null ? "No project is open." : "A project is open."));

        if (project is not null)
        {
            var integration = _integration.Build(project);
            checks.Add(new(
                "PROJECT_INTEGRITY",
                _integrity.Check(project).IsValid,
                _integrity.Check(project).IsValid ? "Базовая целостность проекта подтверждена." : "В проекте найдены критические нарушения структуры.",
                _integrity.Check(project).IsValid ? "Basic project integrity is valid." : "Critical structural issues were found in the project."));

            checks.Add(new(
                "PERSISTENCE_ROUNDTRIP",
                CanRoundTrip(project),
                CanRoundTrip(project) ? "Проект проходит локальный round-trip сохранение/загрузка." : "Round-trip сохранения/загрузки не пройден.",
                CanRoundTrip(project) ? "Project passes local save/load round-trip." : "Project save/load round-trip failed."));

            checks.Add(new(
                "SIMULATION_STRUCTURE",
                integration.CanSimulate,
                integration.CanSimulate ? "Проект допускает симуляцию." : "Проект пока не готов к симуляции.",
                integration.CanSimulate ? "Project is ready for simulation." : "Project is not ready for simulation."));

            checks.Add(new(
                "BLOCKING_DIAGNOSTICS",
                !integration.HasBlockingDiagnostics,
                integration.HasBlockingDiagnostics ? "Есть блокирующие ошибки диагностики." : "Блокирующих ошибок диагностики нет.",
                integration.HasBlockingDiagnostics ? "Blocking diagnostics are present." : "No blocking diagnostics are present."));
        }

        var ready = checks.All(x => x.Passed);
        return new(
            ready,
            checks,
            ready ? "Проект прошёл выпускную предварительную проверку." : "Проект не прошёл выпускную предварительную проверку.",
            ready ? "The project passed the release pre-check." : "The project did not pass the release pre-check.");
    }

    private bool CanRoundTrip(AutomationProject project)
    {
        try
        {
            var text = _persistence.Serialize(project);
            var loaded = _persistence.Deserialize(text);
            return loaded.Success && loaded.Project is not null && loaded.Project.Id == project.Id;
        }
        catch
        {
            return false;
        }
    }
}
