using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Security;
using NetForge.Simulation.DataTypes.Cli;

namespace NetForge.Simulation.Common.Common;

public interface INetworkDevice
{
    string Name { get; }
    string Vendor { get; }

    INetwork? ParentNetwork { get; set; }

    /// <summary>
    /// Contains the unique identifier for this device in the network, that we import from the generated network.
    /// </summary>
    string DeviceId { get; set; }

    bool IsNvramLoaded { get; set; }

    /// <summary>
    /// Get the device name for protocol service compatibility
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Get the device type for protocol service compatibility
    /// </summary>
    string DeviceType { get; }

    event Action<string> LogEntryAdded;

    /// <summary>
    /// Get the enhanced protocol service for this device
    /// </summary>
    /// <returns>Protocol service instance</returns>
    IProtocolService GetProtocolService();

    /// <summary>
    /// Asynchronously process a command and return the output
    /// </summary>
    Task<string> ProcessCommandAsync(string command);

    /// <summary>
    /// Get the command history for this device
    /// </summary>
    CommandHistory GetCommandHistory();

    /// <summary>
    /// Get the current CLI prompt
    /// </summary>
    string GetPrompt();

    /// <summary>
    /// Implementation of ICommandProcessor.GetCurrentPrompt
    /// </summary>
    string GetCurrentPrompt();

    /// <summary>
    /// Public accessors for command handlers
    /// </summary>
    Dictionary<string, IInterfaceConfig> GetAllInterfaces();

    Dictionary<int, VlanConfig> GetAllVlans();
    List<Route> GetRoutingTable();
    Dictionary<int, AccessList> GetAccessLists();
    Dictionary<int, PortChannel> GetPortChannels();
    Dictionary<string, string> GetSystemSettings();
    List<string> GetLogEntries();
    Dictionary<string, string> GetArpTable();
    string GetArpTableOutput();
    OspfConfig? GetOspfConfiguration();
    BgpConfig? GetBgpConfiguration();
    RipConfig? GetRipConfiguration();
    EigrpConfig? GetEigrpConfiguration();
    StpConfig GetStpConfiguration();
    IgrpConfig? GetIgrpConfiguration();
    VrrpConfig? GetVrrpConfiguration();
    HsrpConfig? GetHsrpConfiguration();
    CdpConfig? GetCdpConfiguration();
    LldpConfig? GetLldpConfiguration();
    void SetOspfConfiguration(OspfConfig config);
    void SetBgpConfiguration(BgpConfig config);
    void SetRipConfiguration(RipConfig config);
    void SetEigrpConfiguration(EigrpConfig config);
    void SetStpConfiguration(StpConfig config);
    void SetIgrpConfiguration(IgrpConfig config);
    void SetVrrpConfiguration(VrrpConfig config);
    void SetHsrpConfiguration(HsrpConfig config);
    void SetCdpConfiguration(CdpConfig config);
    void SetLldpConfiguration(LldpConfig config);
    object GetTelnetConfiguration();
    void SetTelnetConfiguration(object config);
    object GetSshConfiguration();
    void SetSshConfiguration(object config);
    object GetSnmpConfiguration();
    void SetSnmpConfiguration(object config);
    object GetHttpConfiguration();
    void SetHttpConfiguration(object config);
    string GetHostname();
    void SetHostname(string name);
    string GetCurrentMode();
    void SetCurrentMode(string mode);

    /// <summary>
    /// Get the current device mode as a strongly typed enum
    /// </summary>
    DeviceMode GetCurrentModeEnum();

    /// <summary>
    /// Set the current device mode using strongly typed enum
    /// </summary>
    void SetCurrentModeEnum(DeviceMode mode);

    string GetCurrentInterface();
    void SetCurrentInterface(string iface);

    /// <summary>
    /// Set the device mode (e.g., configure, interface, etc.)
    /// </summary>
    void SetMode(string mode);

    /// <summary>
    /// Set the device mode using strongly typed enum
    /// </summary>
    void SetModeEnum(DeviceMode mode);

    /// <summary>
    /// Public method wrappers for protected methods
    /// </summary>
    string GetNetworkAddress(string ip, string mask);

