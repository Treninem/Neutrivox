namespace Neutrivox.Models;

public sealed class AutomationProject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Новый проект";
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public List<ProjectDevice> Devices { get; } = [];
    public List<DeviceConnection> Connections { get; } = [];
}

public sealed class ProjectDevice
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DefinitionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<IoChannel> Channels { get; } = [];
}

public sealed class DeviceConnection
{
    public Guid FromDeviceId { get; init; }
    public Guid ToDeviceId { get; init; }
    public string Interface { get; init; } = string.Empty;
}

public sealed class IoChannel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string? Description { get; set; }
}
