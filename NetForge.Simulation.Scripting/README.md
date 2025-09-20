# NetForge.Simulation.Scripting

A comprehensive scripting engine for automating network simulation, configuration, and testing in NetForge environments. The NetSim format provides a declarative, human-readable syntax for creating complex network scenarios and validation tests.

## Overview

NetForge.Simulation.Scripting enables network engineers and developers to:

- **Automate network topology creation** with declarative scripts
- **Batch configure multiple devices** using vendor-specific CLI commands  
- **Create repeatable test scenarios** with validation and reporting
- **Integrate with CI/CD pipelines** for infrastructure testing
- **Build reusable configuration templates** and procedure libraries

## Features

### 🚀 Powerful Scripting Language
- **Variable system** with interpolation and strong typing
- **Control flow** (if/else, loops, functions, templates)
- **Built-in functions** for networking operations (ping, configure, test)
- **Error handling** with try/catch and retry mechanisms
- **Parallel execution** for performance optimization

### 🌐 Network Automation
- **Multi-vendor device support** (Cisco, Juniper, Arista, Nokia, etc.)
- **Intelligent command routing** based on device vendor and context
- **Configuration templates** for common scenarios and best practices
- **Link creation and management** with physical properties
- **Protocol configuration** (OSPF, BGP, VRRP, STP, etc.)

### 🧪 Testing and Validation
- **Connectivity testing** with ping, traceroute, and custom tests
- **Protocol state validation** (neighbor relationships, route tables)
- **Performance testing** with traffic generation and measurement
- **Assertion framework** for automated pass/fail validation
- **Test reporting** with detailed results and metrics

### 🔧 Integration and Extensibility
- **NetForge.Player integration** for seamless script execution
- **HTTP Handler integration** for web-based device management
- **External tool integration** (REST APIs, databases, file I/O)
- **Plugin system** for custom functions and commands
- **Import/export** capabilities for script modularity
- **Command-line interface** for automated execution

## Quick Start

### Installation

```bash
# Install as NuGet package
dotnet add package NetForge.Simulation.Scripting

# Or clone and build from source
git clone https://github.com/yourorg/NetForge.git
cd NetForge/NetForge.Simulation.Scripting
dotnet build
```

### Basic Script Example

```netsim
# simple_network.netsim
@name "Basic Two-Router Setup"
@description "Simple WAN connection with OSPF"

# Variables
$wan_subnet = "10.0.0.0/30"
$hq_lan = "192.168.1.0/24"
$branch_lan = "192.168.2.0/24"

# Create devices
create-device cisco HQ-R1
create-device cisco Branch-R1

# Create WAN link
link HQ-R1 Serial0/0/0 Branch-R1 Serial0/0/0 {
  bandwidth: 1544,
  encapsulation: "ppp"
}

# Configure HQ router
configure HQ-R1 {
  "hostname HQ-R1"
  "interface Serial0/0/0"
  "ip address 10.0.0.1 255.255.255.252"
  "no shutdown"
  "exit"
  "router ospf 1"
  "network 10.0.0.0 0.0.0.3 area 0"
  "network 192.168.1.0 0.0.0.255 area 0"
}

# Configure Branch router
configure Branch-R1 {
  "hostname Branch-R1"
  "interface Serial0/0/0"
  "ip address 10.0.0.2 255.255.255.252"
  "no shutdown"
  "exit"
  "router ospf 1" 
  "network 10.0.0.0 0.0.0.3 area 0"
  "network 192.168.2.0 0.0.0.255 area 0"
}

# Test connectivity
test connectivity {
  wait 30  # Allow OSPF convergence
  
  ping HQ-R1 Branch-R1 --count 5
  assert ping.success {
    message: "WAN connectivity failed"
    severity: critical
  }
  
  assert ospf_neighbors(HQ-R1).count >= 1 {
    message: "OSPF neighbor relationship not established"
  }
}

print "Network setup completed successfully!"
```

### Running Scripts

```bash
# Command-line execution
netsim-runner script.netsim

# With variables
netsim-runner script.netsim --var subnet=192.168.100.0/24

# Debug mode
netsim-runner script.netsim --debug --timeout 600

# Integration with NetForge.Player
./NetForge.Player.exe --script network_setup.netsim --execute
```

## Language Syntax

### Variables and Data Types

```netsim
# Basic types
$hostname = "Router-01"
$vlan_id = 100
$enabled = true
$ip_address = 192.168.1.1
$network = 10.0.0.0/24

# Collections
$interfaces = ["GigabitEthernet0/0", "GigabitEthernet0/1"]
$device_config = {
  "hostname": "R1",
  "mgmt_ip": "192.168.100.1",
  "location": "HQ"
}

# String interpolation
$interface_desc = "Link to ${remote_device} ${remote_interface}"
```

### Control Flow

```netsim
# Conditional execution
if ($enable_ospf) {
  configure R1 {
    "router ospf 1"
    "network 192.168.1.0 0.0.0.255 area 0"
  }
}

# Loops
for $vlan in [10, 20, 30] {
  configure SW1 {
    "vlan ${vlan}"
    "name VLAN_${vlan}"
  }
}

# Functions
function configure_basic_security($device, $enable_password) {
  configure $device {
    "enable secret ${enable_password}"
    "service password-encryption"
    "no ip http server"
  }
}
```

