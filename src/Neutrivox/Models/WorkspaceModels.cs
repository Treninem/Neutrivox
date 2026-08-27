namespace Neutrivox.Models;

public sealed class ProjectWorkspace
{
    public Dictionary<Guid, WorkspaceDevicePosition> DevicePositions { get; } = [];
    public double Zoom { get; set; } = 1;
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
}

public sealed record WorkspaceDevicePosition(double X, double Y);

public sealed class ProjectSelection
{
    public Guid? DeviceId { get; private set; }
    public Guid? ChannelId { get; private set; }
    public Guid? ConnectionFromDeviceId { get; private set; }
    public Guid? ConnectionToDeviceId { get; private set; }

    public void SelectDevice(Guid id) { DeviceId = id; ChannelId = null; ConnectionFromDeviceId = null; ConnectionToDeviceId = null; }
    public void SelectChannel(Guid id) { ChannelId = id; DeviceId = null; ConnectionFromDeviceId = null; ConnectionToDeviceId = null; }
    public void SelectConnection(Guid from, Guid to) { ConnectionFromDeviceId = from; ConnectionToDeviceId = to; DeviceId = null; ChannelId = null; }
    public void Clear() { DeviceId = null; ChannelId = null; ConnectionFromDeviceId = null; ConnectionToDeviceId = null; }
}
