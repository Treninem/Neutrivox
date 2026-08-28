namespace Neutrivox.Models;

public sealed record CommercialPlan(
    string Id,
    ProductEdition Edition,
    string NameRu,
    string NameEn,
    decimal PriceRub,
    int? DurationDays,
    bool IsSubscription,
    bool IsPubliclySellable,
    string DescriptionRu,
    string DescriptionEn);

public sealed record LicenseActivationRequest(string LicenseKey, string DeviceFingerprint);

public sealed record LicenseActivationResult(
    bool Success,
    ProductEdition? Edition,
    DateTimeOffset? ExpiresAtUtc,
    string MessageRu,
    string MessageEn);

public sealed record LicenseKeyPayload(
    string KeyId,
    string PlanId,
    string Subject,
    string? BoundDeviceFingerprint,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset? ExpiresAtUtc,
    string Signature);
