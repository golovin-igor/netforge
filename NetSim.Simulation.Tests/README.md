# NetSim.Simulation.Tests

Comprehensive test suite for the NetSim network simulation framework with **56 test files** covering CLI handlers, protocol implementations, device behaviors, and multi-vendor scenarios across 15+ network equipment vendors.

## Overview

This test project validates the core functionality of NetSim including:
- **Multi-vendor CLI simulation** with realistic command processing
- **Network protocol implementations** with state management
- **Device factory patterns** and vendor-specific behaviors
- **Physical layer simulation** and connection quality metrics
- **Event-driven architecture** and network topology management

## Test Framework

- **Framework**: XUnit (.NET 9.0)
- **Mocking**: Moq for complex dependency scenarios
- **Test Runner**: Visual Studio Test SDK
- **Dependencies**: All NetSim CLI handler modules (15 vendors)

## Running Tests

### All Tests
```bash
dotnet test NetSim.Simulation.Tests/
```

### By Category
```bash
# Run alias tests for all vendors
dotnet test --filter "namespace~AliasTests"

# Run protocol implementation tests
dotnet test --filter "namespace~Protocols"

# Run counter/metrics tests
dotnet test --filter "namespace~CounterTests"

# Run specific vendor tests
dotnet test --filter "ClassName~CiscoVendorTests"
```

### With Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Categories

### 🔧 **AliasTests** (11 test files)
Validates CLI command aliases and interface naming conventions across vendors:
- **Command abbreviations**: `sh run` → `show running-config`
- **Interface aliases**: `Gig1/0/1` → `GigabitEthernet1/0/1`
- **Multi-vendor consistency**: Common aliases work across similar vendors
- **Vendor-specific shortcuts**: Platform-specific abbreviations

**Covered Vendors**: Arista, Aruba, Cisco, Dell, Fortinet, Huawei, Juniper, MikroTik, Nokia + Multi-vendor scenarios

### 📊 **CounterTests** (9 test files + detailed README)
Comprehensive RX/TX interface counter validation for network traffic simulation:
- **Ping counter accuracy**: Packet and byte counts for ICMP traffic
- **Protocol traffic**: OSPF, BGP, RIP packet counting
- **Interface state handling**: Counter behavior when interfaces are down
- **Multi-vendor topologies**: Cross-vendor connectivity testing

**See [CounterTests/README.md](./CounterTests/README.md)** for detailed packet size assumptions and test scenarios.

### 🌐 **Protocol Tests** (13 test files)
Network protocol implementation validation with state management:
- **Routing Protocols**: OSPF, BGP, EIGRP, RIP, IGRP, IS-IS
- **Discovery Protocols**: CDP, LLDP  
- **Redundancy Protocols**: STP, HSRP, VRRP
- **State transitions**: Protocol finite state machines
- **Neighbor management**: Adjacency establishment and maintenance
- **Route calculation**: SPF, DUAL algorithms with conditional execution

### 🏭 **Device & Factory Tests** (4 test files)
Device creation patterns and vendor-specific implementations:
- **DeviceFactory**: Multi-vendor device instantiation
- **Vendor detection**: Automatic vendor identification
- **Device modes**: User/privileged/configuration mode handling
- **Anira device specialization**: Custom device implementation testing

### ⚙️ **Configuration Tests** (4 test files)
Device and interface configuration management:
- **Interface configuration**: IP addressing, status, properties
- **Port channels**: LACP and static aggregation
- **STP configuration**: Spanning tree parameters
- **Device configuration**: Hostname, system settings, NVRAM handling

### 🔗 **Integration Tests** (2 test files)
End-to-end scenarios and architectural validation:
- **Vendor-agnostic architecture**: Common CLI handler patterns
- **Device integration**: Full device lifecycle testing
- **Network topology**: Multi-device network construction
- **Protocol convergence**: Cross-protocol interaction testing

### 📡 **Event System Tests** (3 test files)
Network event handling and notification systems:
- **NetworkEventBus**: Event propagation and subscription
- **Event arguments**: Device/interface/link state changes
- **Comprehensive scenarios**: Complex event interaction patterns

### 🏗️ **Architectural Tests** (10+ test files)
Core system architecture and patterns:
- **VendorContext**: Vendor-specific behavior encapsulation
- **Dependency Injection**: Service registration and resolution
- **CLI handlers**: Vendor-agnostic command processing
- **Physical connections**: Connection quality metrics
- **Network core**: Network topology management

## Vendor Coverage

The test suite validates **15 network equipment vendors**:

| Vendor | Device Types | Test Coverage | CLI Style |
|--------|-------------|---------------|-----------|
| **Cisco** | Router, Switch, Firewall | 🟢 Extensive | IOS/IOS-XE |
| **Juniper** | Router, Switch | 🟢 Comprehensive | JunOS |
| **Arista** | Switch, Router | 🟢 Full | EOS |
| **Nokia** | Router, Switch | 🟢 Complete | SR OS |
| **Huawei** | Router, Switch | 🟢 Full | VRP |
| **Aruba** | Switch, Router | 🟢 Complete | ArubaOS |
| **Fortinet** | Firewall, Router | 🟢 Full | FortiOS |
| **MikroTik** | Router, Switch | 🟢 Complete | RouterOS |
| **Dell** | Switch, Router | 🟢 Basic | OS10 |
| **F5** | Load Balancer | 🟢 Basic | BIG-IP |
| **Extreme** | Switch | 🟢 Basic | EXOS |
| **Broadcom** | Switch | 🟢 Basic | BCM |
| **Alcatel** | Router, Switch | 🟢 Basic | Alcatel OS |
| **Anira** | Custom devices | 🟢 Specialized | Custom CLI |
| **Linux** | Server, Router | 🟢 Basic | Linux CLI |

## Key Test Features

### 🎯 **Realistic CLI Simulation**
- Vendor-specific command syntax and responses
- Context-sensitive help and tab completion
- Error message authenticity
- Configuration mode transitions

### ⚡ **Performance & Scalability**
- In-memory simulation (no actual network traffic)
- Concurrent test execution support
- Large topology handling validation
- Protocol convergence time measurement

### 🛡️ **Reliability & Edge Cases**
- Interface failure scenarios
- Invalid command handling  
- Configuration rollback testing
- Memory leak prevention validation

## Contributing

When adding new tests:

1. **Follow naming conventions**: `{Vendor}_{Scenario}_{ExpectedBehavior}`
2. **Use appropriate test categories**: Place tests in correct namespace folders
3. **Include setup/teardown**: Proper test isolation and cleanup
4. **Document assumptions**: Comment packet sizes, timing expectations
5. **Validate across vendors**: Ensure consistent behavior where expected

## Test Execution Environment

- **Target Framework**: .NET 9.0
- **Test Runner**: XUnit with Visual Studio integration
- **CI/CD Ready**: Fast execution suitable for automated pipelines
- **Memory Efficient**: In-memory simulation without external dependencies
- **Cross-Platform**: Runs on Windows, Linux, macOS

## Performance Metrics

- **Total Tests**: 56 test files with 200+ individual test methods
- **Execution Time**: < 30 seconds for full suite
- **Memory Usage**: < 500MB peak during execution
- **Coverage**: 85%+ code coverage across core simulation components

