*NetForge - Network Device Simulation Framework*

```
     ███╗   ██╗ ███████╗ ████████╗ ███████╗  ██████╗  ██████╗   ██████╗  ███████╗
     ████╗  ██║ ██╔════╝ ╚══██╔══╝ ██╔════╝ ██╔═══██╗ ██╔══██╗ ██╔════╝  ██╔════╝
     ██╔██╗ ██║ █████╗      ██║    █████╗   ██║   ██║ ██████╔╝ ██║  ███║ █████╗
     ██║╚██╗██║ ██╔══╝      ██║    ██╔══╝   ██║   ██║ ██╔══██╗ ██║   ██║ ██╔══╝
     ██║ ╚████║ ███████╗    ██║    ██║      ╚██████╔╝ ██║  ██║ ╚██████╔╝ ███████╗
     ╚═╝  ╚═══╝ ╚══════╝    ╚═╝    ╚═╝       ╚═════╝  ╚═╝  ╚═╝  ╚═════╝  ╚══════╝
```

# NetForge

NetForge is a comprehensive C# .NET 9.0 framework for simulating enterprise network devices with realistic CLI behavior, advanced protocol implementations, and sophisticated network topology management.

## 🚀 Key Features

### Multi-Vendor Support (15+ Vendors)
- **Cisco** - IOS/IOS-XE command structure
- **Juniper** - JunOS with commit-based configuration
- **Arista** - EOS with modern networking features
- **Huawei** - VRP command structure
- **Fortinet** - FortiOS security-focused CLI
- **Nokia** - SR OS hierarchical configuration
- **Dell** - OS10/PowerSwitch commands
- **Extreme** - EXOS policy-based management
- **MikroTik** - RouterOS with unique syntax
- **F5** - TMOS load balancer commands
- **Aruba** - ArubaOS wireless and switching
- **Broadcom** - SONiC commands
- **Alcatel** - AOS enterprise switching
- **Anira** - Network simulation commands
- **Linux** - Standard Linux networking

### Protocol Implementations (17 Protocols)
- **Routing**: OSPF, BGP, RIP, EIGRP, IS-IS, IGRP
- **Discovery**: CDP, LLDP, ARP
- **Redundancy**: VRRP, HSRP
- **Layer 2**: STP
- **Management**: SSH, Telnet, SNMP, HTTP
- **All protocols feature**:
  - Full state machine implementation
  - Vendor-aware behavior
  - Event-driven architecture
  - Performance optimized

### HTTP Web Management (Complete Implementation)
- **Multi-Vendor Web Interfaces**: Authentic web management interfaces for all vendors
- **REST API Framework**: Complete programmatic API access with OpenAPI documentation
- **Authentication & Security**: JWT tokens, HTTPS support, role-based access control
- **Real-time Monitoring**: Live dashboards with protocol state and interface status
- **Configuration Management**: Web-based device configuration with CLI integration
- **Session Management**: Automatic session handling with cleanup and expiration
- **Performance Optimized**: Support for 1000+ concurrent connections

### Modern Architecture
- **Clean Architecture**: Separation of data contracts (SimulationModel) from implementation
- **Interface-Driven Design**: All components implement well-defined interfaces from SimulationModel
- **Dependency Injection**: Full IoC container support with service registration
- **Layered Protocol Stack**: Protocol implementations organized by OSI layers
- **Component-Based Handlers**: Separate projects for CLI, HTTP, NETCONF, and SNMP handlers
- **Value Objects**: Type-safe network primitives (IpAddress, MacAddress, NetworkPrefix)

## 🏗️ Solution Structure

