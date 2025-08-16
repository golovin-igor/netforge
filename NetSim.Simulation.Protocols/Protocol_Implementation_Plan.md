# Protocol Implementation Plan

## Overview

This document outlines the comprehensive plan for implementing a modular, plugin-based protocol architecture for the NetSim network simulation system. The architecture is designed to provide maximum modularity, vendor-specific protocol support, and seamless integration with the existing CLI handler system.

## Current State Analysis

### Existing CLI Handler Architecture
- **Interface**: `ICliHandler` at `NetSim.Simulation.Common\CLI\Interfaces\ICliHandler.cs:8`
- **Base Class**: `BaseCliHandler` at `NetSim.Simulation.Common\CLI\Base\BaseCliHandler.cs:8`
- **Registry Pattern**: `VendorHandlerRegistryBase` with auto-discovery via reflection
- **Vendor-Specific**: Handler registries per vendor (e.g., `CiscoHandlerRegistry.cs:9`)
- **Priority-Based**: Higher priority handlers loaded first

### Current Protocol Architecture
- **Interface**: `INetworkProtocol` at `NetSim.Simulation.Common\Interfaces\INetworkProtocol.cs:23`
- **Direct Implementation**: Protocols like `CdpProtocol.cs:10` directly implement interface
- **Manual Registration**: Hardcoded `RegisterProtocol()` calls in device constructors at `NetworkDevice.cs:751`
- **State Management**: Sophisticated pattern documented in `PROTOCOL_STATE_MANAGEMENT.md`

### Existing State Management Pattern
Based on `PROTOCOL_STATE_MANAGEMENT.md`, protocols already implement:
- **Change Detection**: `StateChanged`, `TopologyChanged`, `PolicyChanged` flags
- **Conditional Processing**: Expensive operations only when state changes
- **Neighbor Management**: Automatic cleanup of stale neighbors
- **Timer Management**: Proper timeout and refresh mechanisms
- **Performance Optimization**: Skip calculations when state unchanged

## Proposed Architecture

### 1. Project Structure: Per-Protocol Modularity

```
NetSim.Simulation.Protocols.Common\              # Core interfaces and base classes
├── NetSim.Simulation.Protocols.Common.csproj
├── Interfaces\
│   ├── INetworkProtocol.cs                      # Enhanced protocol interface
│   ├── IProtocolState.cs                        # State management interface
│   ├── IProtocolPlugin.cs                       # Plugin interface
│   └── IProtocolService.cs                      # Service for CLI handlers
├── Base\
│   ├── BaseProtocol.cs                          # Base implementation with state management
│   ├── BaseProtocolState.cs                     # Base state class
│   └── ProtocolPluginBase.cs                    # Base plugin class
├── Services\
│   ├── ProtocolDiscoveryService.cs              # Auto-discovery service
│   └── ProtocolPluginManager.cs                 # Plugin management
└── Events\
    ├── ProtocolStateChangedEventArgs.cs
    └── ProtocolConfigChangedEventArgs.cs

NetSim.Simulation.Protocols.Telnet\              # Management protocol
NetSim.Simulation.Protocols.OSPF\                # Layer 3 routing
NetSim.Simulation.Protocols.BGP\                 # Layer 3 routing
NetSim.Simulation.Protocols.EIGRP\               # Layer 3 routing (Cisco-specific)
NetSim.Simulation.Protocols.RIP\                 # Layer 3 routing
NetSim.Simulation.Protocols.ISIS\                # Layer 3 routing
NetSim.Simulation.Protocols.IGRP\                # Layer 3 routing (Cisco-specific)
NetSim.Simulation.Protocols.CDP\                 # Layer 2 discovery (Cisco-specific)
NetSim.Simulation.Protocols.LLDP\                # Layer 2 discovery
NetSim.Simulation.Protocols.STP\                 # Layer 2 switching
NetSim.Simulation.Protocols.HSRP\                # Layer 3 redundancy (Cisco-specific)
NetSim.Simulation.Protocols.VRRP\                # Layer 3 redundancy
NetSim.Simulation.Protocols.ARP\                 # Layer 3 network
```

### 2. Core Common Library Implementation

#### Enhanced Protocol Interface

