using System.Security.Cryptography;
using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>
/// Produces a deterministic fingerprint for the exact project-to-device deployment plan.
/// The supplied device order is significant and is preserved intentionally.
/// </summary>
public sealed class DeploymentPlanFingerprintService
{
    public string Compute(AutomationProject project, IEnumerable<Guid> deviceIds)
    {
        var ids = deviceIds.Distinct().ToList();
        var builder = new StringBuilder();
        builder.Append(project.Id.ToString("D"));
        builder.Append('|').Append(project.Name);

        for (var index = 0; index < ids.Count; index++)
        {
            var id = ids[index];
            var device = project.Devices.FirstOrDefault(x => x.Id == id);
            if (device is null) continue;

            builder.Append("\ndeployment-order|").Append(index + 1);
            builder.Append("\ndevice|").Append(device.Id.ToString("D"));
            builder.Append('|').Append(device.Name);
            builder.Append('|').Append(device.DefinitionId);
            builder.Append('|').Append(device.PhysicalBinding?.Manufacturer ?? string.Empty);
            builder.Append('|').Append(device.PhysicalBinding?.Model ?? string.Empty);
            builder.Append('|').Append(device.PhysicalBinding?.Endpoint ?? string.Empty);
            builder.Append('|').Append(device.PhysicalBinding?.IdentificationState ?? string.Empty);

            foreach (var channel in device.Channels)
            {
                builder.Append("\nchannel|").Append(channel.Id.ToString("D"));
                builder.Append('|').Append(channel.Name);
                builder.Append('|').Append(channel.Type);
                builder.Append('|').Append(channel.Direction);
                builder.Append('|').Append(channel.Description ?? string.Empty);
                builder.Append('|').Append(channel.TagName ?? string.Empty);
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
