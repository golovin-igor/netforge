using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Topology.Services;

/// <summary>
/// Manages physical layer connectivity and connection quality for network devices.
/// This service provides default implementations for physical connectivity operations.
/// </summary>
public class DevicePhysicalConnectivityService : IPhysicalConnectivity
{
    private readonly INetworkDevice _device;
    private readonly Dictionary<string, List<PhysicalConnection>> _interfaceConnections = new();

    public DevicePhysicalConnectivityService(INetworkDevice device)
    {
        _device = device ?? throw new ArgumentNullException(nameof(device));
    }

    /// <summary>
    /// Gets physical connections for a specific interface.
    /// </summary>
    public List<PhysicalConnection> GetPhysicalConnectionsForInterface(string interfaceName)
    {
        return _interfaceConnections.TryGetValue(interfaceName, out var connections) 
            ? new List<PhysicalConnection>(connections) 
            : [];
    }

    /// <summary>
    /// Gets all operational physical connections for this device.
    /// </summary>
    public List<PhysicalConnection> GetOperationalPhysicalConnections()
    {
        var allConnections = new List<PhysicalConnection>();
        foreach (var connections in _interfaceConnections.Values)
        {
            allConnections.AddRange(connections.Where(c => c.IsOperational));
        }
        return allConnections;
    }

    /// <summary>
    /// Checks if an interface has operational physical connectivity.
    /// </summary>
    public bool IsInterfacePhysicallyConnected(string interfaceName)
    {
        return _interfaceConnections.TryGetValue(interfaceName, out var connections) 
            && connections.Any(c => c.IsOperational);
    }

    /// <summary>
    /// Gets physical connection quality metrics for an interface.
    /// </summary>
    public PhysicalConnectionMetrics? GetPhysicalConnectionMetrics(string interfaceName)
    {
        if (!_interfaceConnections.TryGetValue(interfaceName, out var connections))
            return null;

        var operationalConnection = connections.FirstOrDefault(c => c.IsOperational);
        // Return null for now - would need to implement metrics based on connection properties
        return null;
    }

    /// <summary>
    /// Tests physical connectivity by simulating packet transmission.
    /// </summary>
    public PhysicalTransmissionResult TestPhysicalConnectivity(string interfaceName, int packetSize = 1500)
    {
        if (!IsInterfacePhysicallyConnected(interfaceName))
        {
            return new PhysicalTransmissionResult
            {
                Success = false,
                Reason = "Interface not physically connected"
            };
        }

        // Simulate transmission
        return new PhysicalTransmissionResult
        {
            Success = true,
            TransmissionTime = Random.Shared.Next(1, 10) / 1000.0,
            ActualLatency = Random.Shared.Next(1, 10)
        };
    }

    /// <summary>
    /// Gets the remote device and interface connected to a local interface.
    /// </summary>
    public (INetworkDevice device, string interfaceName)? GetConnectedDevice(string localInterfaceName)
    {
        if (!_interfaceConnections.TryGetValue(localInterfaceName, out var connections))
            return null;

        var operationalConnection = connections.FirstOrDefault(c => c.IsOperational);
        if (operationalConnection == null)
            return null;

        // This would need to be implemented based on the actual PhysicalConnection structure
        // For now, return null as a placeholder
        return null;
    }

    /// <summary>
    /// Checks if protocols should consider this interface for routing/switching decisions.
    /// </summary>
    public bool ShouldInterfaceParticipateInProtocols(string interfaceName)
    {
        // Only participate if physically connected and operationally up
        return IsInterfacePhysicallyConnected(interfaceName) && IsInterfaceOperationallyUp(interfaceName);
    }

    /// <summary>
    /// Adds a physical connection for an interface.
    /// </summary>
    public void AddPhysicalConnection(string interfaceName, PhysicalConnection connection)
    {
        if (!_interfaceConnections.ContainsKey(interfaceName))
        {
            _interfaceConnections[interfaceName] = [];
        }
        _interfaceConnections[interfaceName].Add(connection);
    }

    /// <summary>
    /// Removes a physical connection for an interface.
    /// </summary>
    public bool RemovePhysicalConnection(string interfaceName, PhysicalConnection connection)
    {
        return _interfaceConnections.TryGetValue(interfaceName, out var connections) 
            && connections.Remove(connection);
    }

    /// <summary>
    /// Helper method to check if an interface is operationally up.
    /// </summary>
    private bool IsInterfaceOperationallyUp(string interfaceName)
    {
        var interfaceConfig = _device.GetInterface(interfaceName);
        return interfaceConfig?.IsUp ?? false;
    }
}