using System.Text.Json;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record LicenseRuntimeState(
    LicenseSnapshot Snapshot,
    string Fingerprint,
    string StatusRu,
    string StatusEn,
    DateTimeOffset? TrialEndsAtUtc,
    bool ClockRollbackDetected);

internal sealed class LocalLicenseState
{
    public DateTimeOffset TrialStartedAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public string? ActivatedLicenseKey { get; set; }
}

/// <summary>
/// Persists local trial/activation state without storing any private signing secret.
/// A signed paid key is re-verified on every application start.
/// </summary>
public sealed class LocalLicenseService
{
    public const int TrialDays = 7;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly DeviceFingerprintService _fingerprints = new();
    private readonly CommercialPlanCatalogService _plans = new();
    private readonly RsaLicenseSignatureVerifier _verifier = new();
    private readonly TrialAnchorService? _trialAnchor;
    private readonly string _statePath;

    public LocalLicenseService(string? statePath = null)
    {
        _statePath = statePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Neutrivox", "license-state.json");
        _trialAnchor = statePath is null ? new TrialAnchorService() : null;
    }

    public LicenseRuntimeState GetCurrent(DateTimeOffset nowUtc)
    {
        var fingerprint = _fingerprints.GetFingerprint();
        var state = LoadOrCreate(nowUtc, fingerprint);
        var rollback = nowUtc < state.LastSeenAtUtc.AddMinutes(-5);

        if (!rollback && !string.IsNullOrWhiteSpace(state.ActivatedLicenseKey))
        {
            var paid = ValidateStoredLicense(state.ActivatedLicenseKey, fingerprint, nowUtc);
            if (paid is not null)
            {
                state.LastSeenAtUtc = Max(state.LastSeenAtUtc, nowUtc);
                Save(state);
                return paid with { ClockRollbackDetected = false };
            }
        }

        var trialEnds = state.TrialStartedAtUtc.AddDays(TrialDays);
        if (!rollback && nowUtc < trialEnds && string.Equals(state.Fingerprint, fingerprint, StringComparison.Ordinal))
        {
            state.LastSeenAtUtc = Max(state.LastSeenAtUtc, nowUtc);
            Save(state);
            var remaining = Math.Max(0, (int)Math.Ceiling((trialEnds - nowUtc).TotalDays));
            return new(
                new LicenseSnapshot(ProductEdition.Professional, LicenseState.Trial, trialEnds, "Professional Trial"),
                fingerprint,
                $"Пробный Professional активен. Осталось дней: {remaining}.",
                $"Professional trial is active. Days remaining: {remaining}.",
                trialEnds,
                false);
        }

        state.LastSeenAtUtc = Max(state.LastSeenAtUtc, nowUtc);
        Save(state);
        var reasonRu = rollback
            ? "Обнаружено некорректное системное время. Пробный доступ отключён; Free остаётся доступным."
            : "Пробный период завершён. Доступен тариф Free.";
        var reasonEn = rollback
            ? "A system clock rollback was detected. Trial access is disabled; Free remains available."
            : "The trial period has ended. Free edition remains available.";
        return new(
            new LicenseSnapshot(ProductEdition.Free, LicenseState.Active, null, "Free"),
            fingerprint,
            reasonRu,
            reasonEn,
            trialEnds,
            rollback);
    }

    public LicenseActivationResult Activate(string licenseKey, DateTimeOffset nowUtc)
    {
        var fingerprint = _fingerprints.GetFingerprint();
        var service = new LicenseActivationService(_plans, _verifier);
        var result = service.Activate(new LicenseActivationRequest(licenseKey.Trim(), fingerprint), nowUtc);
        if (!result.Success) return result;

        var state = LoadOrCreate(nowUtc, fingerprint);
        state.ActivatedLicenseKey = licenseKey.Trim();
        state.LastSeenAtUtc = Max(state.LastSeenAtUtc, nowUtc);
        state.Fingerprint = fingerprint;
        Save(state);
        return result;
    }

    private LicenseRuntimeState? ValidateStoredLicense(string key, string fingerprint, DateTimeOffset nowUtc)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<LicenseKeyPayload>(key);
            if (payload is null || !_verifier.Verify(payload)) return null;
            var plan = _plans.Find(payload.PlanId);
            if (plan is null) return null;
            if (plan.IsPubliclySellable && plan.PriceRub > 0m && string.IsNullOrWhiteSpace(payload.BoundDeviceFingerprint)) return null;
            if (!string.IsNullOrWhiteSpace(payload.BoundDeviceFingerprint) &&
                !string.Equals(payload.BoundDeviceFingerprint, fingerprint, StringComparison.Ordinal)) return null;
            if (payload.ExpiresAtUtc is not null && payload.ExpiresAtUtc <= nowUtc) return null;
            return new(
                new LicenseSnapshot(plan.Edition, LicenseState.Active, payload.ExpiresAtUtc, plan.NameEn),
                fingerprint,
                $"Активна лицензия {plan.NameRu}.",
                $"{plan.NameEn} license is active.",
                null,
                false);
        }
        catch (JsonException) { return null; }
    }

    private LocalLicenseState LoadOrCreate(DateTimeOffset nowUtc, string fingerprint)
    {
        var anchor = _trialAnchor?.ReadStartedAtUtc();
        try
        {
            if (File.Exists(_statePath))
            {
                var existing = JsonSerializer.Deserialize<LocalLicenseState>(File.ReadAllText(_statePath), JsonOptions);
                if (existing is not null && existing.TrialStartedAtUtc != default && !string.IsNullOrWhiteSpace(existing.Fingerprint))
                {
                    if (anchor is DateTimeOffset anchored && anchored < existing.TrialStartedAtUtc)
                        existing.TrialStartedAtUtc = anchored;
                    _trialAnchor?.WriteEarliest(existing.TrialStartedAtUtc);
                    return existing;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (JsonException) { }

        var startedAt = anchor is DateTimeOffset previous && previous < nowUtc ? previous : nowUtc;
        var created = new LocalLicenseState
        {
            TrialStartedAtUtc = startedAt,
            LastSeenAtUtc = nowUtc,
            Fingerprint = fingerprint
        };
        _trialAnchor?.WriteEarliest(startedAt);
        Save(created);
        return created;
    }

    private void Save(LocalLicenseState state)
    {
        try
        {
            var directory = Path.GetDirectoryName(_statePath)!;
            Directory.CreateDirectory(directory);
            var temporary = _statePath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(state, JsonOptions));
            File.Move(temporary, _statePath, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static DateTimeOffset Max(DateTimeOffset left, DateTimeOffset right) => left >= right ? left : right;
}
