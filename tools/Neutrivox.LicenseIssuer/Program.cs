using System.Security.Cryptography;
using System.Text.Json;
using Neutrivox.Models;
using Neutrivox.Services;

static string? Arg(string name, string[] input)
{
    var index = Array.FindIndex(input, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < input.Length ? input[index + 1] : null;
}

var privateKeyPath = Arg("--private-key", args);
var planId = Arg("--plan", args) ?? "professional-30d";
var subject = Arg("--subject", args) ?? "customer";
var fingerprint = Arg("--fingerprint", args);
var output = Arg("--output", args);
var durationText = Arg("--days", args);

if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
{
    Console.Error.WriteLine("Usage: --private-key <pem> [--plan professional-30d] [--subject name] --fingerprint <customer fingerprint> [--days N] [--output file]");
    return 2;
}

var plans = new CommercialPlanCatalogService();
var plan = plans.Find(planId);
if (plan is null)
{
    Console.Error.WriteLine($"Unknown plan: {planId}");
    return 3;
}
if (plan.IsPubliclySellable && plan.PriceRub > 0m && string.IsNullOrWhiteSpace(fingerprint))
{
    Console.Error.WriteLine("Paid public licenses must include --fingerprint so the key cannot be transferred to another customer device.");
    return 4;
}

var issuedAt = DateTimeOffset.UtcNow;
var duration = int.TryParse(durationText, out var explicitDays) ? explicitDays : plan.DurationDays;
var expires = duration is > 0 ? issuedAt.AddDays(duration.Value) : null;
var unsigned = new LicenseKeyPayload(
    Guid.NewGuid().ToString("N"),
    plan.Id,
    subject,
    string.IsNullOrWhiteSpace(fingerprint) ? null : fingerprint.Trim(),
    issuedAt,
    expires,
    string.Empty);

using var rsa = RSA.Create();
rsa.ImportFromPem(await File.ReadAllTextAsync(privateKeyPath));
var signature = rsa.SignData(
    RsaLicenseSignatureVerifier.BuildSignedData(unsigned),
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);
var signed = unsigned with { Signature = Convert.ToBase64String(signature) };
var json = JsonSerializer.Serialize(signed, new JsonSerializerOptions { WriteIndented = true });

if (string.IsNullOrWhiteSpace(output)) Console.WriteLine(json);
else
{
    await File.WriteAllTextAsync(output, json);
    Console.WriteLine($"License written to {output}");
}
return 0;
