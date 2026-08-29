using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LicenseEntitlements(
    bool BasicProject,
    bool OfflineSimulation,
    bool VisualWorkspace,
    bool LogicEditor,
    bool AdvancedDiagnostics,
    bool PhysicalDeviceIntegration,
    bool SequentialDeployment);

/// <summary>
/// Compatibility model for the policy service. Product edition/state are defined once
/// in Neutrivox.Models.Licensing and are intentionally reused here.
/// </summary>
public sealed record LegacyLicensePolicy(
    ProductEdition Edition,
    string LicenseId,
    DateTimeOffset? ExpiresAtUtc,
    bool Activated,
    string Source);
