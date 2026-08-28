namespace Neutrivox.Services;

public sealed record OwenReplicationTarget(string? IpAddress, string? PortName, string? Password);

/// <summary>Builds arguments for the documented OWEN Replication Utility command line.</summary>
public static class OwenReplicationCommandBuilder
{
    public static IReadOnlyList<string> Build(OwenReplicationTarget target, bool silent = true)
    {
        var args = new List<string>();
        if (silent) args.Add("/silent");
        if (!string.IsNullOrWhiteSpace(target.IpAddress)) args.Add($"/ip:{target.IpAddress.Trim()}");
        if (!string.IsNullOrWhiteSpace(target.PortName)) args.Add($"/portname:{target.PortName.Trim()}");
        if (!string.IsNullOrWhiteSpace(target.Password)) args.Add($"/password:{target.Password}");
        return args;
    }
}
