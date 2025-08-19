# Protocol Implementation Plan

## Overview

This document outlines the comprehensive plan for implementing a modular, plugin-based protocol architecture for the NetForge network simulation system. The architecture is designed to provide maximum modularity, vendor-specific protocol support, and seamless integration with the existing CLI handler system.

## Current State Analysis

### Existing CLI Handler Architecture
- **Interface**: `ICliHandler` at `NetForge.Simulation.Common\CLI\Interfaces\ICliHandler.cs:8`
- **Base Class**: `BaseCliHandler` at `NetForge.Simulation.Common\CLI\Base\BaseCliHandler.cs:8`
- **Registry Pattern**: `VendorHandlerRegistryBase` with auto-discovery via reflection
- **Vendor-Specific**: Handler registries per vendor (e.g., `CiscoHandlerRegistry.cs:9`)
- **Priority-Based**: Higher priority handlers loaded first

### Current Protocol Architecture
- **Interface**: `INetworkProtocol` at `NetForge.Simulation.Common\Interfaces\INetworkProtocol.cs:23`
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
NetForge.Simulation.Protocols.Common\              # Core interfaces and base classes
├── NetForge.Simulation.Protocols.Common.csproj
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

NetForge.Simulation.Protocols.Telnet\              # Management protocol
NetForge.Simulation.Protocols.OSPF\                # Layer 3 routing
NetForge.Simulation.Protocols.BGP\                 # Layer 3 routing
NetForge.Simulation.Protocols.EIGRP\               # Layer 3 routing (Cisco-specific)
NetForge.Simulation.Protocols.RIP\                 # Layer 3 routing
NetForge.Simulation.Protocols.ISIS\                # Layer 3 routing
NetForge.Simulation.Protocols.IGRP\                # Layer 3 routing (Cisco-specific)
NetForge.Simulation.Protocols.CDP\                 # Layer 2 discovery (Cisco-specific)
NetForge.Simulation.Protocols.LLDP\                # Layer 2 discovery
NetForge.Simulation.Protocols.STP\                 # Layer 2 switching
NetForge.Simulation.Protocols.HSRP\                # Layer 3 redundancy (Cisco-specific)
NetForge.Simulation.Protocols.VRRP\                # Layer 3 redundancy
NetForge.Simulation.Protocols.ARP\                 # Layer 3 network
```

### 2. Core Common Library Implementation

#### Enhanced Protocol Interface

```csharp
// NetForge.Simulation.Protocols.Common/Interfaces/INetworkProtocol.cs
namespace NetForge.Simulation.Protocols.Common
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
// NetForge.Simulation.Protocols.Common/Base/BaseProtocol.cs
namespace NetForge.Simulation.Protocols.Common
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
// NetForge.Simulation.Protocols.Common/Services/ProtocolDiscoveryService.cs
namespace NetForge.Simulation.Protocols.Common.Services
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
                           (a.FullName?.Contains("NetForge.Simulation.Protocols.") ?? false) &&
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
// NetForge.Simulation.Protocols.Telnet/TelnetProtocol.cs
namespace NetForge.Simulation.Protocols.Telnet
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
// NetForge.Simulation.Protocols.OSPF/OspfProtocol.cs
namespace NetForge.Simulation.Protocols.OSPF
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

    // NetForge.Simulation.Protocols.OSPF/Configuration/OspfState.cs
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
   - Implement `NetForge.Simulation.Protocols.Common` project
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
   - Create `NetForge.Simulation.Protocols.Telnet` project
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

## Phase 5: Migration from Legacy Protocols

### Migration Strategy Overview

With the new protocol architecture foundation complete (as tracked in [PROTOCOL_IMPLEMENTATION_STATUS.md](PROTOCOL_IMPLEMENTATION_STATUS.md)), we need a systematic approach to migrate from the legacy protocol implementations in `NetForge.Simulation.Common` to the new plugin-based architecture.

### Current Legacy Protocol Status

#### Protocols Already in Common Project
The following legacy protocols exist in `NetForge.Simulation.Common` and need migration:

**Routing Protocols:**
- `OspfConfig`, `BgpConfig`, `RipConfig`, `EigrpConfig`, `IsisConfig`, `IgrpConfig`
- Basic configuration classes with limited functionality
- Hardcoded in `NetworkDevice` class with direct field access

