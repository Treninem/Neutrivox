using System.Security.Cryptography;
using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Produces a deterministic fingerprint for the exact project-to-device deployment plan.
/// The fingerprint is used to detect stale plans before a physical operation.
/// </summary>
public sealed class DeploymentPlanFingerprintService
{
    public string Compute(AutomationProject project, IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds.Distinct().OrderBy(x => x).ToList();
        var builder = new StringBuilder();
        builder.Append(project.Id.ToString("D"));
        builder.Append('|').Append(project.Name);

        foreach (var id in ids)
        {
            var device = project.Devices.FirstOrDefault(x => x.Id == id);
            if (device is null) continue;

            builder.Append("\ndevice|").Append(device.Id.ToString("D"));
            builder.Append('|').Append(device.Name);
            builder.Append('|').Append(device.DefinitionId);
            builder.Append('|').Append(device.PhysicalBinding?.Manufacturer ?? string.Empty);
            builder.Append('|').Append(device.PhysicalBinding?.Model ?? string.Empty);
            builder.Append('|').Append(device.PhysicalBinding?.Endpoint ?? string.Empty);

            foreach (var channel in device.Channels)
            {
                builder.Append("\nchannel|").Append(channel.Name);
                builder.Append('|').Append(channel.Type);
                builder.Append('|').Append(channel.Direction);
                builder.Append('|').Append(channel.Description ?? string.Empty);
            }
        }

        foreach (var network in project.Logic.Networks.Where(x => x.Enabled))
        {
            builder.Append("\nnetwork|").Append(network.Name);
            foreach (var instruction in network.Instructions)
            {
                builder.Append("\ninstruction|").Append(instruction.Kind);
                builder.Append('|').Append(instruction.Target);
                builder.Append('|').Append(instruction.SourceA);
                builder.Append('|').Append(instruction.SourceB);
                builder.Append('|').Append(instruction.Constant);
            }
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }
}
