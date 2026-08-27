using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class ProjectSummaryService
{
    public string CreateHumanReadableSummary(AutomationProject project)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Project: {project.Name}");
        sb.AppendLine($"Devices: {project.Devices.Count}");
        sb.AppendLine($"Connections: {project.Connections.Count}");
        sb.AppendLine($"Tags: {project.Tags.Count}");
        sb.AppendLine($"Logic networks: {project.Logic.Networks.Count}");
        sb.AppendLine();
        sb.AppendLine("Devices:");
        foreach (var device in project.Devices)
        {
            var endpoint = device.PhysicalBinding?.Endpoint ?? device.Network.IpAddress ?? device.Network.SerialPort ?? "digital model";
            sb.AppendLine($"- {device.Name} [{device.DefinitionId}] — {endpoint}");
        }
        return sb.ToString();
    }
}
