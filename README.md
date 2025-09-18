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

### Modern Architecture
- **Declarative Vendor System**: Vendor capabilities defined in descriptor classes
- **Auto-Registration**: Protocols and CLI handlers automatically registered based on vendor capabilities
- **Dependency Injection**: Full IoC container support with service registration
- **Interface Segregation**: Clean separation of concerns with 9 focused interfaces
- **Value Objects**: Type-safe network primitives (IpAddress, MacAddress, NetworkPrefix)
- **Command Pattern**: Shared business logic with vendor-specific formatting

## 🏗️ Solution Structure

```
NetForge/
├── NetForge.Interfaces/                    # Core interface definitions
├── NetForge.Simulation.Common/             # Shared models and base classes
├── NetForge.Simulation.DataTypes/          # Value objects and validation
├── NetForge.Simulation.EventBus/           # Event-driven messaging
├── NetForge.Simulation.Topology/           # Network topology and devices
│   ├── Common/Vendors/                     # Vendor system implementation
│   ├── Vendors/                            # Vendor descriptors (Cisco, Juniper, Arista)
│   └── Devices/                            # Device implementations
├── NetForge.Simulation.Protocols/          # Protocol implementations
│   ├── NetForge.Simulation.Protocols.Common/      # Protocol base classes
│   ├── NetForge.Simulation.Protocols.OSPF/        # OSPF with SPF algorithm
│   ├── NetForge.Simulation.Protocols.BGP/         # BGP with path selection
│   ├── NetForge.Simulation.Protocols.CDP/         # CDP with TLV processing
│   └── [14 other protocol implementations]
├── NetForge.Simulation.CliHandlers/        # CLI implementations
│   ├── NetForge.Simulation.CliHandlers.Common/    # Shared CLI services
│   ├── NetForge.Simulation.CliHandlers.Cisco/     # Cisco IOS CLI
│   └── [14 other vendor CLI handlers]
├── NetForge.Simulation.Handlers.Common/    # Handler infrastructure
├── NetForge.Tests/                         # Unit and integration tests
└── NetForge.Player/                        # CLI tool for network simulation
```

## 📊 Implementation Status

### ✅ Completed Components
- **Vendor System**: Declarative vendor architecture with descriptors for Cisco, Juniper, Arista
- **Auto-Registration System**: Protocols and CLI handlers automatically registered based on vendor capabilities
- **CLI Handlers**: All 15 vendor implementations complete with command processing
- **Protocols**: All 17 protocols implemented with full state management
- **Interface Segregation**: INetworkDevice split into 9 focused interfaces
- **Value Objects**: Network primitives with validation (IpAddress, MacAddress, etc.)
- **Test Infrastructure**: Comprehensive testing with MockDeviceBuilder
- **Service Delegation**: NetworkDevice refactored with specialized service classes

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
| **HTTP** | Management | ✅ Complete | ~300 | Web interface support |

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
using NetForge.Simulation.Topology;
using NetForge.Interfaces.Devices;

// Create a network device
var ciscoRouter = new NetworkDevice
{
    Name = "Router1",
    Vendor = "Cisco",
    DeviceType = "Router"
};

// Initialize with vendor system (auto-registers protocols and CLI handlers)
var serviceProvider = new ServiceCollection()
    .ConfigureVendorSystem()
    .BuildServiceProvider();

await VendorSystemStartup.InitializeDeviceWithVendorSystemAsync(ciscoRouter, serviceProvider);

// Add interfaces
var gi0 = new InterfaceConfig
{
    Name = "GigabitEthernet0/0",
    IpAddress = "192.168.1.1",
    SubnetMask = "255.255.255.0",
    IsUp = true
};
ciscoRouter.GetInterfaceManager().AddInterface(gi0);

// Process CLI commands
var output = await ciscoRouter.ProcessCommand("show ip interface brief");
Console.WriteLine(output);
```

### Using Declarative API

```csharp
using NetForge.Simulation.Common.Declarative;

// Build topology declaratively
var topology = new DeclarativeTopologyFactory(serviceProvider)
    .CreateTopology(spec => spec
        .WithName("Test Network")
        .AddDevice(device => device
            .WithName("Router1")
            .WithVendor("Cisco")
            .WithModel("ISR4451")
            .AddInterface("GigabitEthernet0/0", "192.168.1.1", "255.255.255.0")
            .AddProtocol("OSPF", config => config.ProcessId = 1))
        .AddDevice(device => device
            .WithName("Router2")
            .WithVendor("Juniper")
            .WithModel("MX204")
            .AddInterface("ge-0/0/0", "192.168.1.2", "255.255.255.0")
            .AddProtocol("OSPF", config => config.ProcessId = 1))
        .ConnectDevices("Router1", "GigabitEthernet0/0", "Router2", "ge-0/0/0"));

// Network is ready with OSPF running
await topology.StartAsync();
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
- **SOLID Principles**: Clean architecture with dependency injection
- **Event-Driven**: Protocol events and state changes
- **Async/Await**: Non-blocking operations throughout
- **Memory Efficient**: Automatic cleanup and resource management
- **Performance Optimized**: Sub-second CLI responses, fast protocol convergence

### Key Design Patterns
- **Vendor Descriptor Pattern**: Declarative vendor capability definition
- **Service Delegation**: Separation of concerns in NetworkDevice
- **Command Pattern**: CLI command processing with vendor formatters
- **State Machine**: Protocol state management
- **Value Objects**: Type-safe network primitives

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
- [Protocols Documentation](NetForge.Simulation.Protocols/CLAUDE.md)
- [CLI Handlers Documentation](NetForge.Simulation.CliHandlers/README.md)
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