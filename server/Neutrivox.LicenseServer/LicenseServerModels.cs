namespace Neutrivox.LicenseServer;

public sealed class LicenseServerDatabase
{
    public List<LicenseAccount> Accounts { get; set; } = [];
    public List<ConsumedLicenseKey> ConsumedKeys { get; set; } = [];
    public List<AccountSession> Sessions { get; set; } = [];
}

public sealed class LicenseAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmailNormalized { get; set; } = string.Empty;
    public string EmailDisplay { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public DateTimeOffset? EntitlementExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<AccountDevice> Devices { get; set; } = [];
}

public sealed class AccountDevice
{
    public string Fingerprint { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset AddedAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
}

public sealed class ConsumedLicenseKey
{
    public string KeyId { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public DateTimeOffset ConsumedAtUtc { get; set; }
}

public sealed class AccountSession
{
    public string TokenHash { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public sealed record RegisterAccountRequest(string Email, string Password, string LicenseKey, string DeviceFingerprint, string? DeviceName);
public sealed record LoginAccountRequest(string Email, string Password, string DeviceFingerprint, string? DeviceName);
public sealed record RevokeDeviceRequest(string Fingerprint);
public sealed record AccountSessionResponse(bool Success, string? SessionToken, string? PlanId, string? Edition, DateTimeOffset? ExpiresAtUtc, int DeviceLimit, IReadOnlyList<AccountDeviceView> Devices, string Message);
public sealed record AccountDeviceView(string Fingerprint, string Name, DateTimeOffset AddedAtUtc, DateTimeOffset LastSeenAtUtc);
