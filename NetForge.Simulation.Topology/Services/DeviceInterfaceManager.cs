using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Security;

namespace NetForge.Simulation.Topology.Services;

/// <summary>
/// Manages network interfaces, VLANs, port channels, and access lists for network devices.
/// This service extracts interface management responsibilities from NetworkDevice.
/// </summary>
public class DeviceInterfaceManager : IInterfaceManager
{
    private readonly Dictionary<string, IInterfaceConfig> _interfaces = new();
    private readonly Dictionary<int, VlanConfig> _vlans = new();
    private readonly Dictionary<int, PortChannel> _portChannels = new();
    private readonly Dictionary<int, AccessList> _accessLists = new();

    /// <summary>
    /// Gets all interfaces configured on the device.
    /// </summary>
    public Dictionary<string, IInterfaceConfig> GetAllInterfaces() => _interfaces;

    /// <summary>
    /// Gets an interface by name.
    /// </summary>
    public IInterfaceConfig? GetInterface(string name)
    {
        return _interfaces.TryGetValue(name, out var iface) ? iface : null;
    }

    /// <summary>
    /// Adds or updates an interface configuration.
    /// </summary>
    public void AddInterface(string name, IInterfaceConfig config)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Interface name cannot be null or empty.", nameof(name));
        
        _interfaces[name] = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Removes an interface configuration.
    /// </summary>
    public bool RemoveInterface(string name)
    {
        return _interfaces.Remove(name);
    }

    /// <summary>
    /// Gets all VLANs configured on the device.
    /// </summary>
    public Dictionary<int, VlanConfig> GetAllVlans() => _vlans;

    /// <summary>
    /// Gets a VLAN by ID.
    /// </summary>
    public VlanConfig? GetVlan(int id)
    {
        return _vlans.TryGetValue(id, out var vlan) ? vlan : null;
    }

    /// <summary>
    /// Adds or updates a VLAN configuration.
    /// </summary>
    public void AddVlan(int id, VlanConfig config)
    {
        if (id < 1 || id > 4094)
            throw new ArgumentOutOfRangeException(nameof(id), "VLAN ID must be between 1 and 4094.");
        
        _vlans[id] = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Removes a VLAN configuration.
    /// </summary>
    public bool RemoveVlan(int id)
    {
        return _vlans.Remove(id);
    }

    /// <summary>
    /// Gets all port channels configured on the device.
    /// </summary>
    public Dictionary<int, PortChannel> GetPortChannels() => _portChannels;

    /// <summary>
    /// Gets a port channel by number.
    /// </summary>
    public PortChannel? GetPortChannel(int number)
    {
        return _portChannels.TryGetValue(number, out var portChannel) ? portChannel : null;
    }

    /// <summary>
    /// Adds or updates a port channel configuration.
    /// </summary>
    public void AddPortChannel(int number, PortChannel config)
    {
        if (number < 1)
            throw new ArgumentOutOfRangeException(nameof(number), "Port channel number must be positive.");
        
        _portChannels[number] = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Removes a port channel configuration.
    /// </summary>
    public bool RemovePortChannel(int number)
    {
        return _portChannels.Remove(number);
    }

    /// <summary>
    /// Gets all access lists configured on the device.
    /// </summary>
    public Dictionary<int, AccessList> GetAccessLists() => _accessLists;

    /// <summary>
    /// Gets an access list by number.
    /// </summary>
    public AccessList? GetAccessList(int number)
    {
        return _accessLists.TryGetValue(number, out var accessList) ? accessList : null;
    }

    /// <summary>
    /// Adds or updates an access list configuration.
    /// </summary>
    public void AddAccessList(int number, AccessList config)
    {
        if (number < 1)
            throw new ArgumentOutOfRangeException(nameof(number), "Access list number must be positive.");
        
        _accessLists[number] = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Removes an access list configuration.
    /// </summary>
    public bool RemoveAccessList(int number)
    {
        return _accessLists.Remove(number);
    }

    /// <summary>
    /// Clears all interface configurations.
    /// </summary>
    public void ClearAllInterfaces()
    {
        _interfaces.Clear();
    }

    /// <summary>
    /// Clears all VLAN configurations.
    /// </summary>
    public void ClearAllVlans()
    {
        _vlans.Clear();
    }

    /// <summary>
    /// Gets the count of configured interfaces.
    /// </summary>
    public int InterfaceCount => _interfaces.Count;

    /// <summary>
    /// Gets the count of configured VLANs.
    /// </summary>
    public int VlanCount => _vlans.Count;

    /// <summary>
    /// Gets the count of configured port channels.
    /// </summary>
    public int PortChannelCount => _portChannels.Count;

    /// <summary>
    /// Gets the count of configured access lists.
    /// </summary>
    public int AccessListCount => _accessLists.Count;
}