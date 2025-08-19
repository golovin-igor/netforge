# NetForge.Simulation.Common - Network Protocol Simulation Engine

A comprehensive network protocol simulation library for the NetForge platform, providing realistic implementations of major networking protocols with advanced state management and physical connection integration.

## 🏗️ **Architecture Overview**

```
NetForge.Simulation.Common/
├── Common/                    # Core network device and connection models
├── Configuration/             # Device and interface configuration
├── Protocols/                 # Network protocol implementations
│   ├── Implementations/       # Protocol business logic
│   ├── Routing/              # Configuration and state classes
│   ├── Security/             # Security protocols and ACLs
│   └── Switching/            # Layer 2 protocols
├── Events/                   # Network event system
├── CLI/                      # Command-line interface handlers
└── CommandHistory/           # Command tracking and validation
```

## 🌐 **Implemented Network Protocols**

### **Layer 3 Routing Protocols**

#### **OSPF (Open Shortest Path First)**
- **File**: `Protocols/Implementations/OspfProtocol.cs`
- **State**: `Protocols/Routing/OspfState.cs`
- **Features**:
  - Full neighbor adjacency state machine (Down → Init → 2-Way → ExStart → Exchange → Loading → Full)
  - LSA database management with aging and refresh
  - SPF calculation with Dijkstra's algorithm (conditional execution)
  - Area support with proper DR/BDR election
  - Hello protocol with dead neighbor detection
- **Administrative Distance**: 110
- **State Management**: ✅ Full implementation with topology change detection

#### **BGP (Border Gateway Protocol)**
- **File**: `Protocols/Implementations/BgpProtocol.cs`
- **State**: `Protocols/Routing/BgpState.cs`
- **Features**:
  - Complete BGP finite state machine (Idle → Connect → Active → OpenSent → OpenConfirm → Established)
  - Peer session management with hold timers
  - Multiple RIB tables (Adj-RIB-In, Adj-RIB-Out, RIB)
  - Best path selection algorithm
  - AS path tracking and loop prevention
- **Administrative Distance**: 200 (eBGP), 20 (iBGP)
- **State Management**: ✅ Full implementation with policy change detection

#### **EIGRP (Enhanced Interior Gateway Routing Protocol)**
- **File**: `Protocols/Implementations/EigrpProtocol.cs`
- **State**: `Protocols/Routing/EigrpState.cs`
- **Features**:
  - DUAL (Diffusing Update Algorithm) implementation
  - Neighbor discovery and adjacency maintenance
  - Topology table with successor/feasible successor tracking
  - Metric calculation with bandwidth, delay, reliability, load
  - Split horizon and route summarization
- **Administrative Distance**: 90
- **State Management**: ✅ Full implementation with topology change detection

#### **RIP (Routing Information Protocol)**
- **File**: `Protocols/Implementations/RipProtocol.cs`
- **State**: `Protocols/Routing/RipState.cs`
- **Features**:
  - Distance vector algorithm with hop count metric
  - Route timers (180s timeout, 240s flush)
  - Split horizon and poison reverse
  - Triggered updates on topology changes
  - Route state machine (Valid → Invalid → Holddown → Flushing)
- **Administrative Distance**: 120
- **State Management**: ✅ Full implementation with route aging

#### **IGRP (Interior Gateway Routing Protocol)**
- **File**: `Protocols/Implementations/IgrpProtocol.cs`
- **State**: `Protocols/Routing/IgrpState.cs`
- **Features**:
  - Cisco proprietary distance vector protocol
  - Composite metric (bandwidth, delay, reliability, load, MTU)
  - Neighbor adjacency with hold timers
  - Route aging and flush timers (270s invalid, 630s flush)
  - Autonomous system boundary support
- **Administrative Distance**: 100
- **State Management**: ✅ Full implementation with neighbor tracking

#### **IS-IS (Intermediate System to Intermediate System)**
- **File**: `Protocols/Implementations/IsisProtocol.cs`
- **State**: `Protocols/Routing/IsisState.cs`
- **Features**:
  - Link-state protocol with LSP database
  - Level-1/Level-2 hierarchy support
  - Adjacency establishment and maintenance
  - SPF calculation with conditional execution
  - DIS (Designated Intermediate System) election
