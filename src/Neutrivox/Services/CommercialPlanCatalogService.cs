using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Central source of public commercial plans. Prices are stored as RUB and can be changed without
/// coupling the application to the external sales platform.
/// </summary>
public sealed class CommercialPlanCatalogService
{
    private static readonly IReadOnlyList<CommercialPlan> Plans =
    [
        new("free", ProductEdition.Free, "Free", "Free", 0m, null, false, true,
            "Минимальный базовый функционал без оплаты.",
            "Minimal basic functionality at no cost."),
        new("standard-30d", ProductEdition.Standard, "Standard — 30 дней", "Standard — 30 days", 499m, 30, true, true,
            "Расширенные возможности для регулярной работы.",
            "Extended features for regular work."),
        new("professional-30d", ProductEdition.Professional, "Professional — 30 дней", "Professional — 30 days", 999m, 30, true, true,
            "Профессиональный набор инструментов и расширенная конфигурация.",
            "Professional tools and extended configuration."),
        new("professional-365d", ProductEdition.Professional, "Professional — 1 год", "Professional — 1 year", 7990m, 365, true, true,
            "Профессиональный доступ на один год.",
            "Professional access for one year."),
        new("owner-lifetime", ProductEdition.Owner, "Owner Lifetime Max", "Owner Lifetime Max", 0m, null, false, false,
            "Персональная бессрочная лицензия владельца. Не выставляется на продажу.",
            "Personal perpetual owner license. Not offered for public sale.")
    ];

    public IReadOnlyList<CommercialPlan> GetPublicPlans() => Plans.Where(x => x.IsPubliclySellable).ToList();

    public IReadOnlyList<CommercialPlan> GetAllPlans() => Plans;

    public CommercialPlan? Find(string id) => Plans.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
