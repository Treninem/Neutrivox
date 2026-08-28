namespace Neutrivox.Services;

public sealed record DiscoverySafetyDecision(bool Allowed, string Message);

/// <summary>Limits discovery to explicitly requested scopes and keeps identification separate from binding.</summary>
public sealed class DiscoverySafetyPolicy
{
    public DiscoverySafetyDecision Check(DiscoveryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NetworkScope))
            return new(false, "A discovery scope must be explicitly provided.");
        if (!request.IncludeEthernet && !request.IncludeSerial)
            return new(false, "At least one discovery transport must be enabled.");
        return new(true, "Discovery scope is explicitly configured.");
    }
}
