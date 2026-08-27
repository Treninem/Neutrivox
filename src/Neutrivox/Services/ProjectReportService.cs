using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectReportService
{
    private readonly ProjectValidationWorkflowService _validation = new();

    public string CreateTextReport(AutomationProject project)
    {
        var result = _validation.Validate(project);
        var sb = new StringBuilder();
        sb.AppendLine($"Project: {project.Name}");
        sb.AppendLine($"Created: {project.CreatedAtUtc:O}");
        sb.AppendLine($"Devices: {project.Devices.Count}");
        sb.AppendLine($"Connections: {project.Connections.Count}");
        sb.AppendLine($"Tags: {project.Tags.Count}");
        sb.AppendLine($"Logic networks: {project.Logic.Networks.Count}");
        sb.AppendLine($"Logic instructions: {project.Logic.Networks.Sum(x => x.Instructions.Count)}");
        sb.AppendLine();
        sb.AppendLine("Validation:");
        foreach (var item in result.Items) sb.AppendLine($"[{item.Level}] {item.Title}: {item.Description}");
        return sb.ToString();
    }
}