### HTTP Handler Integration

```netsim
# Web interface testing
test web_interface {
  # Test HTTP connectivity
  http_test R1 {
    url: "http://192.168.1.1/api/devices/info",
    method: "GET",
    expect_status: 200
  }

  # Test REST API configuration
  http_configure R1 {
    endpoint: "/api/interfaces/configure",
    method: "POST",
    data: {
      interfaceName: "GigabitEthernet0/0",
      ipAddress: "192.168.1.1",
      subnetMask: "255.255.255.0"
    }
  }

  # Test web interface accessibility
  assert web_accessible(R1) and web_accessible(R2)
}

# Combined CLI and HTTP testing
test hybrid_management {
  # Configure via CLI
  configure R1 {
    "interface GigabitEthernet0/0"
    "ip address 192.168.1.1 255.255.255.0"
    "no shutdown"
  }

  # Verify via HTTP API
  http_test R1 {
    url: "http://192.168.1.1/api/interfaces/GigabitEthernet0/0",
    expect_data: {
      ipAddress: "192.168.1.1",
      isUp: true
    }
  }
}
```

### Testing Framework

```netsim
# Comprehensive testing
test network_validation {
  # Connectivity tests
  ping R1 R2 --count 5 --timeout 1000
  assert ping.success and ping.avg_time < 50

  # Protocol validation
  assert ospf_neighbors(R1).count >= 2
  assert bgp_sessions(R1).all(s => s.state == "Established")

  # Configuration compliance
  assert snmp_enabled(R1) and ntp_configured(R1)
}

# Performance testing
test performance {
  traffic_generator {
    source: R1,
    destination: R2,
    bandwidth: "10Mbps",
    duration: 60
  }

  measure throughput R1 R2 {
    min_throughput: "8Mbps",
    max_latency: "10ms"
  }
}
```

## Architecture

### Core Components

- **Lexer**: Tokenizes NetSim scripts into language tokens
- **Parser**: Converts tokens into Abstract Syntax Tree (AST)
- **Semantic Analyzer**: Validates types, scopes, and semantics
- **Interpreter**: Executes parsed scripts with runtime environment
- **Built-in Functions**: Comprehensive library of networking functions

### Integration Layer

- **NetForge.Player Integration**: Direct integration with simulation engine
- **HTTP Handler Integration**: Web-based device management and REST API access
- **Device Abstraction**: Vendor-agnostic device management
- **Protocol Support**: Full protocol configuration and monitoring
- **External APIs**: REST, database, and file system integration

### Extensibility

- **Plugin System**: Custom function libraries and extensions
- **Template Engine**: Reusable configuration templates
- **Import/Export**: Script modularity and library management
- **Event System**: Script lifecycle and execution events

## Project Structure

```
NetForge.Simulation.Scripting/
├── Core/
│   ├── Lexer/              # Tokenization and lexical analysis
│   ├── Parser/             # Syntax parsing and AST generation
│   ├── Interpreter/        # Script execution engine
│   └── Runtime/            # Runtime environment and context
├── Language/
│   ├── Syntax/             # Grammar definitions
│   ├── Semantics/          # Type checking and validation  
│   └── Builtins/           # Built-in function library
├── Integration/
│   ├── NetForge/           # NetForge.Player integration
│   ├── Devices/            # Device management layer
│   └── Testing/            # Testing framework
├── Extensions/
│   ├── Libraries/          # Standard procedure libraries
│   ├── Templates/          # Configuration templates
│   └── Plugins/            # Plugin framework
├── Tools/
│   ├── CLI/                # Command-line script runner
│   ├── Debugger/           # Script debugging tools
│   └── Formatter/          # Code formatting utilities
├── Examples/               # Example scripts and templates
├── Tests/                  # Comprehensive test suite
└── Documentation/          # API docs and usage guides
```

## Documentation

- **[NetSim Scripting Format](NETSIM_SCRIPTING_FORMAT.md)** - Complete language specification
- **[Implementation Plan](NETSIM_FORMAT_PARSER_IMPLEMENTATION_PLAN.md)** - Development roadmap
- **[API Reference](docs/api/README.md)** - Complete API documentation
- **[Examples](Examples/README.md)** - Script examples and templates
- **[Integration Guide](docs/integration/README.md)** - NetForge.Player integration

## Contributing

We welcome contributions to NetForge.Simulation.Scripting! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details on:

- Code style and conventions
- Testing requirements
- Pull request process
- Issue reporting

## Development Setup

```bash
# Clone repository
git clone https://github.com/yourorg/NetForge.git
cd NetForge/NetForge.Simulation.Scripting

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run tests
dotnet test

# Run example scripts
dotnet run --project Tools/CLI -- Examples/Basic/simple_network.netsim
```

## Performance

- **Parsing Speed**: 1000+ lines per second
- **Memory Usage**: < 100MB for typical scripts  
- **Execution**: Near real-time for most operations
- **Concurrency**: Parallel execution support
- **Scalability**: Handles large network topologies efficiently

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Documentation**: Complete guides and API reference
- **Examples**: Extensive script library and templates
- **Community**: GitHub discussions and issue tracking
- **Enterprise**: Commercial support available

---

*NetForge.Simulation.Scripting - Automating Network Simulation and Testing*