using Microsoft.Win32;

namespace Neutrivox.Services;

/// <summary>
/// Keeps a redundant first-run timestamp in HKCU so deleting only the JSON state file does not restart the trial.
/// This is defense-in-depth for the offline trial; authoritative anti-abuse still belongs on a licensing backend.
/// </summary>
public sealed class TrialAnchorService
{
    private const string RegistryPath = @"Software\Neutrivox";
    private const string StartedValue = "TrialStartedUtc";

    public DateTimeOffset? ReadStartedAtUtc()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
            var value = key?.GetValue(StartedValue) as string;
            return DateTimeOffset.TryParse(value, out var parsed) ? parsed.ToUniversalTime() : null;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
        {
            _ = ex;
            return null;
        }
    }

    public void WriteEarliest(DateTimeOffset startedAtUtc)
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);
            var existingText = key?.GetValue(StartedValue) as string;
            var effective = DateTimeOffset.TryParse(existingText, out var existing)
                ? (existing <= startedAtUtc ? existing : startedAtUtc)
                : startedAtUtc;
            key?.SetValue(StartedValue, effective.ToUniversalTime().ToString("O"), RegistryValueKind.String);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
        {
            _ = ex;
        }
    }
}
