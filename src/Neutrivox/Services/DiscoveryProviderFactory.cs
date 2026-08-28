namespace Neutrivox.Services;

/// <summary>Builds the default discovery stack. Device identification remains a separate verified-profile step.</summary>
public static class DiscoveryProviderFactory
{
    public static DeviceDiscoveryService CreateDefault()
    {
        var service = new DeviceDiscoveryService();
        service.Register(new EthernetEndpointDiscoveryProvider());
        service.Register(new SerialPortInventoryDiscoveryProvider());
        return service;
    }
}