**Redundancy Protocols:**
- `VrrpConfig`, `HsrpConfig`
- Basic configuration with minimal state management

**Discovery Protocols:**
- `CdpConfig`, `LldpConfig`
- Simple configuration classes

**Layer 2 Protocols:**
- `StpConfig` (Spanning Tree Protocol)
- Basic spanning tree implementation

**Network Protocols:**
- ARP functionality embedded in `NetworkDevice`
- Route management embedded in device

### Migration Phases

#### Phase 5.1: Assessment and Preparation (CURRENT STATUS: READY)
- ✅ **Foundation Complete**: New protocol architecture fully implemented
- ✅ **Example Protocol**: Telnet protocol demonstrates full pattern
- ✅ **Integration Points**: NetworkDevice enhanced for new protocols
- ⏳ **Legacy Audit**: Complete assessment of existing functionality needed

**Action Items:**
1. Document exact functionality of each legacy protocol
2. Identify dependencies between legacy protocols
3. Map legacy configuration to new configuration patterns
4. Create migration priority matrix based on complexity and usage

#### Phase 5.2: Compatibility Layer (IMMEDIATE NEXT STEP)
Create temporary bridges to ensure existing functionality continues during migration.

**Compatibility Strategy:**
```csharp
// Create compatibility wrapper in NetworkDevice
public void SetOspfConfiguration(OspfConfig config)
{
    // Legacy support
    bool wasNull = OspfConfig == null;
    OspfConfig = config;
    
    // New architecture integration
    var newProtocol = GetRegisteredProtocols()
        .FirstOrDefault(p => p.Type == ProtocolType.OSPF);
    
    if (newProtocol != null)
    {
        // Convert legacy config to new format and apply
        var newConfig = ConvertLegacyOspfConfig(config);
        newProtocol.ApplyConfiguration(newConfig);
    }
    
    ParentNetwork?.EventBus?.PublishAsync(new Events.ProtocolConfigChangedEventArgs(
        Name, ProtocolType.OSPF, wasNull ? "OSPF configuration initialized" : "OSPF configuration updated"));
}

private object ConvertLegacyOspfConfig(OspfConfig legacyConfig)
{
    // Convert legacy configuration to new architecture format
    return new NetForge.Simulation.Protocols.OSPF.OspfConfig
    {
        IsEnabled = legacyConfig.IsEnabled,
        RouterId = legacyConfig.RouterId,
        Areas = ConvertAreas(legacyConfig.Areas),
        // ... other conversions
    };
}
```

#### Phase 5.3: Protocol-by-Protocol Migration

**Migration Priority Order:**

1. **High Priority - Management Protocols**
   - SSH (new implementation, high value)
   - SNMP (new implementation, monitoring critical)
   
2. **High Priority - Core Routing**
   - OSPF (complex migration, high usage)
   - BGP (complex migration, critical for enterprise)
   
3. **Medium Priority - Discovery**
   - CDP (simple migration, vendor-specific)
   - LLDP (simple migration, standard)
   - ARP (refactor from embedded logic)
   
4. **Medium Priority - Redundancy**
   - VRRP (moderate complexity)
   - HSRP (moderate complexity, Cisco-specific)
   
5. **Lower Priority - Legacy Routing**
   - RIP (simple migration)
   - EIGRP (Cisco-specific)
   - IS-IS (complex, lower usage)
   - IGRP (legacy, minimal usage)

6. **Infrastructure Protocols**
   - STP (embedded in device logic, complex extraction)

**Per-Protocol Migration Process:**

1. **Analysis Phase**
   - Document current behavior and configuration options
   - Identify all integration points with NetworkDevice
   - Map CLI commands that depend on the protocol
   - Create test scenarios for validation

2. **Implementation Phase**
   - Create new protocol project following Telnet pattern
   - Implement protocol logic using new architecture
   - Create configuration conversion utilities
   - Add new CLI integration points

3. **Integration Phase**
   - Add compatibility layer for seamless transition
   - Update protocol discovery to include new implementation
   - Ensure legacy configurations continue to work
   - Add dual-mode operation support

