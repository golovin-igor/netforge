# NetForge.Player

The NetForge Player is the standalone application for running and managing network simulations using the NetForge framework. It provides an interactive environment for creating, configuring, and testing virtual network topologies.

## Overview

NetForge.Player serves as the primary user interface and runtime environment for network simulations. It allows users to:

- **Load network topologies** from various sources
- **Create and manage virtual devices** from multiple vendors
- **Configure realistic network scenarios** with CLI simulation
- **Run network tests** and validate configurations
- **Monitor network behavior** in real-time

## Features

### Interactive Network Management
- **Device creation** and configuration through authentic CLI interfaces
- **Network topology visualization** and management
- **Real-time protocol simulation** with state monitoring
- **Configuration persistence** and scenario management

### Multi-Vendor Device Support
- **15+ supported vendors** with realistic CLI behavior
- **Cisco, Juniper, Arista, Nokia, Huawei** and many more
- **Vendor-specific features** and command syntax
- **Authentic device responses** and error handling

### Advanced Simulation Capabilities
- **Physical layer simulation** with link quality modeling
- **Protocol convergence** and routing table updates
- **Network event simulation** (link failures, device restarts)
- **Performance monitoring** and statistics collection

### Terminal and Remote Access
- **Built-in terminal emulator** for device access
- **Telnet/SSH simulation** for realistic remote access
- **WebSocket support** for web-based interfaces
- **Multi-session management** with authentication

## Getting Started

### Prerequisites
- .NET 6.0 or later runtime
- Windows, Linux, or macOS operating system

### Installation
```bash
# Build the player application
dotnet build NetForge.Player/

# Run the application
dotnet run --project NetForge.Player/
```

### Basic Usage

1. **Start the Player**
   ```bash
   ./NetForge.Player.exe
   ```

2. **Create a Simple Network**
   ```
   > create-device cisco Router1
   > create-device juniper Router2
   > link Router1 GigabitEthernet0/0 Router2 ge-0/0/0
   ```

3. **Configure Devices**
   ```
   > connect Router1
   Router1> enable
   Router1# configure terminal
   Router1(config)# interface GigabitEthernet0/0
   Router1(config-if)# ip address 10.0.0.1 255.255.255.0
   Router1(config-if)# no shutdown
   ```

4. **Test Connectivity**
   ```
   Router1# ping 10.0.0.2
   ```

## Application Architecture

### Core Components

#### **Simulation Engine**
- **Network management** and topology handling
- **Device lifecycle** management and state tracking
- **Protocol orchestration** and convergence control
- **Event system** for real-time updates

#### **CLI Interface**
- **Command parser** and execution engine
- **Multi-vendor CLI** simulation and context management
- **Help system** and command completion
- **Configuration validation** and error reporting

#### **Terminal Services**
- **Local terminal** for direct device access
- **Network terminal server** for remote connections
- **WebSocket integration** for web applications
- **Session management** and authentication

#### **Data Management**
- **Topology persistence** and scenario storage
- **Configuration management** with versioning
- **State snapshots** and restoration
- **Export/import** capabilities

### Application Flow

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   User Input    │    │  Player Engine   │    │  NetForge Core    │
│                 │    │                  │    │                 │
│ • CLI Commands  │───▶│ • Command Parser │───▶│ • Device Mgmt   │
│ • Terminal      │    │ • Session Mgmt   │    │ • Protocol Sim  │
│ • Web Interface │    │ • Event Handler  │    │ • Network Ops   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## Configuration Files

### Player Configuration
```json
{
  "simulation": {
    "auto_save": true,
    "save_interval": 300,
    "max_devices": 100
  },
  "terminal": {
    "enable_telnet": true,
    "telnet_port": 2323,
    "enable_websocket": true,
    "websocket_port": 8080
  },
  "logging": {
    "level": "Info",
    "file_path": "netsim.log"
  }
}
```

