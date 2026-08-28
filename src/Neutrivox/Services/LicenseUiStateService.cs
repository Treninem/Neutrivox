using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LicenseUiState(
    LicenseSnapshot Snapshot,
    IReadOnlyList<LicensePlanView> PublicPlans,
    string Header,
    string Status,
    bool CanUseDiscovery,
    bool CanUseDeployment,
    bool ShowActivation);

public sealed class LicenseUiStateService
{
    private readonly LicensePresentationService _plans = new();
    private readonly FeatureAccessService _access = new();

    public LicenseUiState Build(LicenseSnapshot snapshot, AppLanguage language)
    {
        var views = _plans.GetPlans(language);
        var discovery = _access.Check(snapshot.Edition, CommercialFeature.DeviceDiscovery).Allowed;
        var deployment = _access.Check(snapshot.Edition, CommercialFeature.PhysicalDeployment).Allowed;
        var english = language == AppLanguage.English;
        var status = snapshot.State switch
        {
            LicenseState.Trial => english ? "Trial license is active." : "Пробный период активен.",
            LicenseState.Active => english ? "License is active." : "Лицензия активна.",
            LicenseState.Expired => english ? "License has expired." : "Срок лицензии истёк.",
            LicenseState.Invalid => english ? "License is invalid." : "Лицензия недействительна.",
            _ => english ? "License status is not available." : "Статус лицензии пока недоступен."
        };
        return new(
            snapshot,
            views,
            english ? $"Neutrivox — {snapshot.DisplayName}" : $"Neutrivox — {snapshot.DisplayName}",
            status,
            discovery,
            deployment,
            snapshot.Edition != ProductEdition.Owner);
    }
}
