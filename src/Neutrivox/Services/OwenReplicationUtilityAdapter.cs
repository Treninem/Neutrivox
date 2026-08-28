using System.Diagnostics;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Production integration point for the official Owen Logic Replication Utility.
/// Neutrivox does not ship, reverse engineer, or replace the vendor utility; it prepares
/// a verified target and invokes the user-supplied official utility with documented arguments.
/// </summary>
public sealed class OwenReplicationUtilityAdapter : IDeviceDeploymentAdapter
{
    public const string AdapterIdValue = "owen.replication-utility.windows";
    public string AdapterId => AdapterIdValue;

    public bool Supports(DeviceProfile profile) =>
        profile.Id.StartsWith("owen.pr", StringComparison.OrdinalIgnoreCase) &&
        profile.SupportLevel == DeviceSupportLevel.ReadWriteSupported;

    public async Task<IReadOnlyList<DeploymentStepResult>> ExecuteAsync(
        DeploymentContext context,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DeploymentStepResult>();
        var stopwatch = Stopwatch.StartNew();
        if (!context.UserConfirmed)
        {
            results.Add(new(false, "Confirmation", "Physical deployment was not explicitly confirmed.", stopwatch.Elapsed, false));
            return results;
        }

        if (!context.Parameters.TryGetValue("utilityPath", out var value) || value is not string utilityPath || string.IsNullOrWhiteSpace(utilityPath))
        {
            results.Add(new(false, "Utility", "Official Owen Replication Utility path was not configured.", stopwatch.Elapsed, false));
            return results;
        }

        if (!File.Exists(utilityPath))
        {
            results.Add(new(false, "Utility", $"Replication Utility was not found: {utilityPath}", stopwatch.Elapsed, false));
            return results;
        }

        var endpoint = context.Endpoint.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            results.Add(new(false, "Endpoint", "Deployment endpoint is empty.", stopwatch.Elapsed, false));
            return results;
        }

        var args = BuildArguments(endpoint, context.Parameters);
        results.Add(new(true, "Preflight", $"Official utility selected for {context.Target.Name}.", stopwatch.Elapsed, false));

        var psi = new ProcessStartInfo
        {
            FileName = utilityPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        try
        {
            if (!process.Start())
            {
                results.Add(new(false, "Launch", "The official replication utility could not be started.", stopwatch.Elapsed, false));
                return results;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var output = await outputTask;
            var error = await errorTask;
            var message = string.IsNullOrWhiteSpace(error)
                ? $"Official utility exited with code {process.ExitCode}. {TrimOutput(output)}"
                : $"Official utility exited with code {process.ExitCode}. {TrimOutput(error)}";

            results.Add(new(
                process.ExitCode == 0,
                "Replication Utility",
                message,
                stopwatch.Elapsed,
                process.ExitCode == 0));
            return results;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best effort */ }
            }
            results.Add(new(false, "Cancelled", "Deployment was cancelled and the utility was stopped when possible.", stopwatch.Elapsed, false));
            return results;
        }
        catch (Exception ex)
        {
            results.Add(new(false, "Launch", $"Failed to execute official utility: {ex.Message}", stopwatch.Elapsed, false));
            return results;
        }
    }

    private static string BuildArguments(string endpoint, IReadOnlyDictionary<string, object?> parameters)
    {
        var args = "/silent";
        if (endpoint.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            args += $" /portname:{endpoint}";
        else if (Uri.TryCreate("http://" + endpoint, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
            args += $" /ip:{uri.Host}";
        else
            throw new ArgumentException("Endpoint must be a COM port or an IPv4/hostname endpoint.", nameof(endpoint));

        if (parameters.TryGetValue("password", out var password) && password is string secret && !string.IsNullOrWhiteSpace(secret))
            args += " /password:" + Quote(secret);
        return args;
    }

    private static string Quote(string value) => value.Contains(' ') ? $"\"{value.Replace("\"", "\\\"")}\"" : value;
    private static string TrimOutput(string value) => value.Length <= 1500 ? value.Trim() : value.Trim()[^1500..];
}