```csharp
// NetSim.Simulation.Protocols.Common/Interfaces/INetworkProtocol.cs
namespace NetSim.Simulation.Protocols.Common
{
    public interface INetworkProtocol
    {
        // Core protocol properties
        ProtocolType Type { get; }
        string Name { get; }
        string Version { get; }
        
        // Lifecycle management
        void Initialize(NetworkDevice device);
        Task UpdateState(NetworkDevice device);
        void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self);
        
        // State access for CLI handlers
        IProtocolState GetState();
        T GetTypedState<T>() where T : class;
        
        // Configuration management
        object GetConfiguration();
        void ApplyConfiguration(object configuration);
        
        // Vendor support
        IEnumerable<string> GetSupportedVendors();
        bool SupportsVendor(string vendorName);
    }

    public interface IProtocolState
    {
        DateTime LastUpdate { get; }
        bool IsActive { get; }
        bool IsConfigured { get; }
        bool StateChanged { get; }
        
        void MarkStateChanged();
        Dictionary<string, object> GetStateData();
        T GetTypedState<T>() where T : class;
        
        // Neighbor management
        List<string> GetStaleNeighbors(int timeoutSeconds = 180);
        void RemoveNeighbor(string id);
        void UpdateNeighborActivity(string id);
    }

    public interface IProtocolPlugin
    {
        string PluginName { get; }
        string Version { get; }
        ProtocolType ProtocolType { get; }
        int Priority { get; }
        
        INetworkProtocol CreateProtocol();
        bool SupportsVendor(string vendorName);
        IEnumerable<string> GetSupportedVendors();
    }

    public interface IProtocolService
    {
        T GetProtocol<T>() where T : class, INetworkProtocol;
        INetworkProtocol GetProtocol(ProtocolType type);
        TState GetProtocolState<TState>(ProtocolType type) where TState : class;
        IEnumerable<INetworkProtocol> GetAllProtocols();
        bool IsProtocolActive(ProtocolType type);
    }
}
```

#### Base Protocol Implementation with State Management