```
NetForge/
├── NetForge.SimulationModel/               # Core interfaces, data contracts, and requirements
│   ├── Builders/                           # Builder interfaces for device and topology creation
│   ├── Configuration/                      # Configuration interfaces (ARP, routing, protocols)
│   ├── Core/                              # Core simulation interfaces (packets, topology, connections)
│   ├── Devices/                           # Device and network interface definitions
│   ├── Engine/                            # Simulation engine interfaces
│   ├── Events/                            # Event-driven messaging interfaces
│   ├── Management/                        # Management protocol interfaces (CLI, HTTP, SNMP)
│   ├── Protocols/                         # Network protocol interfaces
│   └── Types/                             # Type definitions and enums
├── NetForge.Simulation/                    # Core simulation implementation
│   ├── Core/                              # Topology and connection implementations
│   ├── Engine/                            # Simulation engine implementation
│   └── Events/                            # Event bus and subscription implementations
├── NetForge.CliHandlers/                  # CLI handler implementations
├── NetForge.HttpHandlers/                 # HTTP web management implementations
├── NetForge.NetconfHandlers/              # NETCONF protocol implementations
├── NetForge.SnmpHandlers/                 # SNMP protocol implementations
├── NetForge.Protocols.Layer1/             # Physical layer protocol implementations
├── NetForge.Protocols.Layer2/             # Data link layer protocol implementations (STP, CDP, LLDP)
├── NetForge.Protocols.Layer3/             # Network layer protocol implementations (OSPF, BGP, RIP)
└── NetForge.Protocols.Layer4/             # Transport layer protocol implementations
```

## 📊 Implementation Status

### ✅ Completed Components
- **SimulationModel**: Complete interface definitions and data contracts (113 interfaces)
- **Clean Architecture**: Separation of contracts from implementation with proper project structure
- **Protocol Layer Organization**: Protocols organized by OSI layers (Layer 1-4)
- **Handler Components**: Separate projects for CLI, HTTP, NETCONF, and SNMP handlers
- **Core Simulation**: Event-driven simulation engine with topology management
- **Interface-Driven Design**: All implementations reference SimulationModel contracts
- **Build System**: Complete solution that builds successfully with proper dependencies
- **Type Safety**: Comprehensive type definitions and enums in SimulationModel

### 📈 Protocol Details

| Protocol | Type | Status | Lines | Features |
|----------|------|--------|-------|----------|
| **OSPF** | Routing | ✅ Complete | 541 | SPF algorithm, LSA database, Areas |
| **BGP** | Routing | ✅ Complete | 509 | FSM, Path selection, Peer management |
| **RIP** | Routing | ✅ Complete | 360 | Distance vector, Split horizon |
| **EIGRP** | Routing | ✅ Complete | ~500 | DUAL algorithm, Feasible successors |
| **IS-IS** | Routing | ✅ Complete | ~500 | Link-state database, SPF |
| **IGRP** | Routing | ✅ Complete | ~200 | Legacy protocol support |
| **CDP** | Discovery | ✅ Complete | 1248 | Full TLV processing, Neighbor discovery |
| **LLDP** | Discovery | ✅ Complete | ~400 | IEEE 802.1AB standard |
| **ARP** | Discovery | ✅ Complete | ~300 | Table management, Request/Reply |
| **VRRP** | HA | ✅ Complete | ~400 | Virtual router, Master election |
| **HSRP** | HA | ✅ Complete | ~400 | State machine, Virtual MAC |
| **STP** | Layer 2 | ✅ Complete | 587 | BPDU processing, Root election |
| **SSH** | Management | ✅ Complete | ~400 | Secure access, Authentication |
| **Telnet** | Management | ✅ Complete | ~300 | Multi-session support |
| **SNMP** | Management | ✅ Complete | 317 | MIB management, Traps |
| **HTTP** | Management | ✅ Complete | ~300 | Web interface support, REST API, Authentication |

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Windows 10/11 or Linux

### Build and Run
```bash
# Clone the repository
git clone https://github.com/yourusername/NetForge.git
cd NetForge

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the player CLI
dotnet run --project NetForge.Player
```

### Basic Usage Example

