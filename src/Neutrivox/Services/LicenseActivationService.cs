using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public interface ILicenseSignatureVerifier
{
    bool Verify(LicenseKeyPayload payload);
}

/// <summary>
/// Parses and validates activation requests. Cryptographic verification is delegated to an injected verifier;
/// private signing material never belongs in the public repository.
/// </summary>
public sealed class LicenseActivationService
{
    private readonly CommercialPlanCatalogService _plans;
    private readonly ILicenseSignatureVerifier _signatureVerifier;
    private readonly HashSet<string> _activatedKeys = new(StringComparer.Ordinal);

    public LicenseActivationService(CommercialPlanCatalogService plans, ILicenseSignatureVerifier signatureVerifier)
    {
        _plans = plans;
        _signatureVerifier = signatureVerifier;
    }

    public LicenseActivationResult Activate(LicenseActivationRequest request, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseKey))
            return Fail("Введите лицензионный ключ.", "Enter a license key.");
        if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
            return Fail("Не удалось определить устройство активации.", "Activation device fingerprint is missing.");

        LicenseKeyPayload? payload;
        try { payload = JsonSerializer.Deserialize<LicenseKeyPayload>(request.LicenseKey); }
        catch (JsonException) { return Fail("Формат лицензионного ключа неверен.", "License key format is invalid."); }

        if (payload is null || string.IsNullOrWhiteSpace(payload.KeyId) || string.IsNullOrWhiteSpace(payload.PlanId) || string.IsNullOrWhiteSpace(payload.Signature))
            return Fail("Лицензионный ключ неполный.", "License key is incomplete.");
        if (!_signatureVerifier.Verify(payload))
            return Fail("Подпись лицензионного ключа недействительна.", "License key signature is invalid.");

        var plan = _plans.Find(payload.PlanId);
        if (plan is null || !plan.IsPubliclySellable && plan.Edition != ProductEdition.Owner)
            return Fail("Лицензионный план не поддерживается.", "License plan is not supported.");
        if (payload.ExpiresAtUtc is not null && payload.ExpiresAtUtc <= nowUtc)
            return Fail("Срок действия лицензии истёк.", "License has expired.");
        if (!_activatedKeys.Add(payload.KeyId))
            return Fail("Этот ключ уже активирован на текущем экземпляре.", "This key is already activated in the current instance.");

        return new(true, plan.Edition, payload.ExpiresAtUtc, $"Лицензия {plan.NameRu} активирована.", $"License {plan.NameEn} activated.");
    }

    private static LicenseActivationResult Fail(string ru, string en) => new(false, null, null, ru, en);
}
