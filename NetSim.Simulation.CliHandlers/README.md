# NetSim CLI Handlers

This directory contains CLI (Command Line Interface) handlers for various network equipment vendors. Each vendor-specific module provides realistic CLI behavior, command processing, and configuration management for network device simulation.

## Supported Vendors

| Vendor | Module | Description |
|--------|--------|-------------|
| **Alcatel** | [NetSim.Simulation.CliHandlers.Alcatel](NetSim.Simulation.CliHandlers.Alcatel/) | Alcatel-Lucent network equipment CLI |
| **Anira** | [NetSim.Simulation.CliHandlers.Anira](NetSim.Simulation.CliHandlers.Anira/) | Anira networking devices CLI |
| **Arista** | [NetSim.Simulation.CliHandlers.Arista](NetSim.Simulation.CliHandlers.Arista/) | Arista EOS (Extensible Operating System) |
| **Aruba** | [NetSim.Simulation.CliHandlers.Aruba](NetSim.Simulation.CliHandlers.Aruba/) | Aruba network devices CLI |
| **Broadcom** | [NetSim.Simulation.CliHandlers.Broadcom](NetSim.Simulation.CliHandlers.Broadcom/) | Broadcom network switch CLI |
| **Cisco** | [NetSim.Simulation.CliHandlers.Cisco](NetSim.Simulation.CliHandlers.Cisco/) | Cisco IOS/IOS-XE command line interface |
| **Dell** | [NetSim.Simulation.CliHandlers.Dell](NetSim.Simulation.CliHandlers.Dell/) | Dell networking OS10 CLI |
| **Extreme** | [NetSim.Simulation.CliHandlers.Extreme](NetSim.Simulation.CliHandlers.Extreme/) | Extreme Networks EXOS CLI |
| **F5** | [NetSim.Simulation.CliHandlers.F5](NetSim.Simulation.CliHandlers.F5/) | F5 BIG-IP system CLI |
| **Fortinet** | [NetSim.Simulation.CliHandlers.Fortinet](NetSim.Simulation.CliHandlers.Fortinet/) | Fortinet FortiOS CLI |
| **Huawei** | [NetSim.Simulation.CliHandlers.Huawei](NetSim.Simulation.CliHandlers.Huawei/) | Huawei VRP (Versatile Routing Platform) |
| **Juniper** | [NetSim.Simulation.CliHandlers.Juniper](NetSim.Simulation.CliHandlers.Juniper/) | Juniper Junos CLI |
| **Linux** | [NetSim.Simulation.CliHandlers.Linux](NetSim.Simulation.CliHandlers.Linux/) | Linux networking commands |
| **MikroTik** | [NetSim.Simulation.CliHandlers.MikroTik](NetSim.Simulation.CliHandlers.MikroTik/) | MikroTik RouterOS CLI |
| **Nokia** | [NetSim.Simulation.CliHandlers.Nokia](NetSim.Simulation.CliHandlers.Nokia/) | Nokia SR OS (Service Router OS) |

## Common Components

- **[NetSim.Simulation.CliHandlers.Common](NetSim.Simulation.CliHandlers.Common/)** - Shared CLI functionality and common command handlers
- **[NetSim.Simulation.CliHandlers.Tests](NetSim.Simulation.CliHandlers.Tests/)** - Comprehensive test suite for all CLI handlers

## Architecture

Each vendor CLI handler module follows a consistent architecture:

### Core Components

1. **Handler Registry** - Registers and organizes command handlers
2. **Vendor Capabilities** - Defines device capabilities and supported features  
3. **Vendor Context** - Manages device state and configuration context
4. **Command Handlers** - Individual command processing implementations

### Directory Structure

```
NetSim.Simulation.CliHandlers.{Vendor}/
├── {Vendor}HandlerRegistry.cs          # Command registration
├── {Vendor}VendorCapabilities.cs       # Device capabilities  
├── {Vendor}VendorContext.cs            # Context management
├── Basic/                              # Basic device commands
│   └── BasicHandlers.cs
├── Configuration/                      # Configuration commands
│   └── ConfigurationHandlers.cs
├── Show/                              # Show/display commands  
│   └── ShowHandlers.cs
└── {vendor}_cli_commands.csv          # Command reference
```

## Key Features

### Realistic CLI Behavior
- **Vendor-specific prompts** and command syntax
- **Authentic error messages** and help text
- **Context-sensitive commands** based on device mode
- **Tab completion** and command abbreviation support

### Configuration Management
- **Hierarchical configuration** modes (global, interface, etc.)
- **Configuration validation** and error handling
- **NVRAM simulation** with startup-config support
- **Configuration rollback** capabilities (vendor-dependent)

### Protocol Support
- **Routing protocols** (OSPF, BGP, EIGRP, RIP, IS-IS, IGRP)
- **Switching protocols** (VLANs, STP, LACP)
- **Discovery protocols** (CDP, LLDP)
- **Security features** (ACLs, authentication)

### Device Operations
- **Interface management** (status, configuration, counters)
- **System operations** (reboot, file management)
- **Network diagnostics** (ping, traceroute)
- **Monitoring commands** (show commands, statistics)

## Usage Example

```csharp
// Create a Cisco device with CLI handlers
var cisco = new CiscoDevice("Router1");

cisco.ProcessCommand("configure terminal");
cisco.ProcessCommand("interface GigabitEthernet0/0");
cisco.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
cisco.ProcessCommand("no shutdown");
cisco.ProcessCommand("exit");

// Process commands through CLI handlers (async)
await cisco.ProcessCommandAsync("enable");
await cisco.ProcessCommandAsync("configure terminal");
await cisco.ProcessCommandAsync("interface GigabitEthernet0/0");
await cisco.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
await cisco.ProcessCommandAsync("no shutdown");
await cisco.ProcessCommandAsync("exit");

// Display configuration
var config = await cisco.ProcessCommandAsync("show running-config");
Console.WriteLine(config);
```

## Testing

The CLI handlers are thoroughly tested with:

- **Unit tests** for individual command handlers
- **Integration tests** for command sequences
- **Device-specific tests** for vendor behavior validation
- **Comprehensive coverage tests** ensuring all commands are tested

Run tests with:
```bash
dotnet test NetSim.Simulation.CliHandlers.Tests/
```

## Contributing

To add a new vendor CLI handler:

1. Create a new project: `NetSim.Simulation.CliHandlers.{Vendor}`
2. Implement the required components (Registry, Capabilities, Context)
3. Add command handlers following established patterns
4. Create comprehensive tests
5. Update this documentation

## License

This project is for educational and testing purposes.