- **Administrative Distance**: 115
- **State Management**: ✅ Full implementation with LSP aging

### **Layer 2 Discovery Protocols**

#### **CDP (Cisco Discovery Protocol)**
- **File**: `Protocols/Implementations/CdpProtocol.cs`
- **State**: `Protocols/Routing/CdpState.cs`
- **Features**:
  - Neighbor discovery with device capabilities
  - Platform and version advertisement
  - Interface-specific neighbor tracking
  - Advertisement timers (60s default) and hold times (180s)
  - Physical connection quality validation
- **State Management**: ✅ Full implementation with neighbor aging

#### **LLDP (Link Layer Discovery Protocol)**
- **File**: `Protocols/Implementations/LldpProtocol.cs`
- **State**: `Protocols/Routing/LldpState.cs`
- **Features**:
  - IEEE 802.1AB standard implementation
  - System and port information exchange
  - Management address advertisement
  - TLV (Type-Length-Value) structure support
  - Chassis ID and Port ID tracking
- **State Management**: ✅ Full implementation with TTL management

### **Layer 2 Redundancy Protocols**

#### **STP (Spanning Tree Protocol)**
- **File**: `Protocols/Implementations/StpProtocol.cs`
- **State**: `Protocols/Routing/StpState.cs`
- **Features**:
  - IEEE 802.1D standard implementation
  - Bridge ID calculation and root bridge election
  - Port state machine (Blocking → Listening → Learning → Forwarding)
  - BPDU processing and aging
  - Topology change detection and notification
- **State Management**: ✅ Full implementation with port state tracking

#### **VRRP (Virtual Router Redundancy Protocol)**
- **File**: `Protocols/Implementations/VrrpProtocol.cs`
- **State**: `Protocols/Routing/VrrpState.cs`
- **Features**:
  - RFC 3768 standard implementation
  - Master/Backup election with priority
  - Virtual IP address management
  - Advertisement intervals and master timeout
  - Graceful failover handling
- **State Management**: ✅ Full implementation with group state tracking

#### **HSRP (Hot Standby Router Protocol)**
- **File**: `Protocols/Implementations/HsrpProtocol.cs`
- **State**: `Protocols/Routing/HsrpState.cs`
- **Features**:
  - Cisco proprietary redundancy protocol
  - State machine (Initial → Learn → Listen → Speak → Standby/Active)
  - Hello timers and hold times
  - Group-based virtual router management
  - Priority and preemption support
- **State Management**: ✅ Full implementation with group monitoring

## 🔄 **Advanced State Management Pattern**

All protocol implementations follow a consistent state management pattern:

### **Key Components**

1. **State Classes**: Each protocol has a dedicated state class (`[Protocol]State.cs`)
2. **Change Detection**: Protocols only perform expensive operations when state changes
3. **Conditional Processing**: SPF, DUAL, and other algorithms run only when necessary
4. **Neighbor Management**: Automatic cleanup of stale neighbors and routes
5. **Physical Integration**: Connection quality validation before adjacency establishment

### **State Management Features**

```csharp
// Example state class structure
public class ProtocolState
{
  // Change tracking
  public bool StateChanged { get; set; } = true;
  public bool TopologyChanged { get; set; } = false;
  public bool RoutesChanged { get; set; } = false;
  // Neighbor management
  public Dictionary<string, Neighbor> Neighbors { get; set; } = new();
    public List<string> GetStaleNeighbors(int timeout = 180);
    public void RemoveNeighbor(string id);
    
    // State persistence
    public DateTime LastUpdate { get; set; } = DateTime.MinValue;
    public void MarkStateChanged();
}
```

### **Performance Benefits**

- **Reduced CPU Usage**: Skip expensive calculations when state unchanged
- **Better Scalability**: Handle larger networks efficiently
- **Improved Convergence**: Faster response to actual network changes
- **Memory Efficiency**: Automatic cleanup prevents memory leaks
- **Debug Visibility**: Comprehensive logging of state changes

## 🔗 **Physical Connection Integration**

All protocols integrate with the physical connection system:

```csharp
private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
{
    var connection = device.GetPhysicalConnectionMetrics(interfaceName);
    return connection?.IsSuitableForRouting ?? false;
}
```

