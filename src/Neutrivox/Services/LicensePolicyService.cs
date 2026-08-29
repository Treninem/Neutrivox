using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class LicensePolicyService
{
    public LicenseEntitlements GetEntitlements(ProductEdition edition) => edition switch
    {
        ProductEdition.Free => new(true, true, true, true, false, false, false),
        ProductEdition.Standard => new(true, true, true, true, true, false, false),
        ProductEdition.Professional => new(true, true, true, true, true, true, true),
        ProductEdition.Business => new(true, true, true, true, true, true, true),
        ProductEdition.Owner => new(true, true, true, true, true, true, true),
        _ => new(true, false, false, false, false, false, false)
    };

    public bool IsActive(LicenseState state, DateTimeOffset? expiresAtUtc, DateTimeOffset utcNow)
        => (state is LicenseState.Trial or LicenseState.Active)
           && (expiresAtUtc is null || expiresAtUtc > utcNow);

    public bool CanUse(
        LicenseSnapshot snapshot,
        DateTimeOffset utcNow,
        Func<LicenseEntitlements, bool> requirement)
    {
        if (!snapshot.IsUsable || (snapshot.ExpiresAtUtc is not null && snapshot.ExpiresAtUtc <= utcNow))
            return false;
        return requirement(GetEntitlements(snapshot.Edition));
    }
}