4. **Testing Phase**
   - Unit tests for new protocol implementation
   - Integration tests with existing systems
   - Performance comparison with legacy implementation
   - CLI command compatibility verification

5. **Migration Phase**
   - Gradual cutover from legacy to new implementation
   - Configuration migration tools for existing deployments
   - Monitoring and rollback capabilities
   - Documentation updates

6. **Cleanup Phase**
   - Remove legacy implementation after validation
   - Clean up compatibility layer
   - Update documentation to reflect new architecture
   - Performance optimization

#### Phase 5.4: Advanced Migration Features

**Configuration Migration Tools:**
```csharp
public class ProtocolMigrationService
{
    public async Task<MigrationResult> MigrateProtocol(ProtocolType protocolType, NetworkDevice device)
    {
        var migrator = GetMigrator(protocolType);
        return await migrator.MigrateAsync(device);
    }
    
    public bool CanMigrate(ProtocolType protocolType)
    {
        return _migrators.ContainsKey(protocolType);
    }
    
    public ValidationResult ValidateMigration(ProtocolType protocolType, NetworkDevice device)
    {
        // Validate that migration is safe and complete
    }
}
```

**State Migration:**
- Preserve neighbor relationships during migration
- Maintain routing tables during transition
- Ensure continuous protocol operation
- Rollback capabilities for failed migrations

#### Phase 5.5: Testing and Validation Framework

**Migration Testing Strategy:**
```csharp
[TestCategory("ProtocolMigration")]
public class OspfMigrationTests
{
    [Test]
    public async Task MigrateOspf_PreservesNeighborRelationships()
    {
        // Arrange: Set up legacy OSPF with neighbors
        // Act: Migrate to new architecture
        // Assert: Neighbors preserved, routing table intact
    }
    
    [Test]
    public async Task MigrateOspf_ConfigurationCompatibility()
    {
        // Test legacy configuration still works
    }
    
    [Test]
    public async Task MigrateOspf_PerformanceComparison()
    {
        // Compare performance before/after migration
    }
}
```

**Validation Metrics:**
- Configuration preservation (100% compatibility required)
- State preservation (neighbor tables, routes, timers)
- Performance parity or improvement
- CLI command compatibility
- Memory usage optimization

### Migration Timeline

**Phase 5.1 - Assessment** (1-2 weeks)
- ✅ Complete foundation (already done)
- ⏳ Document legacy protocols
- ⏳ Create migration framework

**Phase 5.2 - Compatibility Layer** (1 week)
- Create legacy-to-new configuration converters
- Add dual-mode operation support
- Implement rollback mechanisms

**Phase 5.3 - Protocol Migration** (8-12 weeks)
- Week 1-2: SSH + SNMP (new implementations)
- Week 3-4: OSPF migration
- Week 5-6: BGP migration  
- Week 7-8: Discovery protocols (CDP, LLDP, ARP)
- Week 9-10: Redundancy protocols (VRRP, HSRP)
- Week 11-12: Legacy routing protocols (RIP, EIGRP, etc.)

**Phase 5.4 - Advanced Features** (2-3 weeks)
- Migration automation tools
- Performance optimization
- Advanced monitoring and diagnostics

**Phase 5.5 - Cleanup and Documentation** (1-2 weeks)
- Remove legacy implementations
- Update documentation
- Final testing and validation

### Success Criteria

**Functional Requirements:**
- ✅ All existing protocol functionality preserved
- ✅ Configuration compatibility maintained
- ✅ CLI commands continue to work unchanged
- ✅ Network connectivity uninterrupted during migration

**Architecture Requirements:**
- ✅ Modular protocol implementations
- ✅ Plugin-based discovery and loading
- ✅ Vendor-specific protocol support
- ✅ Enhanced state management and monitoring

**Performance Requirements:**
- ✅ Memory usage reduced or maintained
- ✅ Protocol convergence time improved or maintained
- ✅ CPU usage optimized
- ✅ Network overhead minimized

**Operational Requirements:**
- ✅ Zero-downtime migration capability
- ✅ Rollback mechanisms for failed migrations
- ✅ Configuration backup and restore
- ✅ Migration validation and testing tools

