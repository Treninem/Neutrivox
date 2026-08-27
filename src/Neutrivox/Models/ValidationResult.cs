namespace Neutrivox.Models;

public enum ValidationSeverity { Info, Warning, Error }

public sealed record ValidationIssue(
    ValidationSeverity Severity,
    string Code,
    string Message,
    Guid? DeviceId = null);

public sealed class ProjectValidationResult
{
    public List<ValidationIssue> Issues { get; } = [];
    public bool IsValid => Issues.All(x => x.Severity != ValidationSeverity.Error);
    public int ErrorCount => Issues.Count(x => x.Severity == ValidationSeverity.Error);
    public int WarningCount => Issues.Count(x => x.Severity == ValidationSeverity.Warning);
}
