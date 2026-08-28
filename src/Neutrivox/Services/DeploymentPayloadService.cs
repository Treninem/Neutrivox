using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeploymentPayload(
    Guid ProjectId,
    Guid DeviceId,
    string ProfileId,
    string DeviceName,
    string Format,
    byte[] Data,
    string Sha256,
    int InstructionCount);

/// <summary>
/// Creates a deterministic deployment payload from the project for adapters that have an officially
/// documented target format. It deliberately does not imply that this payload is acceptable to any device.
/// </summary>
public sealed class DeploymentPayloadService
{
    public DeploymentPayload BuildJsonPayload(AutomationProject project, ProjectDevice device, DeviceProfile profile)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"project={project.Id:D}");
        builder.AppendLine($"device={device.Id:D}");
        builder.AppendLine($"profile={profile.Id}");
        builder.AppendLine($"name={device.Name}");
        builder.AppendLine($"logic={project.Logic.Name}");
        foreach (var network in project.Logic.Networks.Where(x => x.Enabled))
        foreach (var instruction in network.Instructions)
            builder.AppendLine($"{network.Name}|{instruction.Kind}|{instruction.Target}|{instruction.SourceA}|{instruction.SourceB}|{instruction.Constant}");

        var data = Encoding.UTF8.GetBytes(builder.ToString());
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));
        return new(project.Id, device.Id, profile.Id, device.Name, "neutrivox-json-v1", data, hash, project.Logic.Networks.Sum(x => x.Instructions.Count));
    }
}