### Risk Mitigation

**High-Risk Items:**
1. **State Loss During Migration**
   - Mitigation: Comprehensive state preservation mechanisms
   - Testing: Automated state comparison before/after migration

2. **Performance Regression**
   - Mitigation: Performance monitoring and benchmarking
   - Testing: Load testing with realistic network scenarios

3. **Configuration Incompatibility**
   - Mitigation: Extensive configuration conversion testing
   - Testing: Migration testing with diverse configuration scenarios

4. **CLI Command Breakage**
   - Mitigation: CLI compatibility layer and thorough testing
   - Testing: Automated CLI command regression testing

**Medium-Risk Items:**
1. **Plugin Discovery Issues**
   - Mitigation: Robust error handling and fallback mechanisms
   
2. **Vendor-Specific Feature Loss**
   - Mitigation: Careful analysis and feature mapping per vendor

3. **Network Convergence Delays**
   - Mitigation: Optimized protocol implementation and timing

### Migration Support Tools

**Automated Migration Scripts:**
- Configuration analysis and conversion tools
- State backup and restore utilities
- Migration validation and testing scripts
- Performance comparison and monitoring tools

**Monitoring and Diagnostics:**
- Real-time migration progress tracking
- Protocol state comparison tools
- Performance impact monitoring
- Error detection and rollback triggers

**Documentation and Training:**
- Migration guide for network administrators
- Troubleshooting guide for common migration issues
- Best practices for protocol configuration in new architecture
- Training materials for new CLI integration features

## Phase 6: Remaining Protocol Implementation Strategy

### Current Implementation Status (As of January 2025)

Based on recent implementation progress, the following protocols have been **COMPLETED**:
- ✅ **SSH Protocol**: Full implementation with encryption and secure session management
- ✅ **OSPF Protocol**: Complete link-state routing with SPF calculation and topology database
- ✅ **BGP Protocol**: Full BGP-4 with best path selection and IBGP/EBGP support

### Recommended Implementation Sequence for Remaining Protocols

#### **Phase 6.1: High-Value Discovery Protocols** (Recommended NEXT - 2-3 weeks)

These protocols provide immediate network visibility and are relatively simple to implement:

**1. CDP (Cisco Discovery Protocol) - Priority: HIGH**
```csharp
// Example implementation structure
namespace NetForge.Simulation.Protocols.CDP
{
    public class CdpProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.CDP;
        public override string Name => "Cisco Discovery Protocol";
        
        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            // Broadcast CDP advertisements every 60 seconds
            // Listen for CDP advertisements from neighbors
            // Update neighbor cache with device info
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            // Process received CDP advertisements
            // Update device neighbor database
            // Clean up aged out neighbors
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco" }; // Cisco-specific
        }
    }
}
```

**CLI Integration for CDP:**
```csharp
public class ShowCdpNeighborsHandler : BaseCliHandler
{
    public ShowCdpNeighborsHandler() : base("neighbors", "Show CDP neighbors") { }
    
    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
    {
        var protocolService = context.GetProtocolService();
        var cdpState = protocolService.GetProtocolState<CdpState>(ProtocolType.CDP);
        
        if (cdpState == null || !protocolService.IsProtocolActive(ProtocolType.CDP))
        {
            return Success("CDP is not enabled");
        }
        
        var output = new StringBuilder();
        output.AppendLine("Capability Codes: R - Router, T - Trans Bridge, B - Source Route Bridge");
        output.AppendLine("                  S - Switch, H - Host, I - IGMP, r - Repeater");
        output.AppendLine();
        output.AppendLine("Device ID        Local Intrfce     Holdtme    Capability  Platform  Port ID");
        
        foreach (var neighbor in cdpState.Neighbors.Values)
        {
            output.AppendLine($"{neighbor.DeviceId,-16} {neighbor.LocalInterface,-13} {neighbor.HoldTime,-10} {neighbor.Capabilities,-11} {neighbor.Platform,-9} {neighbor.PortId}");
        }
        
        return Success(output.ToString());
    }
}
```

**2. LLDP (Link Layer Discovery Protocol) - Priority: HIGH**
- Similar to CDP but standards-based
- Supported by all major vendors
- More detailed neighbor information

