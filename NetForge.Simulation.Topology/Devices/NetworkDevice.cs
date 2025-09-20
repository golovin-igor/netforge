using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Security;
using NetForge.Simulation.DataTypes.Cli;
using NetForge.Simulation.Topology.Services;

namespace NetForge.Simulation.Topology.Devices;

/// <summary>
/// Base class for all network device implementations.
/// Refactored to use service classes for better separation of concerns.
/// </summary>
public abstract class NetworkDevice : INetworkDevice
{
    // Core services
    protected readonly DeviceConfigurationManager _configurationManager;
    private readonly DeviceConnectivityService _connectivityService;
    private readonly DeviceInterfaceManager _interfaceManager;
    private readonly DeviceProtocolHost _protocolHost;
    private readonly DeviceCommandProcessor _commandProcessor;
    private readonly DevicePhysicalConnectivityService _physicalConnectivityService;

    // Identity properties
    public string Name { get; protected set; }
    public string Vendor { get; protected init; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName => Name;
    public abstract string DeviceType { get; }

    // Network context
    public INetwork? ParentNetwork { get; set; }

    // Logging
    public event Action<string> LogEntryAdded = delegate { };
    private readonly List<string> _logEntries = [];

    /// <summary>
    /// Initializes a new instance of the NetworkDevice class.
    /// </summary>
    protected NetworkDevice(string name, string vendor, ICliHandlerManager? commandManager = null)
    {
        Name = name;
        Vendor = vendor;

        // Initialize services
        _interfaceManager = new DeviceInterfaceManager();
        _configurationManager = new DeviceConfigurationManager(name, () => ParentNetwork?.EventBus);
        _connectivityService = new DeviceConnectivityService(_interfaceManager, this);
        _protocolHost = new DeviceProtocolHost(this);
        _commandProcessor = new DeviceCommandProcessor(this, commandManager);
        _physicalConnectivityService = new DevicePhysicalConnectivityService(this);

        // Initialize device
        SetHostname(name);
        InitializeDefaultInterfaces();
        RegisterDeviceSpecificHandlers();
    }

    #region IDeviceIdentity Implementation

    public string GetHostname() => _configurationManager.GetSystemSetting("hostname") ?? Name;
    public void SetHostname(string hostname)
    {
        _configurationManager.SetSystemSetting("hostname", hostname);
        Name = hostname;
    }

    public virtual string? GetVersion() => _configurationManager.GetSystemSetting("version");

    #endregion

    #region IConfigurationProvider Implementation (Delegated)

    public bool IsNvramLoaded
    {
        get => _configurationManager.IsNvramLoaded;
        set => _configurationManager.IsNvramLoaded = value;
    }

    public Dictionary<string, string> GetSystemSettings() => _configurationManager.GetSystemSettings();
    public string? GetSystemSetting(string name) => _configurationManager.GetSystemSetting(name);
    public void SetSystemSetting(string name, string value) => _configurationManager.SetSystemSetting(name, value);
    public void SetRunningConfig(string config) => _configurationManager.SetRunningConfig(config);
    public DeviceConfiguration GetRunningConfig() => _configurationManager.GetRunningConfig();

    // Protocol configurations
    public OspfConfig? GetOspfConfiguration() => _configurationManager.GetOspfConfiguration();
    public void SetOspfConfiguration(OspfConfig config) => _configurationManager.SetOspfConfiguration(config);
    public BgpConfig? GetBgpConfiguration() => _configurationManager.GetBgpConfiguration();
    public void SetBgpConfiguration(BgpConfig config) => _configurationManager.SetBgpConfiguration(config);
    public RipConfig? GetRipConfiguration() => _configurationManager.GetRipConfiguration();
    public void SetRipConfiguration(RipConfig config) => _configurationManager.SetRipConfiguration(config);
    public EigrpConfig? GetEigrpConfiguration() => _configurationManager.GetEigrpConfiguration();
    public void SetEigrpConfiguration(EigrpConfig config) => _configurationManager.SetEigrpConfiguration(config);
    public StpConfig GetStpConfiguration() => _configurationManager.GetStpConfiguration();
    public void SetStpConfiguration(StpConfig config) => _configurationManager.SetStpConfiguration(config);
    public IgrpConfig? GetIgrpConfiguration() => _configurationManager.GetIgrpConfiguration();
    public void SetIgrpConfiguration(IgrpConfig config) => _configurationManager.SetIgrpConfiguration(config);
    public VrrpConfig? GetVrrpConfiguration() => _configurationManager.GetVrrpConfiguration();
    public void SetVrrpConfiguration(VrrpConfig config) => _configurationManager.SetVrrpConfiguration(config);
    public HsrpConfig? GetHsrpConfiguration() => _configurationManager.GetHsrpConfiguration();
    public void SetHsrpConfiguration(HsrpConfig config) => _configurationManager.SetHsrpConfiguration(config);
    public CdpConfig? GetCdpConfiguration() => _configurationManager.GetCdpConfiguration();
    public void SetCdpConfiguration(CdpConfig config) => _configurationManager.SetCdpConfiguration(config);
    public LldpConfig? GetLldpConfiguration() => _configurationManager.GetLldpConfiguration();
    public void SetLldpConfiguration(LldpConfig config) => _configurationManager.SetLldpConfiguration(config);
    public object GetTelnetConfiguration() => _configurationManager.GetTelnetConfiguration();
    public void SetTelnetConfiguration(object config) => _configurationManager.SetTelnetConfiguration(config);
    public object GetSshConfiguration() => _configurationManager.GetSshConfiguration();
    public void SetSshConfiguration(object config) => _configurationManager.SetSshConfiguration(config);
    public object GetSnmpConfiguration() => _configurationManager.GetSnmpConfiguration();
    public void SetSnmpConfiguration(object config) => _configurationManager.SetSnmpConfiguration(config);
    public object GetHttpConfiguration() => _configurationManager.GetHttpConfiguration();
    public void SetHttpConfiguration(object config) => _configurationManager.SetHttpConfiguration(config);

    #endregion

    #region INetworkConnectivity Implementation (Delegated)

    public List<Route> GetRoutingTable() => _connectivityService.GetRoutingTable();
    public void AddRoute(Route route) => _connectivityService.AddRoute(route);
    public void RemoveRoute(Route route) => _connectivityService.RemoveRoute(route);
    public void ClearRoutesByProtocol(string protocol) => _connectivityService.ClearRoutesByProtocol(protocol);
    public virtual void AddStaticRoute(string network, string mask, string nextHop, int metric) =>
        _connectivityService.AddStaticRoute(network, mask, nextHop, metric);
    public void RemoveStaticRoute(string network, string mask) =>
        _connectivityService.RemoveStaticRoute(network, mask);
    public void ForceUpdateConnectedRoutes() => _connectivityService.ForceUpdateConnectedRoutes();
    public Dictionary<string, string> GetArpTable() => _connectivityService.GetArpTable();
    public string GetArpTableOutput() => _connectivityService.GetArpTableOutput();
    public string ExecutePing(string destination) => _connectivityService.ExecutePing(destination);
    public string GetNetworkAddress(string ip, string mask) => _connectivityService.GetNetworkAddress(ip, mask);
    public bool CheckIpInNetwork(string ip, string network, string mask) =>
        _connectivityService.CheckIpInNetwork(ip, network, mask);
    public int MaskToCidr(string mask) => _connectivityService.MaskToCidr(mask);
    public string CidrToMask(int cidr) => _connectivityService.CidrToMask(cidr);
    public void UpdateConnectedRoutes() => _connectivityService.ForceUpdateConnectedRoutes();
    public string GetNetwork(string ip, string mask) => _connectivityService.GetNetworkAddress(ip, mask);

    #endregion

    #region ICommandProcessor Implementation (Delegated)

    public virtual async Task<string> ProcessCommandAsync(string command) =>
        await _commandProcessor.ProcessCommandAsync(command);
    public CommandHistory GetCommandHistory() => _commandProcessor.GetCommandHistory();
    public void ClearCommandHistory() => _commandProcessor.ClearCommandHistory();
    public virtual string GetPrompt() => _commandProcessor.GetPrompt();
    public string GetCurrentPrompt() => _commandProcessor.GetCurrentPrompt();
    public string GetCurrentMode() => _commandProcessor.GetCurrentMode();
    public void SetCurrentMode(string mode) => _commandProcessor.SetCurrentMode(mode);
    public DeviceMode GetCurrentModeEnum() => _commandProcessor.GetCurrentModeEnum();
    public void SetCurrentModeEnum(DeviceMode mode) => _commandProcessor.SetCurrentModeEnum(mode);
    public virtual void SetMode(string mode) => _commandProcessor.SetMode(mode);
    public void SetModeEnum(DeviceMode mode) => _commandProcessor.SetModeEnum(mode);
    public string GetCurrentInterface() => _commandProcessor.GetCurrentInterface();
    public void SetCurrentInterface(string iface) => _commandProcessor.SetCurrentInterface(iface);

    #endregion

    #region IProtocolHost Implementation (Delegated)

    public IProtocolService GetProtocolService() => _protocolHost.GetProtocolService();
    public void RegisterProtocol(IDeviceProtocol protocol) => _protocolHost.RegisterProtocol(protocol);
    public void AddProtocol(IDeviceProtocol protocol) => _protocolHost.RegisterProtocol(protocol); // Alias for static registration compatibility
    public async Task UpdateAllProtocolStates() => await _protocolHost.UpdateAllProtocolStates();
    public void SubscribeProtocolsToEvents() => _protocolHost.SubscribeProtocolsToEvents();
    public IReadOnlyList<IDeviceProtocol> GetRegisteredProtocols() => _protocolHost.GetRegisteredProtocols();

    public T? GetProtocol<T> () where T : IDeviceProtocol => _protocolHost.GetProtocol<T>();

    #endregion

    #region IInterfaceManager Implementation (Delegated)

    public Dictionary<string, IInterfaceConfig> GetAllInterfaces() => _interfaceManager.GetAllInterfaces();
    public virtual IInterfaceConfig? GetInterface(string name) => _interfaceManager.GetInterface(name);
    public Dictionary<int, VlanConfig> GetAllVlans() => _interfaceManager.GetAllVlans();
    public VlanConfig? GetVlan(int id) => _interfaceManager.GetVlan(id);
    public Dictionary<int, PortChannel> GetPortChannels() => _interfaceManager.GetPortChannels();
    public PortChannel? GetPortChannel(int number) => _interfaceManager.GetPortChannel(number);
    public Dictionary<int, AccessList> GetAccessLists() => _interfaceManager.GetAccessLists();
    public AccessList? GetAccessList(int number) => _interfaceManager.GetAccessList(number);

    #endregion

    #region IPhysicalConnectivity Implementation (Delegated)

    public virtual List<PhysicalConnection> GetPhysicalConnectionsForInterface(string interfaceName) =>
        _physicalConnectivityService.GetPhysicalConnectionsForInterface(interfaceName);
    public virtual List<PhysicalConnection> GetOperationalPhysicalConnections() =>
        _physicalConnectivityService.GetOperationalPhysicalConnections();
    public virtual bool IsInterfacePhysicallyConnected(string interfaceName) =>
        _physicalConnectivityService.IsInterfacePhysicallyConnected(interfaceName);
    public virtual PhysicalConnectionMetrics? GetPhysicalConnectionMetrics(string interfaceName) =>
        _physicalConnectivityService.GetPhysicalConnectionMetrics(interfaceName);
    public virtual PhysicalTransmissionResult TestPhysicalConnectivity(string interfaceName, int packetSize = 1500) =>
        _physicalConnectivityService.TestPhysicalConnectivity(interfaceName, packetSize);
    public virtual (INetworkDevice device, string interfaceName)? GetConnectedDevice(string localInterfaceName) =>
        _physicalConnectivityService.GetConnectedDevice(localInterfaceName);
    public virtual bool ShouldInterfaceParticipateInProtocols(string interfaceName) =>
        _physicalConnectivityService.ShouldInterfaceParticipateInProtocols(interfaceName);

    #endregion

    #region IDeviceLogging Implementation

    public List<string> GetLogEntries() => new(_logEntries);
    public void AddLogEntry(string entry)
    {
        _logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {entry}");
        LogEntryAdded?.Invoke(entry);
    }
    public void ClearLog() => _logEntries.Clear();

    #endregion

    #region Abstract Methods

    protected abstract void InitializeDefaultInterfaces();
    protected abstract void RegisterDeviceSpecificHandlers();

    #endregion

    #region Protected Helper Methods

    protected void AddInterface(string name, IInterfaceConfig config) =>
        _interfaceManager.AddInterface(name, config);

    protected void AddVlan(int id, VlanConfig config) =>
        _interfaceManager.AddVlan(id, config);

    #endregion
}