### Topology Definition
```json
{
  "name": "Sample Network",
  "devices": [
    {
      "hostname": "Router1",
      "vendor": "cisco",
      "type": "router",
      "interfaces": [
        {
          "name": "GigabitEthernet0/0",
          "ip_address": "10.0.0.1",
          "subnet_mask": "255.255.255.0"
        }
      ]
    }
  ],
  "links": [
    {
      "device1": "Router1",
      "interface1": "GigabitEthernet0/0",
      "device2": "Router2", 
      "interface2": "ge-0/0/0"
    }
  ]
}
```

## Command Reference

### Network Management Commands
```
create-device <vendor> <hostname>    # Create a new device
delete-device <hostname>             # Remove a device
list-devices                         # Show all devices
link <dev1> <int1> <dev2> <int2>    # Create link between devices
unlink <dev1> <int1>                # Remove link from interface
```

### Session Management Commands
```
connect <hostname>                   # Connect to device CLI
disconnect                          # Disconnect from current device
show-sessions                       # List active sessions
```

### Simulation Control Commands
```
start-simulation                     # Begin network simulation
stop-simulation                     # Pause network simulation
update-protocols                    # Force protocol convergence
show-topology                       # Display network topology
```

### Scenario Management Commands
```
save-scenario <filename>            # Save current network state
load-scenario <filename>            # Load network scenario
list-scenarios                      # Show saved scenarios
```

## Advanced Features

### Scripting Support
NetForge.Player supports automation through script files:

```bash
# network_setup.netsim
create-device cisco R1
create-device cisco R2
link R1 GigabitEthernet0/0 R2 GigabitEthernet0/0

connect R1
enable
configure terminal
interface GigabitEthernet0/0
ip address 192.168.1.1 255.255.255.252
no shutdown
exit
router ospf 1
network 192.168.1.0 0.0.0.3 area 0
exit
disconnect

# Run script
./NetForge.Player.exe --script network_setup.netsim
```

### Remote API
The Player exposes REST API endpoints for integration:

```http
GET /api/devices                    # List devices
POST /api/devices                   # Create device
GET /api/devices/{id}/config        # Get device configuration
POST /api/devices/{id}/commands     # Execute commands
GET /api/topology                   # Get network topology
```

### Web Interface
Access the built-in web interface at `http://localhost:8080` for:
- **Graphical topology view** with drag-and-drop editing
- **Web-based terminal** for device access
- **Real-time monitoring** dashboards
- **Configuration wizards** for common scenarios

## Troubleshooting

### Common Issues

**Device Creation Fails**
```
Error: Unable to create device 'Router1'
Solution: Check vendor name is correct and supported
```

**Link Creation Fails**
```
Error: Interface 'GigabitEthernet0/0' not found
Solution: Verify interface name matches device type
```

**Connection Timeout**
```
Error: Unable to connect to device
Solution: Check device is running and accessible
```

### Debug Mode
Enable debug logging for detailed troubleshooting:
```bash
./NetForge.Player.exe --debug --log-level Debug
```

## Performance Tuning

### Large Networks
For simulations with many devices:
- Increase memory allocation: `--memory 4GB`
- Adjust update intervals: `--update-interval 5000`
- Disable unnecessary protocols: `--disable-cdp --disable-lldp`

### Resource Monitoring
Monitor resource usage:
```bash
./NetForge.Player.exe --monitor-resources --stats-interval 30
```

## Integration

### With CI/CD Systems
```yaml
# Example GitHub Actions workflow
- name: Test Network Configuration
  run: |
    ./NetForge.Player.exe --script test_scenario.netsim
    ./NetForge.Player.exe --verify-connectivity
    ./NetForge.Player.exe --export-results results.json
```

### With Monitoring Systems
Export metrics to external systems:
```bash
./NetForge.Player.exe --export-metrics --format prometheus
```

## License

This project is for educational and testing purposes.