using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox.LicenseServer;

public sealed class AccountLicenseService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(7);
    private readonly LicenseServerStore _store;
    private readonly CommercialPlanCatalogService _plans = new();
    private readonly RsaLicenseSignatureVerifier _verifier = new();

    public AccountLicenseService(LicenseServerStore store) => _store = store;

    public async Task<AccountSessionResponse> RegisterAsync(RegisterAccountRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (email is null) return Fail("Invalid email address.");
        if (!StrongPassword(request.Password)) return Fail("Password must contain at least 12 characters, including a letter and a digit.");
        if (string.IsNullOrWhiteSpace(request.DeviceFingerprint)) return Fail("Device fingerprint is required.");

        LicenseKeyPayload? payload;
        try { payload = JsonSerializer.Deserialize<LicenseKeyPayload>(request.LicenseKey); }
        catch (JsonException) { return Fail("License key format is invalid."); }
        if (payload is null || !_verifier.Verify(payload)) return Fail("License key signature is invalid.");
        var plan = _plans.Find(payload.PlanId);
        if (plan is null || plan.Edition == ProductEdition.Free) return Fail("This license cannot create a paid account.");
        if (payload.ExpiresAtUtc is not null && payload.ExpiresAtUtc <= DateTimeOffset.UtcNow) return Fail("License has expired.");
        if (!string.IsNullOrWhiteSpace(payload.BoundDeviceFingerprint) &&
            !string.Equals(payload.BoundDeviceFingerprint, request.DeviceFingerprint, StringComparison.Ordinal))
            return Fail("The purchase key is bound to another initial device.");

        var result = await _store.WriteAsync(database =>
        {
            if (database.Accounts.Any(x => x.EmailNormalized == email)) return Fail("An account with this email already exists.");
            if (database.ConsumedKeys.Any(x => x.KeyId == payload.KeyId)) return Fail("This purchase key has already been linked to an account.");

            var (salt, hash) = PasswordHasher.Hash(request.Password);
            var account = new LicenseAccount
            {
                EmailNormalized = email,
                EmailDisplay = request.Email.Trim(),
                PasswordSalt = salt,
                PasswordHash = hash,
                PlanId = plan.Id,
                EntitlementExpiresAtUtc = payload.ExpiresAtUtc,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            account.Devices.Add(NewDevice(request.DeviceFingerprint, request.DeviceName));
            database.Accounts.Add(account);
            database.ConsumedKeys.Add(new ConsumedLicenseKey { KeyId = payload.KeyId, AccountId = account.Id, ConsumedAtUtc = DateTimeOffset.UtcNow });
            return CreateSession(database, account, request.DeviceFingerprint, plan);
        }, cancellationToken);
        return result;
    }

    public async Task<AccountSessionResponse> LoginAsync(LoginAccountRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (email is null || string.IsNullOrWhiteSpace(request.DeviceFingerprint)) return Fail("Email and device fingerprint are required.");
        return await _store.WriteAsync(database =>
        {
            var account = database.Accounts.FirstOrDefault(x => x.EmailNormalized == email);
            if (account is null || !PasswordHasher.Verify(request.Password, account.PasswordSalt, account.PasswordHash))
                return Fail("Invalid email or password.");
            var plan = _plans.Find(account.PlanId);
            if (plan is null) return Fail("Account plan is unavailable.");
            if (account.EntitlementExpiresAtUtc is not null && account.EntitlementExpiresAtUtc <= DateTimeOffset.UtcNow)
                return Fail("Account subscription has expired.");

            var device = account.Devices.FirstOrDefault(x => x.Fingerprint == request.DeviceFingerprint);
            if (device is null)
            {
                var limit = DeviceLimit(plan);
                if (account.Devices.Count >= limit)
                    return Fail($"Device limit reached ({limit}). Revoke an old device before adding a new one.");
                account.Devices.Add(NewDevice(request.DeviceFingerprint, request.DeviceName));
            }
            else
            {
                device.LastSeenAtUtc = DateTimeOffset.UtcNow;
                if (!string.IsNullOrWhiteSpace(request.DeviceName)) device.Name = request.DeviceName.Trim();
            }
            return CreateSession(database, account, request.DeviceFingerprint, plan);
        }, cancellationToken);
    }

    public async Task<AccountSessionResponse> StatusAsync(string token, CancellationToken cancellationToken)
    {
        var hash = HashToken(token);
        return await _store.WriteAsync(database =>
        {
            CleanupSessions(database);
            var session = database.Sessions.FirstOrDefault(x => x.TokenHash == hash && x.ExpiresAtUtc > DateTimeOffset.UtcNow);
            if (session is null) return Fail("Session is invalid or expired.");
            var account = database.Accounts.FirstOrDefault(x => x.Id == session.AccountId);
            if (account is null) return Fail("Account no longer exists.");
            var plan = _plans.Find(account.PlanId);
            if (plan is null) return Fail("Account plan is unavailable.");
            if (account.EntitlementExpiresAtUtc is not null && account.EntitlementExpiresAtUtc <= DateTimeOffset.UtcNow)
                return Fail("Account subscription has expired.");
            var device = account.Devices.FirstOrDefault(x => x.Fingerprint == session.DeviceFingerprint);
            if (device is null) return Fail("This device is no longer authorized for the account.");
            device.LastSeenAtUtc = DateTimeOffset.UtcNow;
            return View(account, plan, token, "Account is active.");
        }, cancellationToken);
    }

    public async Task<AccountSessionResponse> RevokeDeviceAsync(string token, RevokeDeviceRequest request, CancellationToken cancellationToken)
    {
        var hash = HashToken(token);
        return await _store.WriteAsync(database =>
        {
            CleanupSessions(database);
            var session = database.Sessions.FirstOrDefault(x => x.TokenHash == hash && x.ExpiresAtUtc > DateTimeOffset.UtcNow);
            if (session is null) return Fail("Session is invalid or expired.");
            if (string.Equals(session.DeviceFingerprint, request.Fingerprint, StringComparison.Ordinal))
                return Fail("The current device cannot revoke itself from the active session.");
            var account = database.Accounts.FirstOrDefault(x => x.Id == session.AccountId);
            if (account is null) return Fail("Account no longer exists.");
            account.Devices.RemoveAll(x => x.Fingerprint == request.Fingerprint);
            database.Sessions.RemoveAll(x => x.AccountId == account.Id && x.DeviceFingerprint == request.Fingerprint);
            var plan = _plans.Find(account.PlanId)!;
            return View(account, plan, token, "Device revoked.");
        }, cancellationToken);
    }

    private static AccountSessionResponse CreateSession(LicenseServerDatabase database, LicenseAccount account, string fingerprint, CommercialPlan plan)
    {
        CleanupSessions(database);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        database.Sessions.Add(new AccountSession
        {
            TokenHash = HashToken(token),
            AccountId = account.Id,
            DeviceFingerprint = fingerprint,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(SessionLifetime)
        });
        return View(account, plan, token, "Signed in.");
    }

    private static AccountSessionResponse View(LicenseAccount account, CommercialPlan plan, string? token, string message) =>
        new(true, token, plan.Id, plan.Edition.ToString(), account.EntitlementExpiresAtUtc, DeviceLimit(plan),
            account.Devices.Select(x => new AccountDeviceView(x.Fingerprint, x.Name, x.AddedAtUtc, x.LastSeenAtUtc)).ToList(), message);

    private static AccountDevice NewDevice(string fingerprint, string? name) => new()
    {
        Fingerprint = fingerprint.Trim(),
        Name = string.IsNullOrWhiteSpace(name) ? "Windows PC" : name.Trim(),
        AddedAtUtc = DateTimeOffset.UtcNow,
        LastSeenAtUtc = DateTimeOffset.UtcNow
    };

    private static int DeviceLimit(CommercialPlan plan) => plan.Edition switch
    {
        ProductEdition.Standard => 2,
        ProductEdition.Professional => 3,
        ProductEdition.Business => 5,
        ProductEdition.Owner => 10,
        _ => 1
    };

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var normalized = email.Trim().ToLowerInvariant();
        return normalized.Contains('@') && normalized.Length <= 254 ? normalized : null;
    }

    private static bool StrongPassword(string? password) =>
        password is { Length: >= 12 } && password.Any(char.IsLetter) && password.Any(char.IsDigit);

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()))).ToLowerInvariant();
    private static void CleanupSessions(LicenseServerDatabase database) => database.Sessions.RemoveAll(x => x.ExpiresAtUtc <= DateTimeOffset.UtcNow);
    private static AccountSessionResponse Fail(string message) => new(false, null, null, null, null, 0, [], message);
}
