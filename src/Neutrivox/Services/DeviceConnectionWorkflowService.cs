using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record DeviceConnectionAttempt(
    bool Success,
    string Endpoint,
    string Message,
    DeviceIdentityReadResult? Identity = null);

/// <summary>
/// Coordinates a single explicit connection attempt and identity read.
/// It never scans arbitrary networks by itself and never writes configuration.
/// </summary>
public sealed class DeviceConnectionWorkflowService
{
    private readonly DeviceTransportFactory _factory;
    private readonly EndpointValidationService _endpointValidation = new();

    public DeviceConnectionWorkflowService(DeviceTransportFactory factory) => _factory = factory;

    public async Task<DeviceConnectionAttempt> ConnectAndIdentifyAsync(
        DeviceTransport transport,
        DeviceProtocolKind protocol,
        string endpoint,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var validation = transport == DeviceTransport.SerialRs485
            ? _endpointValidation.ValidateSerialEndpoint(endpoint)
            : _endpointValidation.ValidateNetworkEndpoint(endpoint);
        if (!validation.Valid)
            return new(false, endpoint, validation.Error ?? "Invalid endpoint.");

        var client = _factory.Create(transport, protocol);
        if (client is null)
            return new(false, validation.Normalized, $"No registered adapter supports {transport}/{protocol}.");

        await using (client)
        {
            var connection = await client.ConnectAsync(
                new DeviceConnectionRequest(validation.Normalized, protocol, TimeSpan.FromSeconds(5), parameters),
                cancellationToken);
            if (!connection.IsConnected)
                return new(false, validation.Normalized, connection.StatusMessage);

            var identity = await client.ReadIdentityAsync(cancellationToken);
            await client.DisconnectAsync(cancellationToken);
            return new(identity.Success, validation.Normalized, identity.Message, identity);
        }
    }
}