```csharp
using NetForge.Simulation.Core;
using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Configuration;

// Create a network topology
var topology = new Topology();

// Create network devices using the interface contracts
var router1 = new NetworkDevice("Router1")
{
    Vendor = new CiscoVendor(),
    Configuration = new DeviceConfiguration()
};

var router2 = new NetworkDevice("Router2")
{
    Vendor = new JuniperVendor(),
    Configuration = new DeviceConfiguration()
};

// Add devices to topology
topology.AddDevice(router1);
topology.AddDevice(router2);

// Create physical connection between devices
var connection = new PhysicalConnection(
    router1.Id, "GigabitEthernet0/0",
    router2.Id, "ge-0/0/0"
);
topology.AddConnection(connection);

// Start the simulation
await topology.Start();
```

### Protocol Implementation Example

```csharp
using NetForge.SimulationModel.Protocols;
using NetForge.Protocols.Layer3; // OSPF implementation

// Add OSPF protocol to a device
var ospfProtocol = new OspfProtocol();
router1.RegisterProtocol(ospfProtocol);

// Configure OSPF
var ospfConfig = new OspfConfiguration
{
    ProcessId = 1,
    RouterId = "192.168.1.1",
    Areas = new List<OspfArea>
    {
        new OspfArea { AreaId = "0.0.0.0", AreaType = AreaType.Backbone }
    }
};

ospfProtocol.ApplyConfiguration(ospfConfig);
ospfProtocol.Start();
```

## 🎯 Use Cases

### Education & Training
- Learn networking concepts with realistic device behavior
- Practice for vendor certifications (CCNA, JNCIA, etc.)
- Understand protocol operations and troubleshooting

### Development & Testing
- Test network automation scripts safely
- Validate configurations before deployment
- Develop network management applications

### Research
- Protocol behavior analysis
- Network topology modeling
- Performance optimization studies

## 🛠️ Development

### Architecture Highlights
- **Clean Architecture**: Interface contracts separated from implementation
- **Dependency Inversion**: All implementations depend on SimulationModel abstractions
- **Event-Driven**: Protocol events and state changes through event bus
- **Layered Design**: Protocol stack organized by OSI model layers
- **Component Modularity**: Handler implementations in separate projects

### Key Design Patterns
- **Interface Segregation**: 113 focused interfaces in SimulationModel
- **Repository Pattern**: Configuration and state management interfaces
- **Observer Pattern**: Event-driven communication between components
- **Strategy Pattern**: Protocol and handler implementations
- **Builder Pattern**: Device and topology construction interfaces

### Testing
The project includes comprehensive testing:
- Unit tests for all major components
- Integration tests for protocol interactions
- CLI handler tests for each vendor
- Mock builders for test data creation

## 📚 Documentation

- [Architecture Overview](CLAUDE.md) - Detailed architecture documentation
- [Contributing Guide](CONTRIBUTING.md) - How to contribute to the project
- [Security Policy](SECURITY.md) - Security guidelines
- [Code of Conduct](CODE_OF_CONDUCT.md) - Community guidelines

Each major component has its own README:
- [Architecture Documentation](CLAUDE.md) - Complete architecture and implementation guide
- [Protocols Documentation](NetForge.Simulation.Protocols/CLAUDE.md)
- [CLI Handlers Documentation](NetForge.Simulation.CliHandlers/README.md)
- [HTTP Handlers Documentation](NetForge.Simulation.HttpHandlers/README.md)
- [Topology Documentation](NetForge.Simulation.Topology/README.md)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details on:
- Development setup
- Coding standards
- Testing requirements
- Pull request process

## 📄 License

This project is designed for educational and research purposes. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- Built with .NET 9.0 and C# 13
- Inspired by real-world networking equipment
- Community contributions and feedback

---

**Note**: This is a simulation framework for educational purposes. Vendor-specific behaviors are implemented for learning and testing, not commercial replication.