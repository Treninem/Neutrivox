namespace Neutrivox.Services;

public sealed record DiscoverySafetyResult(bool Allowed, string NormalizedScope, string? Error);

/// <summary>Applies conservative limits to automatic discovery so the application only scans explicitly requested, bounded scopes.</summary>
public sealed class DiscoverySafetyService
{
    public DiscoverySafetyResult Check(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope)) return new(false, string.Empty, "Укажите IP-адрес или подсеть для обнаружения.");
        if (!NetworkScopeParser.TryParse(scope.Trim(), out var addresses))
            return new(false, scope.Trim(), "Недопустимый или слишком большой диапазон обнаружения.");
        if (addresses.Count > 4096)
            return new(false, scope.Trim(), "Диапазон обнаружения ограничен 4096 адресами за один поиск.");
        return new(true, scope.Trim(), null);
    }
}
