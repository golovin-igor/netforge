# NetSim - Network Device Simulation Framework

A comprehensive C# .NET framework for simulating network devices with realistic CLI behavior, protocol implementations, and network topology management.

## Overview

NetSim is a powerful network simulation platform that provides:

- **Multi-vendor CLI simulation** supporting 15+ network equipment vendors
- **Comprehensive protocol implementations** including routing, switching, and discovery protocols  
- **Realistic device behavior** with vendor-specific command syntax and responses
- **Physical layer simulation** with connection quality modeling
- **Modular architecture** for easy extensibility and testing

## Architecture

The NetSim solution consists of several key components:

### Core Libraries
- **NetSim.Simulation.Common** - Core networking protocols, device models, and shared infrastructure
- **NetSim.Simulation.Core** - Device implementations, factories, and simulation engine

### CLI Handler Libraries
- **Vendor-specific CLI handlers** for Cisco, Juniper, Arista, Huawei, Fortinet, Nokia, Dell, Extreme, Broadcom, MikroTik, Alcatel, Anira, Linux, F5, and Aruba
- **Common CLI handlers** for shared functionality across vendors
- **Comprehensive test suite** validating all CLI implementations

### Player Application  
- **NetSim.Player** - Standalone application for running network simulations

## Key Features

### Multi-Vendor Support
- **15 supported vendors** with realistic CLI behavior
- **Device-specific prompts** and command syntax
- **Vendor-specific configuration parsing** and NVRAM support
- **Authentic error messages** and help systems

### Network Protocols
- **Layer 2**: VLANs, STP/RSTP, LACP, CDP, LLDP
- **Layer 3**: Static routing, OSPF, BGP, RIP, EIGRP, IS-IS, IGRP
- **Security**: Access Control Lists, security protocols
- **Management**: SNMP, telnet/SSH simulation

### Physical Layer Simulation
- **Connection quality modeling** with bandwidth, latency, packet loss
- **Link state management** (up/down/degraded)  
- **Protocol-aware connectivity** that influences routing decisions
- **Event-driven updates** for realistic network behavior

### Advanced Features
- **Device factory pattern** for easy device creation
- **Network topology management** with automatic protocol updates
- **Configuration management** with NVRAM parsing
- **Comprehensive event system** for monitoring network changes
- **Terminal server support** for remote device access

## Quick Start

```csharp
using NetSim.Simulation.Core;
using NetSim.Simulation.Common;

// Create devices
var cisco = DeviceFactory.CreateDevice("cisco", "Router1");
var juniper = DeviceFactory.CreateDevice("juniper", "Router2");

// Build network
var network = new Network();
network.AddDevice(cisco);
network.AddDevice(juniper);
network.AddLink("Router1", "GigabitEthernet0/0", "Router2", "ge-0/0/0");

// Configure devices
cisco.ProcessCommand("enable");
cisco.ProcessCommand("configure terminal");
cisco.ProcessCommand("interface GigabitEthernet0/0");
cisco.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
cisco.ProcessCommand("no shutdown");

// Update protocols and test connectivity
network.UpdateProtocols();
var result = cisco.ProcessCommand("ping 10.0.0.2");
```

## Project Structure

See individual component READMEs for detailed information:
- [NetSim.Simulation.Common](NetSim.Simulation.Common/README.md) - Core protocols and infrastructure
- [NetSim.Simulation.Core](NetSim.Simulation.Core/README.md) - Device implementations and simulation engine
- [CLI Handlers](NetSim.Simulation.CliHandlers/) - Vendor-specific command line interfaces

## Development

### Prerequisites
- .NET 6.0 or later
- Visual Studio 2022 or VS Code

### Building
```bash
dotnet build NetSim.sln
```

### Testing
```bash
dotnet test
```

## License

This project is for educational and testing purposes.