```csharp
// NetSim.Simulation.Protocols.Common/Base/BaseProtocol.cs
namespace NetSim.Simulation.Protocols.Common
{
    public abstract class BaseProtocol : INetworkProtocol
    {
        protected NetworkDevice _device;
        protected readonly BaseProtocolState _state;
        
        public abstract ProtocolType Type { get; }
        public abstract string Name { get; }
        public virtual string Version => "1.0.0";
        
        protected BaseProtocol()
        {
            _state = CreateInitialState();
        }
        
        protected abstract BaseProtocolState CreateInitialState();
        
        public virtual void Initialize(NetworkDevice device)
        {
            _device = device;
            _state.IsConfigured = true;
            _state.MarkStateChanged();
            
            device.AddLogEntry($"{Name} protocol initialized");
            OnInitialized();
        }
        
        protected virtual void OnInitialized() { }
        
        // Core state management pattern from PROTOCOL_STATE_MANAGEMENT.md
        public virtual async Task UpdateState(NetworkDevice device)
        {
            if (!_state.IsActive || !_state.IsConfigured)
                return;
            
            // Always update neighbors and timers
            await UpdateNeighbors(device);
            await CleanupStaleNeighbors(device);
            await ProcessTimers(device);
            
            // Only run expensive operations if state changed
            if (_state.StateChanged)
            {
                device.AddLogEntry($"{Name}: State changed, running protocol calculations...");
                await RunProtocolCalculation(device);
                _state.StateChanged = false;
                _state.LastUpdate = DateTime.Now;
            }
            else
            {
                device.AddLogEntry($"{Name}: No state changes detected, skipping expensive calculations.");
            }
        }
        
        // Template methods for protocol-specific implementation
        protected virtual async Task UpdateNeighbors(NetworkDevice device) { }
        
        protected virtual async Task CleanupStaleNeighbors(NetworkDevice device)
        {
            var staleNeighbors = _state.GetStaleNeighbors();
            foreach (var neighborId in staleNeighbors)
            {
                device.AddLogEntry($"{Name}: Neighbor {neighborId} timed out, removing");
                _state.RemoveNeighbor(neighborId);
                OnNeighborRemoved(neighborId);
            }
        }
        
        protected virtual async Task ProcessTimers(NetworkDevice device) { }
        
        protected abstract Task RunProtocolCalculation(NetworkDevice device);
        
        protected virtual void OnNeighborRemoved(string neighborId) { }
        
        // State access for CLI handlers
        public IProtocolState GetState() => _state;
        public T GetTypedState<T>() where T : class => _state as T;
        
        // Configuration management
        public virtual object GetConfiguration() => GetProtocolConfiguration();
        protected abstract object GetProtocolConfiguration();
        
        public virtual void ApplyConfiguration(object configuration)
        {
            OnApplyConfiguration(configuration);
            _state.MarkStateChanged();
        }
        protected abstract void OnApplyConfiguration(object configuration);
        
        // Vendor support
        public virtual IEnumerable<string> GetSupportedVendors() => new[] { "Generic" };
        
        public virtual bool SupportsVendor(string vendorName)
        {
            return GetSupportedVendors().Contains(vendorName, StringComparer.OrdinalIgnoreCase);
        }
        
        // Event subscription
        public virtual void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            OnSubscribeToEvents(eventBus, self);
        }
        
        protected virtual void OnSubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self) { }
    }

    public abstract class BaseProtocolState : IProtocolState
    {
        // Core state tracking from PROTOCOL_STATE_MANAGEMENT.md
        public bool StateChanged { get; set; } = true;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public bool IsConfigured { get; set; } = false;
        
        // Neighbor management
        protected readonly Dictionary<string, object> _neighbors = new();
        protected readonly Dictionary<string, DateTime> _neighborLastSeen = new();
        
        public virtual void MarkStateChanged() => StateChanged = true;
        
        public virtual TNeighbor GetOrCreateNeighbor<TNeighbor>(string id, Func<TNeighbor> factory) 
            where TNeighbor : class
        {
            if (!_neighbors.ContainsKey(id))
            {
                _neighbors[id] = factory();
                MarkStateChanged();
            }
            return (TNeighbor)_neighbors[id];
        }
        
        public virtual void RemoveNeighbor(string id)
        {
            if (_neighbors.Remove(id))
            {
                _neighborLastSeen.Remove(id);
                MarkStateChanged();
            }
        }
        
        public virtual List<string> GetStaleNeighbors(int timeoutSeconds = 180)
        {
            var staleNeighbors = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in _neighborLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > timeoutSeconds)
                {
                    staleNeighbors.Add(kvp.Key);
                }
            }
            
            return staleNeighbors;
        }
        
        public virtual void UpdateNeighborActivity(string id)
        {
            _neighborLastSeen[id] = DateTime.Now;
        }
        
        // IProtocolState implementation
        public virtual Dictionary<string, object> GetStateData()
        {
            return new Dictionary<string, object>
            {
                ["LastUpdate"] = LastUpdate,
                ["IsActive"] = IsActive,
                ["IsConfigured"] = IsConfigured,
                ["StateChanged"] = StateChanged,
                ["NeighborCount"] = _neighbors.Count
            };
        }
        
        public virtual T GetTypedState<T>() where T : class => this as T;
    }

    public abstract class ProtocolPluginBase : IProtocolPlugin
    {
        public abstract string PluginName { get; }
        public virtual string Version => "1.0.0";
        public abstract ProtocolType ProtocolType { get; }
        public virtual int Priority => 100;
        
        public abstract INetworkProtocol CreateProtocol();
        
        public virtual bool SupportsVendor(string vendorName)
        {
            return GetSupportedVendors().Contains(vendorName, StringComparer.OrdinalIgnoreCase);
        }
        
        public virtual IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Generic" };
        }
    }
}
```

### 3. Protocol Discovery Service

