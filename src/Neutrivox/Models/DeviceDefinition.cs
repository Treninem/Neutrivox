namespace Neutrivox.Models;

public sealed record DeviceDefinition(
    string Id,
    string Manufacturer,
    string Model,
    DeviceCategory Category,
    IReadOnlyList<ChannelDefinition> Channels,
    IReadOnlyList<string> Interfaces);

public sealed record ChannelDefinition(string Name, string Type, string Direction);

public enum DeviceCategory
{
    Controller,
    ExpansionModule,
    CommunicationModule
}
