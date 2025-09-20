using NetForge.Simulation.Common.Common;
using NetForge.Simulation.DataTypes.NetworkPrimitives;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Manages physical layer connectivity and connection quality for network devices.
/// This interface handles physical connection state, metrics, and testing.
/// </summary>
public interface IPhysicalConnectivity
{
    /// <summary>
    /// Gets physical connections for a specific interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns>List of physical connections for the interface.</returns>
    List<PhysicalConnection> GetPhysicalConnectionsForInterface(string interfaceName);

    /// <summary>
    /// Gets all operational physical connections for this device.
    /// </summary>
    /// <returns>List of operational physical connections.</returns>
    List<PhysicalConnection> GetOperationalPhysicalConnections();

    /// <summary>
    /// Checks if an interface has operational physical connectivity.
    /// </summary>
    /// <param name="interfaceName">The interface name to check.</param>
    /// <returns>True if the interface is physically connected, false otherwise.</returns>
    bool IsInterfacePhysicallyConnected(string interfaceName);

    /// <summary>
    /// Gets physical connection quality metrics for an interface.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns>Connection metrics, or null if not connected.</returns>
    PhysicalConnectionMetrics? GetPhysicalConnectionMetrics(string interfaceName);

    /// <summary>
    /// Tests physical connectivity by simulating packet transmission.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <param name="packetSize">The size of the test packet (default 1500 bytes).</param>
    /// <returns>The transmission result.</returns>
    PhysicalTransmissionResult TestPhysicalConnectivity(string interfaceName, int packetSize = 1500);

    /// <summary>
    /// Gets the remote device and interface connected to a local interface.
    /// </summary>
    /// <param name="localInterfaceName">The local interface name.</param>
    /// <returns>A tuple of remote device and interface name, or null if not connected.</returns>
    (INetworkDevice device, string interfaceName)? GetConnectedDevice(string localInterfaceName);

    /// <summary>
    /// Checks if protocols should consider this interface for routing/switching decisions.
    /// This method respects physical connectivity - protocols should only use interfaces
    /// that have operational physical connections.
    /// </summary>
    /// <param name="interfaceName">The interface name to check.</param>
    /// <returns>True if the interface should participate in protocols, false otherwise.</returns>
    bool ShouldInterfaceParticipateInProtocols(string interfaceName);
}