# NetSim - Network Device Simulation Framework


NetSim is a modular C# .NET framework for simulating network devices with realistic CLI behavior, protocol implementations, and network topology management. It supports 15+ vendors and advanced protocol/state simulation for education, testing, and automation.


## Features

- **Multi-vendor CLI simulation**: 15+ network equipment vendors (Cisco, Juniper, Arista, Huawei, Fortinet, Nokia, Dell, Extreme, Broadcom, MikroTik, Alcatel, Anira, Linux, F5, Aruba)
- **Protocol models**: Layer 2 (VLAN, STP/RSTP, LACP, CDP, LLDP), Layer 3 (Static, OSPF, BGP, RIP, EIGRP, IS-IS, IGRP), Security (ACLs)
- **Realistic device behavior**: Vendor-specific command syntax, prompts, error messages, and help systems
- **Physical layer simulation**: Bandwidth, latency, packet loss, link state, and protocol-aware connectivity
- **Device factory pattern**: Easy device creation and extensibility
- **Event-driven architecture**: Realistic protocol and topology updates
- **Remote access**: Socket and WebSocket terminal servers for integration/testing


## Solution Architecture

### Core Libraries
- **NetSim.Simulation.Common**: Core protocols, device models, shared infrastructure ([details](NetSim.Simulation.Common/README.md))
- **NetSim.Simulation.Core**: Device implementations, factories, simulation engine ([details](NetSim.Simulation.Core/README.md))

### CLI Handler Libraries
- **NetSim.Simulation.CliHandlers**: Vendor-specific CLI handlers for Cisco, Juniper, Arista, Huawei, Fortinet, Nokia, Dell, Extreme, Broadcom, MikroTik, Alcatel, Anira, Linux, F5, Aruba ([details](NetSim.Simulation.CliHandlers/README.md))
- **NetSim.Simulation.CliHandlers.Common**: Shared CLI logic

### Test Projects
- **NetSim.Simulation.Tests**: Validates simulation, protocols, and device behaviors ([details](NetSim.Simulation.Tests/README.md))

## Supported Vendors

| Vendor     | Module                                    |
|------------|-------------------------------------------|
| Alcatel    | NetSim.Simulation.CliHandlers.Alcatel     |
| Anira      | NetSim.Simulation.CliHandlers.Anira       |
| Arista     | NetSim.Simulation.CliHandlers.Arista      |
| Aruba      | NetSim.Simulation.CliHandlers.Aruba       |
| Broadcom   | NetSim.Simulation.CliHandlers.Broadcom    |
| Cisco      | NetSim.Simulation.CliHandlers.Cisco       |
| Dell       | NetSim.Simulation.CliHandlers.Dell        |
| Extreme    | NetSim.Simulation.CliHandlers.Extreme     |
| F5         | NetSim.Simulation.CliHandlers.F5          |
| Fortinet   | NetSim.Simulation.CliHandlers.Fortinet    |
| Huawei     | NetSim.Simulation.CliHandlers.Huawei      |
| Juniper    | NetSim.Simulation.CliHandlers.Juniper     |
| Linux      | NetSim.Simulation.CliHandlers.Linux       |
| MikroTik   | NetSim.Simulation.CliHandlers.MikroTik    |
| Nokia      | NetSim.Simulation.CliHandlers.Nokia       |

## Protocol Support

- **Layer 2**: VLAN, STP/RSTP, LACP, CDP, LLDP
- **Layer 3**: Static, OSPF, BGP, RIP, EIGRP, IS-IS, IGRP
- **Security**: ACLs

## Physical & Event Simulation

- Bandwidth, latency, packet loss, link state (up/down/degraded)
- Protocol-aware connectivity and event-driven updates

## Key Features

### Multi-Vendor Support
- **15 supported vendors** with realistic CLI behavior
- **Device-specific prompts** and command syntax
- **Vendor-specific configuration parsing** and NVRAM support
- **Authentic error messages** and help systems

### Network Protocols
- **Layer 2**: VLANs, STP/RSTP, LACP, CDP, LLDP
- **Layer 3**: Static routing, OSPF, BGP, RIP, EIGRP, IS-IS, IGRP
- **Security**: Access Control Lists and related security features

### Physical Layer Simulation
- **Connection quality modeling** with bandwidth, latency, packet loss
- **Link state management** (up/down/degraded)  
- **Protocol-aware connectivity** that influences routing decisions
- **Event-driven updates** for realistic network behavior

### Advanced Features
- **Device factory pattern** for easy device creation
- **Network topology management** with event-driven protocol updates
- **Configuration management** with NVRAM parsing
- **Comprehensive event system** for monitoring network changes

## Quick Start

## Quick Start Example
```csharp
using NetSim.Simulation.Core;
using NetSim.Simulation.Common;

// Create devices
var cisco = DeviceFactory.CreateDevice("cisco", "Router1");
var juniper = DeviceFactory.CreateDevice("juniper", "Router2");

// Build network
var network = new Network();
await network.AddDeviceAsync(cisco);
await network.AddDeviceAsync(juniper);
await network.AddLinkAsync("Router1", "GigabitEthernet0/0", "Router2", "ge-0/0/0");

// Configure devices
	await cisco.ProcessCommandAsync("enable");
	await cisco.ProcessCommandAsync("configure terminal");
	await cisco.ProcessCommandAsync("interface GigabitEthernet0/0");
	await cisco.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
	await cisco.ProcessCommandAsync("no shutdown");
	await cisco.ProcessCommandAsync("exit");

	await juniper.ProcessCommandAsync("configure");
	await juniper.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/24");
	await juniper.ProcessCommandAsync("commit");

// Test connectivity (protocols update automatically)
var result = await cisco.ProcessCommandAsync("ping 10.0.0.2");
```


## Project Structure

- [NetSim.Simulation.Common](NetSim.Simulation.Common/README.md): Core protocols, device models, and infrastructure
- [NetSim.Simulation.Core](NetSim.Simulation.Core/README.md): Device implementations, factories, simulation engine
- [NetSim.Simulation.CliHandlers](NetSim.Simulation.CliHandlers/README.md): Vendor-specific CLI handlers


## Development

### Prerequisites
- .NET 9.0 or later
- Visual Studio 2022 or VS Code

### Build
```pwsh
dotnet build NetSim.sln
```


### Test
```pwsh
dotnet test
```


## License

This project is for educational and testing purposes only.