**3. ARP (Address Resolution Protocol) - Priority: HIGH**
- Extract from embedded NetworkDevice logic
- Create proper protocol with ARP table management
- Essential for IP-to-MAC mapping

#### **Phase 6.2: Management Protocols** (3-4 weeks)

**1. SNMP (Simple Network Management Protocol) - Priority: HIGH**
```csharp
namespace NetForge.Simulation.Protocols.SNMP
{
    public class SnmpProtocol : BaseProtocol
    {
        private SnmpAgent _snmpAgent;
        private readonly Dictionary<string, SnmpVariable> _mibDatabase = new();
        
        protected override void OnInitialized()
        {
            var snmpConfig = GetSnmpConfig();
            if (snmpConfig.IsEnabled)
            {
                InitializeMibDatabase();
                StartSnmpAgent(snmpConfig);
            }
        }
        
        private void InitializeMibDatabase()
        {
            // Populate MIB with device information
            _mibDatabase["1.3.6.1.2.1.1.1.0"] = new SnmpVariable("sysDescr", _device.Description);
            _mibDatabase["1.3.6.1.2.1.1.3.0"] = new SnmpVariable("sysUpTime", GetSystemUpTime());
            _mibDatabase["1.3.6.1.2.1.1.5.0"] = new SnmpVariable("sysName", _device.Hostname);
            // Add interface counters, routing table, etc.
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            // Update MIB variables with current device state
            await UpdateMibDatabase(device);
            
            // Process any pending SNMP requests
            await ProcessSnmpRequests();
        }
    }
}
```

**CLI Integration for SNMP:**
```csharp
public class ShowSnmpHandler : BaseCliHandler
{
    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
    {
        var protocolService = context.GetProtocolService();
        var snmpState = protocolService.GetProtocolState<SnmpState>(ProtocolType.SNMP);
        
        var output = new StringBuilder();
        output.AppendLine("SNMP Agent Information:");
        output.AppendLine($"Agent Status: {(snmpState?.IsActive == true ? "Enabled" : "Disabled")}");
        output.AppendLine($"Community Strings: {snmpState?.Communities?.Count ?? 0}");
        output.AppendLine($"Total Requests: {snmpState?.TotalRequests ?? 0}");
        output.AppendLine($"Total Responses: {snmpState?.TotalResponses ?? 0}");
        
        return Success(output.ToString());
    }
}
```

**2. SNMP (Simple Network Management Protocol) - Priority: HIGH**
```csharp
namespace NetForge.Simulation.Protocols.SNMP
{
    public class SnmpProtocol : BaseProtocol
    {
        private SnmpAgent _snmpAgent;
        private readonly Dictionary<string, SnmpVariable> _mibDatabase = new();
        
        protected override void OnInitialized()
        {
            var snmpConfig = GetSnmpConfig();
            if (snmpConfig.IsEnabled)
            {
                InitializeMibDatabase();
                StartSnmpAgent(snmpConfig);
            }
        }
        
        private void InitializeMibDatabase()
        {
            // Populate MIB with device information
            _mibDatabase["1.3.6.1.2.1.1.1.0"] = new SnmpVariable("sysDescr", _device.Description);
            _mibDatabase["1.3.6.1.2.1.1.3.0"] = new SnmpVariable("sysUpTime", GetSystemUpTime());
            _mibDatabase["1.3.6.1.2.1.1.5.0"] = new SnmpVariable("sysName", _device.Hostname);
            // Add interface counters, routing table, etc.
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            // Update MIB variables with current device state
            await UpdateMibDatabase(device);
            
            // Process any pending SNMP requests via SNMP Handlers
            await ProcessSnmpRequests();
        }
    }
}
```

**SNMP Handler Integration:**
SNMP protocol requires specialized SNMP handlers similar to CLI handlers but for SNMP protocol operations. These handlers will be implemented per vendor to support vendor-specific MIB extensions and SNMP behaviors.

