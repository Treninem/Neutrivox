using Neutrivox.Models;

namespace Neutrivox.Services;

public enum LicenseFeature
{
    BasicProjects,
    BasicIoConfiguration,
    BasicValidation,
    BasicSimulation,
    AdvancedLogic,
    DeviceDiscovery,
    DeviceDeployment,
    AdvancedDiagnostics,
    ProjectRecovery,
    ExportReports
}

/// <summary>One central feature policy for the product editions. UI and services can share the same decision.</summary>
public sealed class LicenseFeatureGateService
{
    public bool IsAllowed(ProductEdition edition, LicenseFeature feature) => feature switch
    {
        LicenseFeature.BasicProjects or LicenseFeature.BasicIoConfiguration or LicenseFeature.BasicValidation => true,
        LicenseFeature.BasicSimulation => edition != ProductEdition.Free || true,
        LicenseFeature.AdvancedLogic or LicenseFeature.DeviceDiscovery or LicenseFeature.ProjectRecovery => edition is ProductEdition.Standard or ProductEdition.Professional or ProductEdition.Owner,
        LicenseFeature.DeviceDeployment or LicenseFeature.AdvancedDiagnostics or LicenseFeature.ExportReports => edition is ProductEdition.Professional or ProductEdition.Owner,
        _ => false
    };
}

public sealed record LicenseEntitlement(ProductEdition Edition, DateTimeOffset? ExpiresAtUtc)
{
    public bool IsActive(DateTimeOffset nowUtc) => ExpiresAtUtc is null || ExpiresAtUtc > nowUtc;
}
