using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Neutrivox.Services;

/// <summary>
/// Produces a stable, non-secret local fingerprint used for license binding.
/// The fingerprint contains only a SHA-256 digest; raw machine/user values are never persisted.
/// </summary>
public sealed class DeviceFingerprintService
{
    public string GetFingerprint()
    {
        var raw = string.Join('|',
            Environment.MachineName,
            Environment.UserDomainName,
            Environment.UserName,
            RuntimeInformation.OSArchitecture,
            "Neutrivox-License-Fingerprint-v1");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
    }
}