**Architecture Pattern:**
```csharp
public class CiscoSnmpHandler : BaseSnmpHandler
{
    protected override async Task<SnmpResult> HandleGetRequest(SnmpRequest request)
    {
        // Cisco-specific SNMP GET handling
        // Support for Cisco private MIBs (1.3.6.1.4.1.9.x.x.x)
        return await ProcessCiscoSpecificOids(request);
    }
    
    protected override async Task<SnmpResult> HandleSetRequest(SnmpRequest request)
    {
        // Cisco-specific SNMP SET handling with validation
        return await ProcessCiscoConfiguration(request);
    }
}
```

**Implementation Plan:**
- SNMP Protocol implementation following the established pattern
- Integration with dedicated SNMP Handler architecture
- Vendor-specific SNMP handler implementations
- MIB database management with standard and private MIBs
- SNMP agent simulation with GET/SET/GETNEXT/GETBULK support
- Trap/notification generation for network events

**Integration with SNMP Handlers:**
The SNMP protocol will integrate with the SNMP Handler system (similar to CLI Handler integration) to provide:
- Vendor-specific MIB support
- Custom OID handling per vendor
- SNMP community string management
- Trap destination configuration
- Performance monitoring via SNMP

For detailed SNMP Handler implementation, see: [SNMP_HANDLERS_IMPLEMENTATION_PLAN.md](../NetForge.Simulation.SnmpHandlers/SNMP_HANDLERS_IMPLEMENTATION_PLAN.md)

**3. HTTP/HTTPS (Web Management Interface) - Priority: MEDIUM**
- Basic web server for device management
- REST API for configuration
- Web-based monitoring interface

#### **Phase 6.3: Routing Protocols Migration** (4-5 weeks)

**1. RIP (Routing Information Protocol) - Priority: MEDIUM**
```csharp
namespace NetForge.Simulation.Protocols.RIP
{
    public class RipProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.RIP;
        public override string Name => "Routing Information Protocol";
        
        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            var ripConfig = GetRipConfig();
            
            // Send RIP updates every 30 seconds
            if (ShouldSendUpdate())
            {
                await SendRipUpdates(device, ripConfig);
            }
            
            // Process received RIP updates
            await ProcessReceivedUpdates(device);
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var ripState = (RipState)_state;
            
            // Remove timed out routes
            RemoveTimedOutRoutes(ripState);
            
            // Install valid routes
            await InstallRipRoutes(device, ripState);
        }
        
        private async Task SendRipUpdates(NetworkDevice device, RipConfig config)
        {
            foreach (var interfaceName in config.Networks)
            {
                var ripPacket = CreateRipUpdate(device, interfaceName);
                await BroadcastRipUpdate(ripPacket, interfaceName);
            }
        }
    }
}
```

**CLI Integration for RIP:**
```csharp
public class ShowIpRipHandler : BaseCliHandler
{
    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
    {
        var protocolService = context.GetProtocolService();
        var ripState = protocolService.GetProtocolState<RipState>(ProtocolType.RIP);
        
        var output = new StringBuilder();
        output.AppendLine("RIP Routing Table:");
        output.AppendLine("Network          Next Hop         Metric Interface");
        
        foreach (var route in ripState.Routes.Values)
        {
            output.AppendLine($"{route.Network,-16} {route.NextHop,-16} {route.Metric,-6} {route.Interface}");
        }
        
        return Success(output.ToString());
    }
}
```

**2. EIGRP (Enhanced Interior Gateway Routing Protocol) - Priority: MEDIUM**
- Cisco proprietary protocol
- DUAL algorithm implementation
- Advanced metrics (bandwidth, delay, reliability)

#### **Phase 6.4: Redundancy Protocols** (3-4 weeks)

**1. HSRP (Hot Standby Router Protocol) - Priority: MEDIUM**
```csharp
namespace NetForge.Simulation.Protocols.HSRP
{
    public class HsrpProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.HSRP;
        public override string Name => "Hot Standby Router Protocol";
        
        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            var hsrpConfig = GetHsrpConfig();
            
            foreach (var group in hsrpConfig.Groups.Values)
            {
                // Send HSRP hello messages
                await SendHsrpHello(device, group);
                
                // Process received HSRP messages
                await ProcessHsrpMessages(device, group);
                
                // Update HSRP state machine
                await UpdateHsrpStateMachine(device, group);
            }
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var hsrpState = (HsrpState)_state;
            
            foreach (var group in hsrpState.Groups.Values)
            {
                // Determine active/standby routers
                await ElectActiveRouter(group);
                
                // Update virtual MAC address
                UpdateVirtualMacAddress(group);
                
                // Install/remove virtual IP routes
                await ManageVirtualIpRoutes(device, group);
            }
        }
    }
}
```

