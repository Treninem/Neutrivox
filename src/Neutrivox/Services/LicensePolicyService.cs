namespace Neutrivox.Services;

public sealed class LicensePolicyService
{
    public LicenseEntitlements GetEntitlements(LicenseTier tier) => tier switch
    {
        LicenseTier.Free => new(true, true, true, true, true, false, false),
        LicenseTier.Professional => new(true, true, true, true, true, true, true),
        LicenseTier.OwnerPerpetual => new(true, true, true, true, true, true, true),
        _ => new(true, false, false, false, false, false, false)
    };

    public bool IsActive(LicenseState state, DateTime utcNow) =>
        state.Activated && (state.ExpiresAtUtc is null || state.ExpiresAtUtc > utcNow);

    public bool CanUse(LicenseState state, DateTime utcNow, Func<LicenseEntitlements, bool> requirement)
    {
        if (!IsActive(state, utcNow)) return false;
        return requirement(GetEntitlements(state.Tier));
    }
}