    void ForceUpdateConnectedRoutes();
    string ExecutePing(string destination);
    bool CheckIpInNetwork(string ip, string network, string mask);

    /// <summary>
    /// Add routes to routing table
    /// </summary>
    void AddRoute(Route route);

    /// <summary>
    /// Remove routes from routing table
    /// </summary>
    void RemoveRoute(Route route);

    /// <summary>
    /// Clear all routes of a specific protocol
    /// </summary>
    void ClearRoutesByProtocol(string protocol);

    /// <summary>
    /// Convert subnet mask to CIDR notation
    /// </summary>
    int MaskToCidr(string mask);

    /// <summary>
    /// Get interface by name
    /// </summary>
    IInterfaceConfig? GetInterface(string name);

    /// <summary>
    /// Get VLAN by ID
    /// </summary>
    VlanConfig? GetVlan(int id);

    /// <summary>
    /// Get access list by number
    /// </summary>
    AccessList? GetAccessList(int number);

    /// <summary>
    /// Get port channel by number
    /// </summary>
    PortChannel? GetPortChannel(int number);

    /// <summary>
    /// Get system setting by name
    /// </summary>
    string? GetSystemSetting(string name);

    /// <summary>
    /// Set system setting
    /// </summary>
    void SetSystemSetting(string name, string value);

    /// <summary>
    /// Add log entry
    /// </summary>
    void AddLogEntry(string entry);

    /// <summary>
    /// Clear log entries
    /// </summary>
    void ClearLog();

    /// <summary>
    /// Add a static route
    /// </summary>
    void AddStaticRoute(string network, string mask, string nextHop, int metric);

    /// <summary>
    /// Remove a static route
    /// </summary>
    void RemoveStaticRoute(string network, string mask);

    /// <summary>
    /// Registers a network protocol implementation with the device.
    /// </summary>
    /// <param name="protocol">The protocol to register.</param>
    void RegisterProtocol(IDeviceProtocol protocol);

    /// <summary>
    /// Updates the state of all registered network protocols.
    /// </summary>
    Task UpdateAllProtocolStates();

    /// <summary>
    /// Get physical connections for a specific interface
    /// </summary>
    List<PhysicalConnection> GetPhysicalConnectionsForInterface(string interfaceName);

    /// <summary>
    /// Get all operational physical connections for this device
    /// </summary>
    List<PhysicalConnection> GetOperationalPhysicalConnections();

    /// <summary>
    /// Check if an interface has operational physical connectivity
    /// </summary>
    bool IsInterfacePhysicallyConnected(string interfaceName);

    /// <summary>
    /// Get physical connection quality metrics for an interface
    /// </summary>
    PhysicalConnectionMetrics? GetPhysicalConnectionMetrics(string interfaceName);

    /// <summary>
    /// Test physical connectivity by simulating packet transmission
    /// </summary>
    PhysicalTransmissionResult TestPhysicalConnectivity(string interfaceName, int packetSize = 1500);

    /// <summary>
    /// Get remote device and interface connected to a local interface
    /// </summary>
    (INetworkDevice device, string interfaceName)? GetConnectedDevice(string localInterfaceName);

    /// <summary>
    /// Check if protocols should consider this interface for routing/switching decisions
    /// This method respects physical connectivity - protocols should only use interfaces
    /// that have operational physical connections
    /// </summary>
    bool ShouldInterfaceParticipateInProtocols(string interfaceName);

    /// <summary>
    /// Just set the running configuration directly, no commands processed.
    /// </summary>
    /// <param name="config"></param>
    void SetRunningConfig(string config);

    /// <summary>
    /// Subscribe all registered protocols to events
    /// This should be called when a device is added to a network
    /// </summary>
    void SubscribeProtocolsToEvents();

    /// <summary>
    /// Get all registered network protocols
    /// </summary>
    /// <returns>Read-only collection of registered protocols</returns>
    IReadOnlyList<IDeviceProtocol> GetRegisteredProtocols();

    /// <summary>
    /// Clear the command history
    /// </summary>
    void ClearCommandHistory();


}
