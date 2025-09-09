using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Security;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Manages network interfaces, VLANs, port channels, and access lists for network devices.
/// This interface handles all interface-related configuration and management.
/// </summary>
public interface IInterfaceManager
{
    /// <summary>
    /// Gets all interfaces configured on the device.
    /// </summary>
    Dictionary<string, IInterfaceConfig> GetAllInterfaces();

    /// <summary>
    /// Gets an interface by name.
    /// </summary>
    /// <param name="name">The interface name.</param>
    /// <returns>The interface configuration, or null if not found.</returns>
    IInterfaceConfig? GetInterface(string name);

    /// <summary>
    /// Gets all VLANs configured on the device.
    /// </summary>
    Dictionary<int, VlanConfig> GetAllVlans();

    /// <summary>
    /// Gets a VLAN by ID.
    /// </summary>
    /// <param name="id">The VLAN ID.</param>
    /// <returns>The VLAN configuration, or null if not found.</returns>
    VlanConfig? GetVlan(int id);

    /// <summary>
    /// Gets all port channels configured on the device.
    /// </summary>
    Dictionary<int, PortChannel> GetPortChannels();

    /// <summary>
    /// Gets a port channel by number.
    /// </summary>
    /// <param name="number">The port channel number.</param>
    /// <returns>The port channel configuration, or null if not found.</returns>
    PortChannel? GetPortChannel(int number);

    /// <summary>
    /// Gets all access lists configured on the device.
    /// </summary>
    Dictionary<int, AccessList> GetAccessLists();

    /// <summary>
    /// Gets an access list by number.
    /// </summary>
    /// <param name="number">The access list number.</param>
    /// <returns>The access list configuration, or null if not found.</returns>
    AccessList? GetAccessList(int number);
}