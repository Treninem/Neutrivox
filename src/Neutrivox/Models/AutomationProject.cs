namespace Neutrivox.Models;

public sealed class AutomationProject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Новый проект";
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public ProjectMetadata Metadata { get; set; } = new();
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public List<ProjectDevice> Devices { get; } = [];
    public List<DeviceConnection> Connections { get; } = [];
    public List<ProjectTag> Tags { get; } = [];
    public LogicProgram Logic { get; set; } = new();
}

public sealed class ProjectDevice
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DefinitionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DeviceNetworkConfiguration Network { get; set; } = new();
    public PhysicalDeviceBinding? PhysicalBinding { get; set; }
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
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TagName { get; set; }
}

public sealed class ProjectTag
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TagDataType DataType { get; set; } = TagDataType.Boolean;
    public string? Description { get; set; }
    public object? InitialValue { get; set; }
}

public enum TagDataType { Boolean, Integer, Number, Text }

public sealed class DeviceNetworkConfiguration
{
    public string? IpAddress { get; set; }
    public string? Protocol { get; set; }
    public int? Port { get; set; }
    public string? SerialPort { get; set; }
    public int? BaudRate { get; set; }
}

public sealed class PhysicalDeviceBinding
{
    public string Endpoint { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string IdentificationState { get; set; } = "Unverified";
    public DateTime LastSeenUtc { get; set; }
}
