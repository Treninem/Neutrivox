using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeploymentReportService
{
    public string Build(DeploymentExecutionResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Neutrivox deployment report");
        sb.AppendLine($"Status: {(result.Success ? "SUCCESS" : result.UserCancelled ? "CANCELLED" : "NOT COMPLETE")}");
        sb.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
        sb.AppendLine();
        foreach (var item in result.Items.OrderBy(x => x.Order))
        {
            sb.AppendLine($"{item.Order}. {item.DeviceName} [{item.State}]");
            sb.AppendLine($"   {item.Message}");
            foreach (var step in item.Steps)
                sb.AppendLine($"   - {step.Step}: {(step.Success ? "OK" : "FAILED")} — {step.Message} ({step.Duration.TotalMilliseconds:0} ms)");
        }
        sb.AppendLine();
        sb.AppendLine(result.Summary);
        return sb.ToString();
    }
}