```csharp
// NetSim.Simulation.Protocols.Common/Services/ProtocolDiscoveryService.cs
namespace NetSim.Simulation.Protocols.Common.Services
{
    public class ProtocolDiscoveryService
    {
        private readonly List<IProtocolPlugin> _plugins = new();
        private bool _isDiscovered = false;
        
        public IEnumerable<IProtocolPlugin> DiscoverProtocolPlugins()
        {
            if (!_isDiscovered)
            {
                DiscoverAndRegisterPlugins();
                _isDiscovered = true;
            }
            return _plugins.OrderByDescending(p => p.Priority);
        }
        
        public IEnumerable<INetworkProtocol> GetProtocolsForVendor(string vendorName)
        {
            var protocols = new List<INetworkProtocol>();
            
            // Always include Telnet for management
            var telnetPlugin = _plugins.FirstOrDefault(p => p.ProtocolType == ProtocolType.TELNET);
            if (telnetPlugin != null)
            {
                protocols.Add(telnetPlugin.CreateProtocol());
            }
            
            // Add vendor-specific protocols
            foreach (var plugin in DiscoverProtocolPlugins())
            {
                if (plugin.ProtocolType != ProtocolType.TELNET && plugin.SupportsVendor(vendorName))
                {
                    try
                    {
                        var protocol = plugin.CreateProtocol();
                        protocols.Add(protocol);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create protocol {plugin.PluginName}: {ex.Message}");
                    }
                }
            }
            
            return protocols;
        }
        
        private void DiscoverAndRegisterPlugins()
        {
            // Assembly discovery logic (similar to CLI handlers)
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && 
                           (a.FullName?.Contains("NetSim.Simulation.Protocols.") ?? false) &&
                           !a.FullName.Contains("Common"));
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IProtocolPlugin).IsAssignableFrom(t) && 
                                   !t.IsInterface && !t.IsAbstract);
                    
                    foreach (var pluginType in pluginTypes)
                    {
                        try
                        {
                            var plugin = (IProtocolPlugin)Activator.CreateInstance(pluginType);
                            if (plugin != null)
                            {
                                _plugins.Add(plugin);
                            }
                        }
                        catch
                        {
                            // Ignore failed plugin instantiation
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore reflection errors
                }
            }
        }
    }
}
```

### 4. Telnet Protocol Implementation

#### Special Management Protocol

```csharp
// NetSim.Simulation.Protocols.Telnet/TelnetProtocol.cs
namespace NetSim.Simulation.Protocols.Telnet
{
    public class TelnetProtocol : BaseProtocol
    {
        private TelnetServer _telnetServer;
        private readonly TelnetSessionManager _sessionManager;
        
        public override ProtocolType Type => ProtocolType.TELNET;
        public override string Name => "Telnet Protocol";
        
        public TelnetProtocol()
        {
            _sessionManager = new TelnetSessionManager();
        }
        
        protected override BaseProtocolState CreateInitialState()
        {
            return new TelnetState();
        }
        
        protected override void OnInitialized()
        {
            var telnetConfig = GetTelnetConfig();
            if (telnetConfig.IsEnabled)
            {
                StartTelnetServer(telnetConfig);
            }
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var telnetState = (TelnetState)_state;
            var telnetConfig = GetTelnetConfig();
            
            if (!telnetConfig.IsEnabled)
            {
                await StopTelnetServer();
                telnetState.IsActive = false;
                return;
            }
            
            // Update active sessions
            await _sessionManager.UpdateSessions();
            
            // Update telnet state
            telnetState.ActiveSessions = _sessionManager.GetActiveSessions().Count;
            telnetState.TotalConnections = _sessionManager.GetTotalConnectionCount();
            telnetState.LastActivity = _sessionManager.GetLastActivity();
        }
        
        private void StartTelnetServer(TelnetConfig config)
        {
            try
            {
                _telnetServer = new TelnetServer(_device, config, _sessionManager);
                _telnetServer.ConnectionReceived += OnTelnetConnectionReceived;
                _telnetServer.CommandReceived += OnTelnetCommandReceived;
                _telnetServer.Start();
                
                _device.AddLogEntry($"Telnet server started on port {config.Port}");
                ((TelnetState)_state).IsActive = true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Failed to start Telnet server: {ex.Message}");
                ((TelnetState)_state).IsActive = false;
            }
        }
        
        private async void OnTelnetCommandReceived(object sender, TelnetCommandEventArgs e)
        {
            var session = e.Session;
            var command = e.Command;
            
            _device.AddLogEntry($"Telnet command from {session.ClientEndpoint}: {command}");
            
            try
            {
                // Route to CLI handlers - this is the key integration point
                var response = await ProcessTelnetCommand(session, command);
                await session.SendResponse(response);
            }
            catch (Exception ex)
            {
                await session.SendResponse($"Error: {ex.Message}");
                _device.AddLogEntry($"Error processing Telnet command: {ex.Message}");
            }
        }
        
        private async Task<string> ProcessTelnetCommand(TelnetSession session, string command)
        {
            if (session.IsAuthenticated)
            {
                // Use the device's existing CLI processing system
                var cliResponse = await _device.ProcessCommandAsync(command, session);
                return FormatTelnetResponse(cliResponse, session);
            }
            else
            {
                return await ProcessAuthenticationCommand(session, command);
            }
        }
        
        private string FormatTelnetResponse(string cliResponse, TelnetSession session)
        {
            var prompt = GetDevicePrompt(session);
            return $"{cliResponse}\r\n{prompt}";
        }
        
        private string GetDevicePrompt(TelnetSession session)
        {
            var hostname = _device.Hostname ?? _device.Name;
            var mode = session.CurrentMode ?? ">";
            return $"{hostname}{mode} ";
        }
        
        protected override object GetProtocolConfiguration()
        {
            return _device?.GetTelnetConfiguration() ?? new TelnetConfig { IsEnabled = true, Port = 23 };
        }
        
        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is TelnetConfig telnetConfig)
            {
                _device?.SetTelnetConfiguration(telnetConfig);
                
                // Restart server if configuration changed
                _ = Task.Run(async () =>
                {
                    await StopTelnetServer();
                    if (telnetConfig.IsEnabled)
                    {
                        StartTelnetServer(telnetConfig);
                    }
                });
            }
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Generic", "Cisco", "Juniper", "Arista" }; // All vendors support Telnet
        }
    }

    public class TelnetProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "Telnet Protocol Plugin";
        public override ProtocolType ProtocolType => ProtocolType.TELNET;
        public override int Priority => 1000; // Highest priority for management protocol
        
        public override INetworkProtocol CreateProtocol() => new TelnetProtocol();
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Generic", "Cisco", "Juniper", "Arista" };
        }
    }
}
```