**2. VRRP (Virtual Router Redundancy Protocol) - Priority: MEDIUM**
- Standards-based alternative to HSRP
- RFC 3768 compliance
- Multi-vendor support

**3. STP (Spanning Tree Protocol) - Priority: MEDIUM**
- Layer 2 loop prevention
- Bridge protocol data units (BPDUs)
- Port states and roles

#### **Phase 6.5: Advanced and Legacy Protocols** (2-3 weeks)

**1. IS-IS (Intermediate System to Intermediate System) - Priority: LOW**
- Link-state routing protocol
- OSI-based addressing
- Areas and levels

**2. IGRP (Interior Gateway Routing Protocol) - Priority: LOW**
- Legacy Cisco protocol
- Distance vector with composite metrics
- Minimal modern usage

### Enhanced CLI Handler Integration Strategy

#### **1. Protocol-Aware CLI Context**
```csharp
public class ProtocolAwareCliContext : CliContext
{
    public T GetProtocolState<T>(ProtocolType type) where T : class
    {
        return GetProtocolService()?.GetProtocolState<T>(type);
    }
    
    public bool IsProtocolEnabled(ProtocolType type)
    {
        return GetProtocolService()?.IsProtocolActive(type) ?? false;
    }
    
    public IEnumerable<ProtocolType> GetActiveProtocols()
    {
        return GetProtocolService()?.GetAllProtocols()?.Select(p => p.Type) ?? Array.Empty<ProtocolType>();
    }
}
```

#### **2. Unified Show Commands**
```csharp
public class ShowProtocolsHandler : BaseCliHandler
{
    public ShowProtocolsHandler() : base("protocols", "Show all configured protocols") { }
    
    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
    {
        var protocolService = context.GetProtocolService();
        var protocols = protocolService.GetAllProtocols();
        
        var output = new StringBuilder();
        output.AppendLine("Configured Protocols:");
        output.AppendLine("Protocol        Status    Neighbors  Last Update");
        output.AppendLine("--------------- --------- ---------- -------------------");
        
        foreach (var protocol in protocols.OrderBy(p => p.Type))
        {
            var state = protocol.GetState();
            var neighborCount = GetNeighborCount(state);
            
            output.AppendLine($"{protocol.Type,-15} {(state.IsActive ? "Active" : "Inactive"),-9} {neighborCount,-10} {state.LastUpdate:MM/dd/yyyy HH:mm:ss}");
        }
        
        return Success(output.ToString());
    }
}
```

#### **3. Protocol Configuration Handlers**
```csharp
public class RouterProtocolHandler : BaseCliHandler
{
    public RouterProtocolHandler() : base("router", "Configure routing protocols") { }
    
    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
    {
        if (context.CommandParts.Length < 2)
        {
            return Error(CliErrorType.InvalidArgument, "Specify protocol: ospf, bgp, rip, eigrp");
        }
        
        var protocolName = context.CommandParts[1].ToLower();
        
        return protocolName switch
        {
            "ospf" => await EnterOspfConfigurationMode(context),
            "bgp" => await EnterBgpConfigurationMode(context),
            "rip" => await EnterRipConfigurationMode(context),
            "eigrp" => await EnterEigrpConfigurationMode(context),
            _ => Error(CliErrorType.InvalidArgument, $"Unknown protocol: {protocolName}")
        };
    }
    
    private async Task<CliResult> EnterOspfConfigurationMode(CliContext context)
    {
        var protocolService = context.GetProtocolService();
        var ospfProtocol = protocolService.GetProtocol(ProtocolType.OSPF);
        
        if (ospfProtocol == null)
        {
            return Error(CliErrorType.ExecutionError, "OSPF protocol not available");
        }
        
        // Switch to OSPF configuration mode
        return Success("Entering OSPF configuration mode", DeviceMode.OspfConfig);
    }
}
```

