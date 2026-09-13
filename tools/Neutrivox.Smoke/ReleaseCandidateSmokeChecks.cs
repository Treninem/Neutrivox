using System.Runtime.CompilerServices;
using Neutrivox.Models;
using Neutrivox.Services;

internal static class ReleaseCandidateSmokeChecks
{
    [ModuleInitializer]
    internal static void Run() => RunAsync().GetAwaiter().GetResult();

    private static async Task RunAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "Neutrivox-Smoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var trialPath = Path.Combine(root, "license.json");
            var license = new LocalLicenseService(trialPath);
            var start = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var trial = license.GetCurrent(start);
            Require(trial.Snapshot.Edition == ProductEdition.Professional && trial.Snapshot.State == LicenseState.Trial,
                "A new local installation must start in the 7-day Professional trial.");
            var expired = license.GetCurrent(start.AddDays(8));
            Require(expired.Snapshot.Edition == ProductEdition.Free && expired.Snapshot.State == LicenseState.Active,
                "An expired trial must fall back to the usable Free edition.");

            var settingsPath = Path.Combine(root, "settings.json");
            var settingsService = new ApplicationSettingsService(settingsPath);
            settingsService.Save(new ApplicationSettings { English = true, OwenReplicationUtilityPath = @"C:\Tools\Owen.exe" });
            var settings = settingsService.Load();
            Require(settings.English && settings.OwenReplicationUtilityPath == @"C:\Tools\Owen.exe",
                "Application settings did not survive a round-trip.");

            var recoveryPath = Path.Combine(root, "recovery.json");
            var recovery = new PersistentProjectRecoveryService(recoveryPath);
            var project = new AutomationProject { Name = "Recovery smoke" };
            await recovery.SaveAsync(project, @"C:\Projects\smoke.neutrivox");
            var restored = await recovery.LoadLatestAsync();
            Require(restored.Success && restored.Entry?.Project.Id == project.Id,
                "Persistent recovery did not restore the saved project identity.");

            var invalidPayload = new LicenseKeyPayload(
                "invalid", "professional-30d", "smoke", null, start, start.AddDays(30), Convert.ToBase64String([1, 2, 3]));
            Require(!new RsaLicenseSignatureVerifier().Verify(invalidPayload),
                "RSA verifier accepted an invalid signature.");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