### **Benefits**

- **Connection Quality Assurance**: Only establish adjacencies over suitable connections
- **Network Stability**: Prevents flapping adjacencies on poor connections
- **Realistic Simulation**: Models real-world protocol behavior
- **Performance Optimization**: Avoids wasted resources on unsuitable links

## 📊 **Route Installation with Administrative Distances**

All routing protocols properly install routes with correct administrative distances:

```csharp
private async Task InstallRoutes(NetworkDevice device)
{
    device.ClearRoutesByProtocol("PROTOCOL_NAME");
    
    foreach (var route in _state.CalculatedRoutes)
    {
        var deviceRoute = new Route(route.Network, route.Mask, route.NextHop, route.Interface, "PROTOCOL_NAME")
        {
            Metric = route.Metric,
            AdminDistance = PROTOCOL_AD
        };
        device.AddRoute(deviceRoute);
    }
}
```

### **Administrative Distance Values**

| Protocol | Administrative Distance | Notes |
|----------|------------------------|-------|
| **Connected** | 0 | Directly connected interfaces |
| **Static** | 1 | Manually configured routes |
| **EIGRP** | 90 | Enhanced Interior Gateway Routing |
| **IGRP** | 100 | Interior Gateway Routing |
| **OSPF** | 110 | Open Shortest Path First |
| **IS-IS** | 115 | Intermediate System to Intermediate System |
| **RIP** | 120 | Routing Information Protocol |
| **External EIGRP** | 170 | EIGRP external routes |
| **iBGP** | 200 | Internal BGP |
| **eBGP** | 20 | External BGP |

## 🧪 **Testing and Validation**

### **Protocol Test Coverage**

- **Unit Tests**: Individual protocol logic testing
- **Integration Tests**: Multi-protocol interaction testing
- **State Management Tests**: Proper state transitions and cleanup
- **Performance Tests**: Scalability and convergence time measurement
- **Physical Integration Tests**: Connection quality impact validation

### **Debugging Features**

- **Comprehensive Logging**: Detailed state change logging
- **Neighbor Tracking**: Real-time adjacency monitoring
- **Route Table Inspection**: Route installation and removal tracking
- **Timer Management**: Timeout and refresh timer visibility
- **Error Handling**: Graceful degradation and recovery

## 🚀 **Usage Examples**

### **Protocol Initialization**

```csharp
// Initialize OSPF protocol
var ospfProtocol = new OspfProtocol();
ospfProtocol.Initialize(device);
ospfProtocol.SubscribeToEvents(network.EventBus, device);

// Configure OSPF
device.SetOspfConfiguration(new OspfConfig
{
    RouterId = "1.1.1.1",
    Areas = new Dictionary<string, OspfArea>
    {
        ["0.0.0.0"] = new OspfArea { AreaId = "0.0.0.0", AreaType = "standard" }
    }
});

// Update protocol state
await ospfProtocol.UpdateState(device);
```

### **State Monitoring**

```csharp
// Check protocol state
var ospfState = ospfProtocol.GetState();
Console.WriteLine($"Neighbors: {ospfState.Neighbors.Count}");
Console.WriteLine($"LSAs: {ospfState.LsaDatabase.Count}");
Console.WriteLine($"Last SPF: {ospfState.LastSpfCalculation}");
Console.WriteLine($"Topology Changed: {ospfState.TopologyChanged}");
```

## 📚 **Additional Documentation**

- **[Protocol State Management Guide](Protocols/PROTOCOL_STATE_MANAGEMENT.md)**: Detailed implementation patterns
- **[Physical Connection Integration](../Common/PhysicalConnection.cs)**: Connection quality assessment
- **[Event System](../Events/)**: Network event handling and propagation
- **[CLI Integration](../CLI/)**: Command-line interface for protocol configuration

## 🤝 **Contributing**

When implementing new protocols:

1. Follow the established state management pattern
2. Implement proper administrative distances
3. Include physical connection integration
4. Add comprehensive logging and error handling
5. Write unit and integration tests
6. Update this documentation

## 📄 **License**

This code is part of the NetForge platform and is for educational and testing purposes. 