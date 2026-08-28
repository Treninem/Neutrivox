using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LicensePlanView(
    string Id,
    string Name,
    string Price,
    string Duration,
    string Description,
    bool PubliclySellable);

public sealed class LicensePresentationService
{
    private readonly CommercialPlanCatalogService _plans = new();
    private readonly LocalizationService _localization = new();

    public IReadOnlyList<LicensePlanView> GetPlans(AppLanguage language)
    {
        _localization.SetLanguage(language);
        return _plans.GetPublicPlans().Select(plan => new LicensePlanView(
            plan.Id,
            _localization.Get(plan.NameRu, plan.NameEn),
            plan.PriceRub == 0m ? _localization.Get("Бесплатно", "Free") : $"{plan.PriceRub:0} ₽",
            plan.DurationDays is null ? _localization.Get("Бессрочно", "Perpetual") : $"{plan.DurationDays} {(_localization.Language == AppLanguage.Russian ? "дн." : "days")}",
            _localization.Get(plan.DescriptionRu, plan.DescriptionEn),
            plan.IsPubliclySellable)).ToList();
    }
}
