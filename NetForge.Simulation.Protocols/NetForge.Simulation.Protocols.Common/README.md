# NetForge.Simulation.Protocols.Common

This library provides the core infrastructure for the modular protocol architecture in NetForge. It implements the foundation for plugin-based protocol discovery, state management, and integration with CLI handlers.

## Overview

The common library follows the sophisticated state management patterns documented in `COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md` while providing a plugin-based architecture similar to the CLI handler system.

## Key Components

### Interfaces

- **`IDeviceProtocol`** - Unified comprehensive protocol interface (merged from IDeviceProtocol and IEnhancedDeviceProtocol)
- **`INetworkProtocol`** - Legacy protocol interface maintained for backward compatibility
- **`IProtocolState`** - Protocol state management interface with neighbor tracking
- **`IProtocolPlugin`** - Plugin interface for auto-discovery of protocols
- **`IProtocolService`** - Service interface for CLI handlers to access protocol state via IoC/DI

### Base Classes

- **`BaseProtocol`** - Base implementation with state management pattern from `COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md`
- **`BaseProtocolState`** - Base state class with neighbor management and change tracking
- **`ProtocolPluginBase`** - Base plugin class for easy protocol plugin creation

### Services

- **`ProtocolDiscoveryService`** - Auto-discovery of protocol plugins from assemblies
- **`NetworkDeviceProtocolService`** - IoC service for CLI handlers to access protocols

### Events

- **`ProtocolStateChangedEventArgs`** - Event for protocol state changes
- **`ProtocolNeighborChangedEventArgs`** - Event for neighbor discovery/loss/changes

## Key Features

### 1. State Management Pattern

Implements the proven pattern from `COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md`:

```csharp
public virtual async Task UpdateState(NetworkDevice device)
{
    // Always update neighbors and timers
    await UpdateNeighbors(device);
    await CleanupStaleNeighbors(device);
    await ProcessTimers(device);
    
    // Only run expensive operations if state changed
    if (_state.StateChanged)
    {
        await RunProtocolCalculation(device);
        _state.StateChanged = false;
        _state.LastUpdate = DateTime.Now;
    }
}
```

### 2. Plugin-Based Discovery

Similar to CLI handler discovery:

```csharp
public class MyProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "My Protocol Plugin";
    public override ProtocolType ProtocolType => ProtocolType.OSPF;
    public override IDeviceProtocol CreateProtocol() => new MyProtocol();
}
```

### 3. Vendor-Specific Support

Protocols can support specific vendors with priority:

```csharp
public override IEnumerable<string> GetSupportedVendors()
{
    return new[] { "Cisco" }; // EIGRP is Cisco-specific
}

public override int Priority => 200; // Higher than generic
```

### 4. CLI Handler Integration

CLI handlers can access protocol state via IoC:

```csharp
protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
{
    var protocolService = context.GetProtocolService();
    var ospfState = protocolService.GetProtocolState<OspfState>(ProtocolType.OSPF);
    
    // Use protocol state for command output
    return Success($"OSPF Router ID: {ospfState.RouterId}");
}
```

## Usage

### Creating a New Protocol

1. **Create the Protocol Implementation:**

```csharp
public class MyProtocol : BaseProtocol
{
    public override ProtocolType Type => ProtocolType.OSPF;
    public override string Name => "My Protocol";
    
    protected override BaseProtocolState CreateInitialState()
    {
        return new MyProtocolState();
    }
    
    protected override async Task RunProtocolCalculation(NetworkDevice device)
    {
        // Expensive calculations only when state changes
    }
    
    protected override object GetProtocolConfiguration()
    {
        return _device?.GetMyProtocolConfiguration();
    }
    
    protected override void OnApplyConfiguration(object configuration)
    {
        // Apply configuration changes
    }
}
```

2. **Create the Protocol State:**

```csharp
public class MyProtocolState : BaseProtocolState
{
    public string RouterId { get; set; } = "";
    public Dictionary<string, MyNeighbor> Neighbors { get; set; } = new();
    
    public override Dictionary<string, object> GetStateData()
    {
        var baseData = base.GetStateData();
        baseData["RouterId"] = RouterId;
        baseData["Neighbors"] = Neighbors;
        return baseData;
    }
}
```

3. **Create the Plugin:**

```csharp
public class MyProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "My Protocol Plugin";
    public override ProtocolType ProtocolType => ProtocolType.OSPF;
    
    public override IDeviceProtocol CreateProtocol()
    {
        return new MyProtocol();
    }
}
```

### Auto-Discovery

Protocols are automatically discovered and registered based on vendor:

```csharp
// In NetworkDevice constructor
private void AutoRegisterProtocols()
{
    var protocolDiscovery = new ProtocolDiscoveryService();
    var protocols = protocolDiscovery.GetProtocolsForVendor(this.Vendor);
    
    foreach (var protocol in protocols)
    {
        RegisterProtocol(protocol);
    }
}
```

## Benefits

1. **Modular** - Each protocol can be in its own project
2. **State Management** - Incorporates proven performance optimizations
3. **Vendor-Specific** - Supports vendor-specific protocol implementations
4. **CLI Integration** - Clean IoC/DI integration with CLI handlers
5. **Auto-Discovery** - Automatic protocol discovery like CLI handlers
6. **Extensible** - Easy to add new protocols without changing existing code
7. **Performance** - Conditional execution based on state changes

## Dependencies

- `NetForge.Simulation.Common` - Core simulation infrastructure
- `Microsoft.Extensions.DependencyInjection.Abstractions` - IoC/DI support
- `Microsoft.Extensions.Logging.Abstractions` - Logging support

## Related Documentation

- `COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md` - Complete protocol documentation and state management patterns
- Individual Protocol Project READMEs - Protocol-specific implementation details
- CLI Handler documentation for similar patterns