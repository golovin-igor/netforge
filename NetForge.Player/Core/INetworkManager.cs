// TODO: Phase 1.2 - Implement Network Management System
// This interface defines network management functionality for device lifecycle and topology

using NetForge.Simulation.Topology.Devices;

namespace NetForge.Player.Core;

/// <summary>
/// Core interface for network management in NetForge.Player
/// </summary>
public interface INetworkManager
{
    // TODO: Implement device management methods
    // - CreateDeviceAsync: Create devices with vendor-specific configurations
    // - DeleteDeviceAsync: Remove devices and cleanup resources
    // - GetDeviceAsync: Retrieve device by hostname or ID
    // - GetAllDevicesAsync: List all devices in network
    // - CloneDeviceAsync: Create device copies with modified configurations
    // - BulkCreateDevicesAsync: Create multiple devices from templates

    /// <summary>
    /// Create a new network device
    /// </summary>
    /// <param name="vendor">Vendor type (cisco, juniper, etc.)</param>
    /// <param name="hostname">Device hostname</param>
    /// <param name="deviceType">Device type (router, switch, etc.)</param>
    /// <returns>Created network device</returns>
    Task<NetworkDevice> CreateDeviceAsync(string vendor, string hostname, string? deviceType = null);

    /// <summary>
    /// Delete a network device
    /// </summary>
    /// <param name="hostname">Device hostname</param>
    /// <returns>True if device was deleted</returns>
    Task<bool> DeleteDeviceAsync(string hostname);

    /// <summary>
    /// Get device by hostname
    /// </summary>
    /// <param name="hostname">Device hostname</param>
    /// <returns>Network device or null if not found</returns>
    Task<NetworkDevice?> GetDeviceAsync(string hostname);

    /// <summary>
    /// Get all devices in the network
    /// </summary>
    /// <returns>List of all network devices</returns>
    Task<IEnumerable<NetworkDevice>> GetAllDevicesAsync();

    // TODO: Implement topology management methods
    // - CreateLinkAsync: Create physical links between devices
    // - DeleteLinkAsync: Remove links and update topology
    // - GetTopologyAsync: Retrieve current network topology
    // - ValidateTopologyAsync: Check topology consistency
    // - OptimizeTopologyAsync: Suggest topology improvements

    /// <summary>
    /// Create a link between two devices
    /// </summary>
    /// <param name="device1">First device hostname</param>
    /// <param name="interface1">First device interface</param>
    /// <param name="device2">Second device hostname</param>
    /// <param name="interface2">Second device interface</param>
    /// <returns>True if link was created</returns>
    Task<bool> CreateLinkAsync(string device1, string interface1, string device2, string interface2);

    /// <summary>
    /// Remove a link from a device interface
    /// </summary>
    /// <param name="deviceName">Device hostname</param>
    /// <param name="interfaceName">Interface name</param>
    /// <returns>True if link was removed</returns>
    Task<bool> DeleteLinkAsync(string deviceName, string interfaceName);

    // TODO: Implement network state management
    // - SaveNetworkStateAsync: Persist current network configuration
    // - LoadNetworkStateAsync: Restore network from saved state
    // - ResetNetworkAsync: Clear all devices and topology
    // - GetNetworkStatisticsAsync: Provide network performance metrics
    // - ValidateNetworkAsync: Check network health and consistency

    /// <summary>
    /// Get current network topology
    /// </summary>
    /// <returns>Network topology information</returns>
    Task<NetworkTopology> GetTopologyAsync();

    // TODO: Implement protocol management
    // - StartProtocolsAsync: Initialize and start network protocols
    // - StopProtocolsAsync: Gracefully shutdown protocols
    // - UpdateProtocolsAsync: Force protocol convergence
    // - GetProtocolStatusAsync: Check protocol operational status
    // - ConfigureProtocolAsync: Modify protocol settings

    /// <summary>
    /// Update network protocols and force convergence
    /// </summary>
    /// <returns>True if protocols updated successfully</returns>
    Task<bool> UpdateProtocolsAsync();
}