### 5. Individual Protocol Project Example: OSPF

```csharp
// NetSim.Simulation.Protocols.OSPF/OspfProtocol.cs
namespace NetSim.Simulation.Protocols.OSPF
{
    public class OspfProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.OSPF;
        public override string Name => "Open Shortest Path First";
        
        protected override BaseProtocolState CreateInitialState()
        {
            return new OspfState();
        }
        
        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            var ospfState = (OspfState)_state;
            var ospfConfig = device.GetOspfConfiguration();
            
            if (ospfConfig == null || !ospfConfig.IsEnabled)
            {
                ospfState.IsActive = false;
                return;
            }
            
            // Discover OSPF neighbors using the state management pattern
            await DiscoverOspfNeighbors(device, ospfConfig, ospfState);
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var ospfState = (OspfState)_state;
            
            device.AddLogEntry("OSPF: Running SPF calculation due to topology change...");
            
            // Clear existing OSPF routes
            device.ClearRoutesByProtocol("OSPF");
            ospfState.RoutingTable.Clear();
            
            // Run Dijkstra's SPF algorithm
            await RunSpfCalculation(device, ospfState);
            
            device.AddLogEntry("OSPF: SPF calculation completed");
        }
        
        private async Task DiscoverOspfNeighbors(NetworkDevice device, OspfConfig config, OspfState state)
        {
            // Implementation following the existing pattern from PROTOCOL_STATE_MANAGEMENT.md
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;
                
                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    
                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                        continue;
                    
                    var neighborOspf = neighborDevice.GetOspfConfiguration();
                    if (neighborOspf?.IsEnabled == true)
                    {
                        var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
                        var neighbor = state.GetOrCreateNeighbor(neighborKey, () => new OspfNeighbor
                        {
                            RouterId = neighborDevice.Name,
                            InterfaceName = neighborInterface,
                            State = OspfNeighborState.Down
                        });
                        
                        // Update neighbor state machine
                        await UpdateNeighborStateMachine(neighbor, device, neighborDevice);
                        state.UpdateNeighborActivity(neighborKey);
                        
                        device.AddLogEntry($"OSPF: Neighbor {neighbor.RouterId} state: {neighbor.State}");
                    }
                }
            }
        }
        
        private async Task RunSpfCalculation(NetworkDevice device, OspfState state)
        {
            // Dijkstra's algorithm implementation
            // Install calculated routes with OSPF administrative distance
            foreach (var route in state.CalculatedRoutes)
            {
                var deviceRoute = new Route(route.Network, route.Mask, route.NextHop, route.Interface, "OSPF")
                {
                    Metric = route.Cost,
                    AdminDistance = 110 // OSPF administrative distance
                };
                device.AddRoute(deviceRoute);
            }
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
        
        protected override object GetProtocolConfiguration()
        {
            return _device?.GetOspfConfiguration();
        }
        
        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is OspfConfig ospfConfig)
            {
                _device?.SetOspfConfiguration(ospfConfig);
                _state.IsActive = ospfConfig.IsEnabled;
            }
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Generic" };
        }
    }
    
    public class OspfProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "OSPF Protocol Plugin";
        public override ProtocolType ProtocolType => ProtocolType.OSPF;
        public override int Priority => 110; // Administrative distance
        
        public override INetworkProtocol CreateProtocol() => new OspfProtocol();
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Generic" };
        }
    }

    // NetSim.Simulation.Protocols.OSPF/Configuration/OspfState.cs
    public class OspfState : BaseProtocolState
    {
        public string RouterId { get; set; } = "";
        public Dictionary<string, OspfArea> Areas { get; set; } = new();
        public Dictionary<string, OspfNeighbor> Neighbors { get; set; } = new();
        public Dictionary<string, OspfRoute> RoutingTable { get; set; } = new();
        public List<OspfRoute> CalculatedRoutes { get; set; } = new();
        public bool TopologyChanged { get; set; } = true;
        
        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["RouterId"] = RouterId;
            baseData["Areas"] = Areas;
            baseData["Neighbors"] = Neighbors;
            baseData["RoutingTable"] = RoutingTable;
            baseData["TopologyChanged"] = TopologyChanged;
            return baseData;
        }
    }
}
```

