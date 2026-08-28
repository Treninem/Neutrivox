using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectReportService
{
    private readonly ProjectValidationWorkflowService _validation = new();

    public string CreateTextReport(AutomationProject project)
    {
        var result = _validation.ValidateForSimulation(project);
        var sb = new StringBuilder();
        sb.AppendLine($"Project: {project.Name}");
        sb.AppendLine($"Created: {project.CreatedAtUtc:O}");
        sb.AppendLine($"Project ID: {project.Id}");
        sb.AppendLine();
        sb.AppendLine("Equipment:");
        foreach (var device in project.Devices)
            sb.AppendLine($"- {device.Name} [{device.DefinitionId}] channels={device.Channels.Count}");
        sb.AppendLine();
        sb.AppendLine("Connections:");
        foreach (var connection in project.Connections)
            sb.AppendLine($"- {connection.FromDeviceId} -> {connection.ToDeviceId} via {connection.Interface}");
        sb.AppendLine();
        sb.AppendLine($"Tags: {project.Tags.Count}");
        sb.AppendLine($"Logic networks: {project.Logic.Networks.Count}");
        sb.AppendLine($"Logic instructions: {project.Logic.Networks.Sum(x => x.Instructions.Count)}");
        sb.AppendLine();
        sb.AppendLine($"Simulation readiness: {(result.IsReadyForSimulation ? "Ready" : "Blocked")}");
        foreach (var item in result.Issues)
            sb.AppendLine($"[{item.Severity}] {item.Area}: {item.Message}");
        return sb.ToString();
    }
}