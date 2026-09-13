namespace Neutrivox.Models;

public sealed class ElectricalSchematic
{
    public List<SchematicElement> Elements { get; set; } = [];
    public List<SchematicWire> Wires { get; set; } = [];
}

public enum SchematicElementKind
{
    Controller,
    PowerSupply24V,
    Relay,
    Contactor,
    Lamp,
    Sensor,
    PushButton,
    Motor230V,
    Fuse,
    Terminal
}

public enum SchematicFault
{
    None,
    OpenCircuit,
    ShortCircuit,
    StuckOff,
    StuckOn
}

public sealed class SchematicElement
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public SchematicElementKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ProjectDeviceId { get; set; }
    public Guid? BoundChannelId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public bool SimulatedState { get; set; }
    public SchematicFault Fault { get; set; }
    public List<SchematicPin> Pins { get; set; } = [];
}

public sealed record SchematicPin(string Name, string Role);

public sealed class SchematicWire
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid FromElementId { get; init; }
    public string FromPin { get; init; } = string.Empty;
    public Guid ToElementId { get; init; }
    public string ToPin { get; init; } = string.Empty;
    public string? Label { get; set; }
}