### 6. Enhanced NetworkDevice Integration

```csharp
// Enhanced NetworkDevice with IoC and Protocol Management
public abstract class NetworkDevice : INetworkDevice, ICommandProcessor
{
    protected readonly IServiceProvider _serviceProvider;
    private readonly ServiceCollection _services;
    
    // Protocol management
    private readonly List<INetworkProtocol> _protocols = [];
    private readonly ProtocolDiscoveryService _protocolDiscovery;
    
    // Telnet configuration
    protected TelnetConfig TelnetConfig = new() { IsEnabled = true, Port = 23 };
    
    protected NetworkDevice(string name)
    {
        Name = name;
        Hostname = name;
        
        // Setup IoC container
        _services = new ServiceCollection();
        ConfigureServices(_services);
        _serviceProvider = _services.BuildServiceProvider();
        
        // Initialize protocol discovery
        _protocolDiscovery = new ProtocolDiscoveryService();
        
        InitializeDefaultInterfaces();
        
        // Initialize with dependency injection
        CommandManager = new CliHandlerManager(this, _serviceProvider);
        RegisterCommonHandlers();
        RegisterDeviceSpecificHandlers();
        
        // Auto-register protocols based on vendor
        AutoRegisterProtocols();
        
        CommandHistory = new CommandHistory.CommandHistory(1000);
    }
    
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Register device services for CLI handlers
        services.AddSingleton<INetworkDevice>(this);
        services.AddSingleton<IProtocolService>(provider => new NetworkDeviceProtocolService(this));
        services.AddSingleton<IVendorContext>(provider => CreateVendorContext());
        
        // Add protocol-related services
        services.AddTransient<IRoutingTableService, RoutingTableService>();
        services.AddTransient<IInterfaceService, InterfaceService>();
    }
    
    private void AutoRegisterProtocols()
    {
        var protocols = _protocolDiscovery.GetProtocolsForVendor(this.Vendor);
        
        foreach (var protocol in protocols)
        {
            RegisterProtocol(protocol);
        }
        
        AddLogEntry($"Auto-registered {_protocols.Count} protocols for vendor {Vendor}");
    }
    
    // Enhanced protocol registration with state management
    public void RegisterProtocol(INetworkProtocol protocol)
    {
        if (protocol != null && !_protocols.Any(p => p.Type == protocol.Type))
        {
            _protocols.Add(protocol);
            protocol.Initialize(this);
            
            if (ParentNetwork?.EventBus != null)
            {
                protocol.SubscribeToEvents(ParentNetwork.EventBus, this);
                AddLogEntry($"Protocol {protocol.Type} registered, initialized, and subscribed to events.");
            }
            else
            {
                AddLogEntry($"Protocol {protocol.Type} registered and initialized. EventBus not available yet.");
            }
        }
        else if (protocol != null)
        {
            AddLogEntry($"Protocol {protocol.Type} is already registered.");
        }
    }
    
    // Enhanced command processing with Telnet session support
    public virtual async Task<string> ProcessCommandAsync(string command, TelnetSession session = null)
    {
        try
        {
            CommandHistory.AddCommand(command);
            
            var context = new CliContext
            {
                Device = this,
                CommandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                FullCommand = command,
                ServiceProvider = _serviceProvider,
                Session = session // Support for Telnet sessions
            };
            
            var result = await CommandManager.ProcessCommandAsync(context);
            
            // Update session mode if changed by command (for Telnet)
            if (session != null && result.NewMode.HasValue)
            {
                session.UpdateMode(result.NewMode.Value);
            }
            
            return result.Output;
        }
        catch (Exception ex)
        {
            AddLogEntry($"Command processing error: {ex.Message}");
            return $"% Error: {ex.Message}";
        }
    }
    
    // Telnet configuration methods
    public TelnetConfig GetTelnetConfiguration() => TelnetConfig;
    public void SetTelnetConfiguration(TelnetConfig config) => TelnetConfig = config;
    
    // Enhanced protocol state updates with state management
    public virtual async Task UpdateAllProtocolStates()
    {
        AddLogEntry("Updating all protocol states...");
        foreach (var protocol in _protocols)
        {
            try
            {
                await protocol.UpdateState(this);
                AddLogEntry($"Protocol {protocol.Type} state updated.");
            }
            catch (Exception ex)
            {
                AddLogEntry($"Error updating protocol {protocol.Type}: {ex.Message}");
            }
        }
    }
}
```

