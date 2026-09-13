using System.Security.Cryptography;
using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Verifies Neutrivox license payloads with the public signing key.
/// The private key is deliberately not part of the application or repository.
/// </summary>
public sealed class RsaLicenseSignatureVerifier : ILicenseSignatureVerifier
{
    private const string PublicKeyPem = """
-----BEGIN PUBLIC KEY-----
MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAybU+FKQEaVG2I60JBEBj
pRuKIoR5nlv9MoYwDgvPP4vsgXiUZoBNGSno/iI15ukzXyXtILPefvGEWlh1Ms2J
7OYfaax9qelHF0zx+8yxXbuyKvcVywxrzVTXyp/Grn2TrW38BvqJMhu7eJRxQIY+
vXnNsQAtV9VowRFCQnlhdMQzo4Qzy0RFsWcy3CKVwgHGeM8xSwFfb++1Lm5msdjn
xm3p4BP+o6U276KsN3qoiFDV+5OAFYuptYX6u5fVSj/zMY0PeXWHua7oZ67B7tH+
Gkip+sgAYRlUTr2kIxBx1Q5VsZ3cBYKjqXgI+DadWr2HPckeF22Miw8+NCZyV/ih
KpiEW/XreD9Fz+cfCdEmL73vWzW7qMa1xMXQt/sCEVr1evISpBr2fnBdglM4dX5V
NYm9ltZzWP8YMJfhbwVN+X2N3jWFRcDgRGLS55YfL65Iczd3sEVwa9cWIdgcUvpB
q0eTz1nxpt+ogNcfJdvXNzn9XwElvai+hWjw1izKynl7AgMBAAE=
-----END PUBLIC KEY-----
""";

    public bool Verify(LicenseKeyPayload payload)
    {
        try
        {
            var signature = Convert.FromBase64String(payload.Signature);
            using var rsa = RSA.Create();
            rsa.ImportFromPem(PublicKeyPem);
            return rsa.VerifyData(
                BuildSignedData(payload),
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
        catch (FormatException) { return false; }
        catch (CryptographicException) { return false; }
    }

    public static byte[] BuildSignedData(LicenseKeyPayload payload)
    {
        var text = string.Join('\n',
            payload.KeyId,
            payload.PlanId,
            payload.Subject,
            payload.BoundDeviceFingerprint ?? string.Empty,
            payload.IssuedAtUtc.ToUniversalTime().ToString("O"),
            payload.ExpiresAtUtc?.ToUniversalTime().ToString("O") ?? string.Empty);
        return Encoding.UTF8.GetBytes(text);
    }
}
