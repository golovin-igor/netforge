using NetForge.Interfaces.Events;
using NetForge.Simulation.DataTypes.NetworkPrimitives;
using NetForge.Interfaces.Devices;

namespace NetForge.Interfaces.Devices
{

public interface INetwork
{
    INetworkEventBus EventBus { get; }

    /// <summary>
    /// Add a device to the network
    /// </summary>
    Task AddDeviceAsync(INetworkDevice device);

    /// <summary>
    /// Add a physical connection between two device interfaces
    /// </summary>
    Task AddPhysicalConnectionAsync(string device1Name, string interface1, string device2Name, string interface2,
        object connectionType = null);

    /// <summary>
    /// Add a link between two device interfaces (legacy method - creates physical connection)
    /// </summary>
    Task AddLinkAsync(string device1Name, string interface1, string device2Name, string interface2);

    /// <summary>
    /// Remove a physical connection between devices
    /// </summary>
    Task RemovePhysicalConnectionAsync(string device1Name, string interface1, string device2Name, string interface2);

    /// <summary>
    /// Remove a link between devices (legacy method)
    /// </summary>
    Task RemoveLinkAsync(string device1Name, string interface1, string device2Name, string interface2);

    /// <summary>
    /// Get a physical connection by device and interface names
    /// </summary>
    object? GetPhysicalConnection(string device1Name, string interface1, string device2Name, string interface2);

    /// <summary>
    /// Get all physical connections for a specific device interface
    /// </summary>
    List<object> GetPhysicalConnectionsForInterface(string deviceName, string interfaceName);

    /// <summary>
    /// Get all physical connections in the network
    /// </summary>
    IEnumerable<object> GetAllPhysicalConnections();

    /// <summary>
    /// Simulate cable failure on a specific connection
    /// </summary>
    Task SimulateCableFailureAsync(string device1Name, string interface1, string device2Name, string interface2, string reason = "Cable failure");

    /// <summary>
    /// Simulate connection degradation
    /// </summary>
    Task SimulateConnectionDegradationAsync(string device1Name, string interface1, string device2Name, string interface2,
        double packetLoss, int additionalLatency, string reason = "Signal degradation");

    /// <summary>
    /// Restore a failed or degraded connection
    /// </summary>
    Task RestoreConnectionAsync(string device1Name, string interface1, string device2Name, string interface2);

    /// <summary>
    /// Get device by name
    /// </summary>
    INetworkDevice GetDevice(string name);

    /// <summary>
    /// Remove a device from the network
    /// </summary>
    Task RemoveDeviceAsync(string deviceName);

    /// <summary>
    /// Remove a device from the network by device ID
    /// </summary>
    Task RemoveDeviceByIdAsync(string deviceId);

    /// <summary>
    /// Update device ID mapping (useful when a device's DeviceId changes after being added)
    /// </summary>
    void UpdateDeviceIdMapping(INetworkDevice device, string? oldDeviceId = null);

    /// <summary>
    /// Check if a device with the specified ID exists in the network
    /// </summary>
    bool ContainsDeviceId(string deviceId);

    /// <summary>
    /// Get device by device ID
    /// </summary>
    INetworkDevice? GetDeviceById(string deviceId);

    /// <summary>
    /// Find device by IP address
    /// </summary>
    INetworkDevice FindDeviceByIp(string ip);

    /// <summary>
    /// Get all devices in the network
    /// </summary>
    IEnumerable<INetworkDevice> GetAllDevices();

    /// <summary>
    /// Get devices connected to a specific device interface
    /// </summary>
    List<(INetworkDevice device, string interfaceName)> GetConnectedDevices(string deviceName, string interfaceName);

    /// <summary>
    /// Check if two interfaces are connected via operational physical connection
    /// </summary>
    bool AreConnected(string device1Name, string interface1, string device2Name, string interface2);

    /// <summary>
    /// Check if an interface has any operational physical connections
    /// </summary>
    bool IsInterfaceConnected(string deviceName, string interfaceName);

    /// <summary>
    /// Get network statistics for monitoring
    /// </summary>
    NetworkStatistics GetNetworkStatistics();

    /// <summary>
    /// Update all protocol states (legacy method - protocols now update via events)
    /// </summary>
    void UpdateProtocols();
}
}