### 7. CLI Handler Integration with Protocol Service

```csharp
// Enhanced CliContext for protocol access
public class CliContext
{
    public INetworkDevice Device { get; set; }
    public string[] CommandParts { get; set; }
    public string FullCommand { get; set; }
    public IServiceProvider ServiceProvider { get; set; }
    public TelnetSession Session { get; set; } // For Telnet integration
    
    // Convenience methods
    public T GetService<T>() => ServiceProvider.GetService<T>();
    public IProtocolService GetProtocolService() => GetService<IProtocolService>();
}

// Enhanced CliResult to support mode changes
public class CliResult
{
    public bool IsSuccess { get; set; }
    public string Output { get; set; }
    public CliErrorType ErrorType { get; set; }
    public string[] Suggestions { get; set; }
    
    // New: Mode change support for Telnet sessions
    public DeviceMode? NewMode { get; set; }
    
    public static CliResult Ok(string output, DeviceMode? newMode = null)
    {
        return new CliResult { IsSuccess = true, Output = output, NewMode = newMode };
    }
}

// Protocol service for CLI handlers
public class NetworkDeviceProtocolService : IProtocolService
{
    private readonly NetworkDevice _device;
    
    public NetworkDeviceProtocolService(NetworkDevice device)
    {
        _device = device;
    }
    
    public T GetProtocol<T>() where T : class, INetworkProtocol
    {
        return _device._protocols.OfType<T>().FirstOrDefault();
    }
    
    public INetworkProtocol GetProtocol(ProtocolType type)
    {
        return _device._protocols.FirstOrDefault(p => p.Type == type);
    }
    
    public TState GetProtocolState<TState>(ProtocolType type) where TState : class
    {
        var protocol = GetProtocol(type);
        return protocol?.GetState()?.GetTypedState<TState>();
    }
    
    public IEnumerable<INetworkProtocol> GetAllProtocols()
    {
        return _device._protocols.AsReadOnly();
    }
    
    public bool IsProtocolActive(ProtocolType type)
    {
        var protocol = GetProtocol(type);
        return protocol?.GetState()?.IsActive ?? false;
    }
}

// Example CLI handler using protocol service
public class ShowRouterOspfHandler : BaseCliHandler
{
    public ShowRouterOspfHandler() : base("ospf", "Show OSPF routing information") { }
    
    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
    {
        var protocolService = context.GetProtocolService();
        if (protocolService == null)
        {
            return Error(CliErrorType.ExecutionError, "Protocol service not available");
        }
        
        // Get OSPF state using the state management pattern
        var ospfState = protocolService.GetProtocolState<OspfState>(ProtocolType.OSPF);
        if (ospfState == null)
        {
            return Success("OSPF is not configured");
        }
        
        var output = new StringBuilder();
        output.AppendLine("OSPF Router Information:");
        output.AppendLine($"Router ID: {ospfState.RouterId}");
        output.AppendLine($"Areas: {ospfState.Areas?.Count ?? 0}");
        output.AppendLine($"Neighbors: {ospfState.Neighbors?.Count ?? 0}");
        output.AppendLine($"Last Update: {ospfState.LastUpdate}");
        
        if (ospfState.Neighbors?.Any() == true)
        {
            output.AppendLine("\nNeighbors:");
            foreach (var neighbor in ospfState.Neighbors.Values)
            {
                output.AppendLine($"  {neighbor.RouterId} - State: {neighbor.State}");
            }
        }
        
        return Success(output.ToString());
    }
}
```

