namespace Neutrivox.Models;

public sealed record AccountRegisterRequest(string Email, string Password, string LicenseKey, string DeviceFingerprint, string? DeviceName);
public sealed record AccountLoginRequest(string Email, string Password, string DeviceFingerprint, string? DeviceName);
public sealed record AccountDeviceInfo(string Fingerprint, string Name, DateTimeOffset AddedAtUtc, DateTimeOffset LastSeenAtUtc);
public sealed record AccountSessionInfo(
    bool Success,
    string? SessionToken,
    string? PlanId,
    string? Edition,
    DateTimeOffset? ExpiresAtUtc,
    int DeviceLimit,
    IReadOnlyList<AccountDeviceInfo> Devices,
    string Message);