### Implementation Timeline and Resource Allocation

#### **Phase 6.1: Discovery Protocols** (Weeks 1-3)
- **Week 1**: CDP implementation and testing
- **Week 2**: LLDP implementation and testing
- **Week 3**: ARP protocol extraction and refactoring

**Resource Requirements:**
- 1 Senior Developer (protocol implementation)
- 1 Junior Developer (testing and documentation)
- 0.5 QA Engineer (integration testing)

#### **Phase 6.2: Management Protocols** (Weeks 4-7)
- **Week 4-5**: SNMP agent implementation
- **Week 6-7**: HTTP/HTTPS web interface

**Resource Requirements:**
- 1 Senior Developer (SNMP/HTTP implementation)
- 1 Network Engineer (MIB design and testing)
- 0.5 Frontend Developer (web interface)

#### **Phase 6.3: Routing Protocols** (Weeks 8-12)
- **Week 8-9**: RIP implementation and migration
- **Week 10-12**: EIGRP implementation and testing

**Resource Requirements:**
- 1 Senior Developer (routing protocol expertise)
- 1 Network Engineer (protocol validation)
- 1 QA Engineer (comprehensive testing)

#### **Phase 6.4: Redundancy Protocols** (Weeks 13-16)
- **Week 13-14**: HSRP implementation
- **Week 15**: VRRP implementation
- **Week 16**: STP implementation

#### **Phase 6.5: Legacy Protocols** (Weeks 17-19)
- **Week 17-18**: IS-IS implementation
- **Week 19**: IGRP implementation and cleanup

### Quality Assurance and Testing Strategy

#### **Per-Protocol Testing Requirements**
1. **Unit Tests**: Protocol logic and state management
2. **Integration Tests**: CLI handler integration
3. **Performance Tests**: Memory usage and convergence time
4. **Compatibility Tests**: Legacy configuration migration
5. **Stress Tests**: Large-scale network scenarios

#### **CLI Handler Testing**
```csharp
[TestCategory("ProtocolCliIntegration")]
public class ProtocolCliTests
{
    [Test]
    public async Task ShowCdpNeighbors_WhenCdpEnabled_ReturnsNeighborList()
    {
        // Arrange
        var device = CreateTestDevice();
        var cdpProtocol = new CdpProtocol();
        device.RegisterProtocol(cdpProtocol);
        
        // Add test neighbors
        var cdpState = cdpProtocol.GetState() as CdpState;
        cdpState.Neighbors["neighbor1"] = CreateTestNeighbor();
        
        // Act
        var result = await ExecuteCommand(device, "show cdp neighbors");
        
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Output, Contains.Substring("neighbor1"));
    }
}
```

#### **Performance Benchmarks**
- Protocol convergence time < 30 seconds for OSPF/BGP
- Memory usage per protocol < 10MB baseline
- CLI response time < 100ms for show commands
- Network update processing < 1000 updates/second

### Risk Mitigation and Rollback Strategy

#### **Implementation Risks**
1. **Protocol Complexity**: Phased implementation with extensive testing
2. **CLI Integration Issues**: Comprehensive integration testing
3. **Performance Impact**: Continuous performance monitoring
4. **Legacy Compatibility**: Parallel operation during migration

#### **Rollback Mechanisms**
1. **Protocol-Level Rollback**: Disable new protocols, re-enable legacy
2. **Configuration Rollback**: Restore previous configurations
3. **State Preservation**: Maintain neighbor relationships during rollback
4. **Monitoring Alerts**: Automated detection of implementation issues

## Conclusion

This comprehensive implementation strategy provides a clear roadmap for completing the remaining protocol implementations while maintaining the highest standards for CLI integration, performance, and reliability.

The recommended prioritization focuses on high-value protocols that provide immediate network visibility and management capabilities, followed by systematic migration of existing routing and redundancy protocols.

With the foundation phase complete (SSH, OSPF, BGP) and the established patterns proven, the remaining protocol implementations can proceed with confidence in the architecture's scalability and maintainability.

The emphasis on CLI handler integration ensures that each protocol implementation provides immediate value through comprehensive show commands, configuration interfaces, and troubleshooting capabilities, making the NetForge platform a complete network simulation and management solution.