## Implementation Steps

### Phase 1: Foundation (Week 1-2)
1. **Create Common Library**
   - Implement `NetSim.Simulation.Protocols.Common` project
   - Define core interfaces and base classes
   - Implement protocol discovery service
   - Add IoC container support

2. **Enhance NetworkDevice**
   - Add IoC container setup
   - Implement auto-protocol registration
   - Add protocol service for CLI handlers
   - Enhance command processing for Telnet sessions

### Phase 2: Telnet Protocol (Week 2-3)
1. **Implement Telnet Protocol**
   - Create `NetSim.Simulation.Protocols.Telnet` project
   - Implement TCP server and session management
   - Add authentication system
   - Integrate with CLI handlers

2. **Test Telnet Integration**
   - External Telnet client connectivity
   - CLI command routing
   - Session management
   - Authentication flow

### Phase 3: Migrate Existing Protocols (Week 3-5)
1. **Create Individual Protocol Projects**
   - Move existing protocols to separate projects
   - Implement plugin interfaces
   - Maintain existing state management patterns
   - Add vendor-specific support

2. **Update Device Classes**
   - Remove hardcoded protocol registration
   - Update configuration methods
   - Test protocol auto-discovery

### Phase 4: Enhanced CLI Integration (Week 5-6)
1. **Update CLI Handlers**
   - Add protocol service injection
   - Update handlers to use protocol state
   - Test `show` commands with protocol data
   - Implement protocol-specific commands

2. **Testing and Validation**
   - Comprehensive protocol testing
   - CLI handler integration testing
   - Performance validation
   - State management verification

## Key Benefits

### 1. **Modularity**
- Each protocol is completely self-contained
- Independent development and testing
- Selective protocol deployment

### 2. **State Management Integration**
- Incorporates proven state management patterns
- Performance optimized with conditional execution
- Automatic neighbor cleanup and timer management

### 3. **Vendor-Specific Support**
- Protocols can be vendor-specific (EIGRP for Cisco)
- Priority-based protocol selection
- Vendor-aware protocol discovery

### 4. **CLI Handler Integration**
- Clean IoC/DI pattern for protocol access
- Type-safe protocol state access
- Seamless integration with existing CLI system

### 5. **Realistic Network Simulation**
- Telnet protocol provides actual network management interface
- External tools can connect to simulated devices
- True network-based device management

### 6. **Performance Optimized**
- State change detection prevents unnecessary calculations
- Neighbor aging prevents memory leaks
- Conditional protocol execution

### 7. **Extensible Architecture**
- Easy to add new protocols without touching existing code
- Plugin-based discovery similar to CLI handlers
- Clean separation of concerns

## Migration Strategy

### Backward Compatibility
- Existing protocol implementations continue to work
- Gradual migration per protocol
- No breaking changes to NetworkDevice API

### Testing Strategy
- Unit tests for each protocol project
- Integration tests for protocol discovery
- CLI handler integration tests
- Performance benchmarking

### Documentation
- Update existing `PROTOCOL_STATE_MANAGEMENT.md`
- Create per-protocol documentation
- Add CLI handler integration examples
- Performance optimization guidelines

## Conclusion

This implementation plan provides a comprehensive roadmap for transforming the NetSim protocol architecture into a modular, plugin-based system that maintains the sophisticated state management patterns while adding vendor-specific support, IoC integration, and realistic network management via Telnet protocol.

The architecture follows established patterns from the CLI handler system while being appropriately simplified for protocol use cases, ensuring consistency across the codebase and maximum extensibility for future development.