namespace Neutrivox.Services;

public enum LicenseTier
{
    Free,
    Professional,
    OwnerPerpetual
}

public sealed record LicenseEntitlements(
    bool BasicProject,
    bool OfflineSimulation,
    bool VisualWorkspace,
    bool LogicEditor,
    bool AdvancedDiagnostics,
    bool PhysicalDeviceIntegration,
    bool SequentialDeployment);

public sealed record LicenseState(
    LicenseTier Tier,
    string LicenseId,
    DateTime? ExpiresAtUtc,
    bool Activated,
    string Source);
