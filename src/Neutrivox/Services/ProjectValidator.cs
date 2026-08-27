using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record ValidationMessage(string Code, string Message, ValidationSeverity Severity);

public enum ValidationSeverity { Info, Warning, Error }

public sealed class ProjectValidator
{
    public IReadOnlyList<ValidationMessage> Validate(AutomationProject project)
    {
        var messages = new List<ValidationMessage>();
        if (string.IsNullOrWhiteSpace(project.Name))
            messages.Add(new("PROJECT_NAME", "Project name is required.", ValidationSeverity.Error));
        if (project.Devices.Count == 0)
            messages.Add(new("NO_DEVICES", "No equipment has been added to the project.", ValidationSeverity.Warning));

        var duplicateNames = project.Devices
            .Where(d => !string.IsNullOrWhiteSpace(d.Name))
            .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);
        foreach (var duplicate in duplicateNames)
            messages.Add(new("DUPLICATE_DEVICE_NAME", $"Duplicate device name: {duplicate.Key}", ValidationSeverity.Error));

        return messages;
    }
}
