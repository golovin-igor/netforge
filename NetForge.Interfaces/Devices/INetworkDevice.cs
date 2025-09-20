namespace NetForge.Interfaces.Devices
{

/// <summary>
/// Composite interface for network devices that inherits from all segregated interfaces.
/// This interface maintains backward compatibility while following the Interface Segregation Principle.
/// Implementations should consider using the segregated interfaces directly for better separation of concerns.
/// </summary>
public interface INetworkDevice :
    IDeviceIdentity,
    IConfigurationProvider,
    INetworkConnectivity,
    ICommandProcessor,
    IProtocolHost,
    IInterfaceManager,
    IPhysicalConnectivity,
    IDeviceLogging,
    INetworkContext
    {

    }
}
