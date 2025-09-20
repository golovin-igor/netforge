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

### External Network Connectivity
- **Real IP address binding** for device interfaces
- **External tool integration** (Wireshark, network scanners, monitoring systems)
- **Virtual network interface creation** for seamless host integration
- **Multi-protocol support** (Telnet, SSH, SNMP, HTTP) on actual IP addresses
- **Bridge mode** for connecting simulated networks to real infrastructure
- **Web-based device management** with vendor-specific HTTP handlers

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
│   User Input    │    │  Player Engine   │    │  NetForge Core  │
│                 │    │                  │    │                 │
│ • CLI Commands  │───▶│ • Command Parser │───▶│ • Device Mgmt   │
│ • Terminal      │    │ • Session Mgmt   │    │ • Protocol Sim  │
│ • Web Interface │    │ • Event Handler  │    │ • Network Ops   │
│ • External Tools│    │ • Network Bridge │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │
                       ┌────────▼─────────┐
                       │  Network Bridge  │
                       │                  │
                       │ • Virtual IFs    │
                       │ • IP Binding     │
                       │ • Traffic Proxy  │
                       │ • Protocol Route │
                       └──────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
   ┌────▼────┐             ┌────▼────┐             ┌────▼────┐
   │External │             │External │             │External │
   │Router1  │             │Router2  │             │Switch1  │
   │192.168. │             │192.168. │             │192.168. │
   │100.10   │             │100.11   │             │100.12   │
   └─────────┘             └─────────┘             └─────────┘
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
  "network_connectivity": {
    "enabled": false,
    "mode": "virtual_interfaces",
    "bridge_network": "192.168.100.0/24",
    "gateway": "192.168.100.1",
    "interface_prefix": "netsim",
    "auto_create_interfaces": true,
    "require_admin": true
  },
  "security": {
    "isolate_simulated_network": true,
    "allowed_external_hosts": ["192.168.100.0/24"],
    "enable_firewall": true
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

### External Connectivity Commands
```
enable-network-bridge               # Enable external network bridge
disable-network-bridge              # Disable external connectivity
show-network-bridge                 # Display bridge status and statistics
create-virtual-interface <name>     # Create virtual network interface
delete-virtual-interface <name>     # Remove virtual interface
bind-device-ip <hostname> <ip>      # Bind device to external IP address
unbind-device-ip <hostname>         # Remove IP binding
test-external-access <ip>           # Test connectivity to external IP
show-external-devices               # List externally accessible devices
```

## External Network Connectivity

NetForge.Player supports binding simulated device interfaces to real IP addresses, enabling external tools and systems to connect directly to simulated devices as if they were physical hardware.

### Overview

This feature creates network bridges between the simulated environment and the host system, allowing:
- **Real network tools** (ping, traceroute, Wireshark) to interact with simulated devices
- **External monitoring systems** to collect SNMP data, connect via Telnet/SSH
- **Network automation tools** to manage simulated devices using real IP addresses
- **Hybrid testing scenarios** mixing simulated and physical network components

### Use Cases

#### Network Monitoring and Analysis
- **SNMP Integration**: Connect enterprise monitoring tools (Nagios, Zabbix, PRTG) to simulated devices
- **Traffic Analysis**: Use Wireshark or tcpdump to capture and analyze protocol behavior
- **Performance Testing**: Benchmark network protocols under realistic conditions

#### Automation and Configuration Management
- **Ansible/Puppet Integration**: Test infrastructure automation scripts against simulated networks
- **CI/CD Testing**: Validate network configurations in automated testing pipelines  
- **Training and Certification**: Practice with real network tools on simulated infrastructure

#### Hybrid Network Testing
- **Migration Testing**: Connect simulated devices to existing physical networks during migrations
- **Vendor Evaluation**: Test new network equipment alongside simulated existing infrastructure
- **Disaster Recovery**: Simulate network failures and test recovery procedures with real tools

### Implementation Approaches

#### 1. Virtual Network Interfaces (Recommended)
Creates TAP/TUN interfaces on the host system for each device interface:

```json
{
  "network_connectivity": {
    "mode": "virtual_interfaces",
    "auto_create_interfaces": true,
    "interface_prefix": "netsim",
    "bridge_to_host": true
  }
}
```

**Benefits:**
- Full network stack integration
- Support for all protocols (L2/L3)
- Realistic packet flow and routing
- Works with network analysis tools

**Requirements:**
- Administrator/root privileges
- TAP driver support (Windows: TAP-Windows, Linux/macOS: built-in)

#### 2. Host Network Integration
Binds device IP addresses directly to host network interfaces:

```json
{
  "network_connectivity": {
    "mode": "host_binding",
    "bind_interfaces": ["192.168.100.0/24", "10.0.0.0/8"],
    "exclude_ranges": ["192.168.100.1-192.168.100.10"]
  }
}
```

**Benefits:**
- No additional drivers required
- Direct IP address accessibility
- Lower overhead than virtual interfaces
- Easy integration with existing networks

**Limitations:**
- Requires available IP address space on host
- May conflict with existing network configuration

#### 3. NAT/Proxy Mode
Routes external connections through the Player process:

```json
{
  "network_connectivity": {
    "mode": "nat_proxy",
    "external_interface": "0.0.0.0",
    "port_mapping": {
      "telnet": "23xx",
      "ssh": "22xx", 
      "snmp": "161xx",
      "http": "80xx"
    }
  }
}
```

**Benefits:**
- No special network configuration required
- Works in restricted environments
- Automatic port management
- Built-in security isolation

### Configuration Examples

#### Basic External Connectivity
```json
{
  "name": "External Access Network",
  "network_connectivity": {
    "enabled": true,
    "mode": "virtual_interfaces",
    "bridge_network": "192.168.100.0/24",
    "gateway": "192.168.100.1"
  },
  "devices": [
    {
      "hostname": "Router1",
      "vendor": "cisco",
      "interfaces": [
        {
          "name": "GigabitEthernet0/0",
          "ip_address": "192.168.100.10",
          "subnet_mask": "255.255.255.0",
          "external_accessible": true
        }
      ]
    }
  ]
}
```

#### Multi-Device External Access
```bash
# Create network with external connectivity
create-device cisco Router1 --external-ip 192.168.100.10
create-device juniper Router2 --external-ip 192.168.100.11
create-device arista Switch1 --external-ip 192.168.100.12

# Configure interfaces for external access
connect Router1
configure terminal
interface GigabitEthernet0/1
ip address 192.168.100.10 255.255.255.0
no shutdown
enable-external-access
exit

# Start external connectivity
enable-network-bridge --network 192.168.100.0/24
```

### External Tool Integration

#### Network Analysis with Wireshark
```bash
# Capture traffic on simulated device interfaces
wireshark -i netsim0 -f "host 192.168.100.10"

# Monitor protocol convergence
tshark -i netsim0 -f "ospf or bgp" -V
```

#### SNMP Monitoring
```bash
# Query simulated device via SNMP
snmpwalk -v2c -c public 192.168.100.10 1.3.6.1.2.1.1

# Monitor interface statistics
snmpget -v2c -c public 192.168.100.10 1.3.6.1.2.1.2.2.1.10.1
```

#### Telnet/SSH Access
```bash
# Connect to simulated device via standard tools
telnet 192.168.100.10
ssh admin@192.168.100.10

# Use network automation tools
ansible-playbook -i inventory playbook.yml
```

#### Network Testing Tools
```bash
# Standard network connectivity tests
ping 192.168.100.10
traceroute 192.168.100.10
mtr --report-cycles 10 192.168.100.10

# Port scanning and service discovery
nmap -sS -O 192.168.100.0/24
nmap -sU -p 161 192.168.100.0/24  # SNMP discovery
```

### Architecture Integration

```
┌─────────────────────────────────────────────────────────────┐
│                     External Network                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  Wireshark  │  │ Monitoring  │  │ Automation Tools    │  │
│  │             │  │   Systems   │  │ (Ansible/Puppet)    │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────┬───────────────┬───────────────────┬───────────┘
              │               │                   │
         ┌────▼────┐     ┌────▼────┐         ┌────▼────┐
         │Virtual  │     │Virtual  │         │Virtual  │
         │IF netsim0│     │IF netsim1│       │IF netsim2│
         │192.168. │     │192.168. │         │192.168. │
         │100.10   │     │100.11   │         │100.12   │
         └────┬────┘     └────┬────┘         └────┬────┘
              │               │                   │
┌─────────────▼───────────────▼───────────────────▼───────────┐
│                  NetForge.Player Bridge                     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Router1   │  │   Router2   │  │      Switch1        │  │
│  │   (Cisco)   │  │ (Juniper)   │  │     (Arista)        │  │
│  │             │  │             │  │                     │  │
│  │ Telnet: 23  │  │  SSH: 22    │  │    SNMP: 161        │  │
│  │ SNMP: 161   │  │ SNMP: 161   │  │    HTTP: 80         │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Security Considerations

#### Network Isolation
```json
{
  "security": {
    "isolate_simulated_network": true,
    "firewall_rules": [
      {
        "action": "allow",
        "protocol": "tcp",
        "ports": [22, 23, 80, 443],
        "source": "192.168.100.0/24"
      },
      {
        "action": "allow", 
        "protocol": "udp",
        "ports": [161, 162],
        "source": "192.168.100.0/24"
      },
      {
        "action": "deny",
        "protocol": "all",
        "source": "any"
      }
    ]
  }
}
```

#### Access Control
```json
{
  "access_control": {
    "require_authentication": true,
    "allowed_hosts": ["192.168.100.0/24", "10.0.0.0/8"],
    "rate_limiting": {
      "max_connections_per_ip": 10,
      "max_requests_per_minute": 100
    }
  }
}
```

### Performance Considerations

#### Large Scale Networks
```json
{
  "performance": {
    "max_external_devices": 50,
    "interface_buffer_size": "64KB",
    "enable_traffic_shaping": true,
    "bandwidth_limit_per_device": "100Mbps"
  }
}
```

#### Resource Management
```bash
# Monitor network bridge performance
./NetForge.Player.exe --monitor-network-bridge --stats-interval 10

# Optimize for high-throughput scenarios  
./NetForge.Player.exe --network-mode performance --buffer-size 1MB
```

### Troubleshooting External Connectivity

#### Common Issues

**Virtual Interface Creation Fails**
```
Error: Failed to create TAP interface
Solution: Run as administrator and install TAP driver
```

**IP Address Conflicts**
```
Error: Address 192.168.100.10 already in use
Solution: Check for IP conflicts and modify configuration
```

**External Tool Connection Timeouts**
```
Error: Connection timeout to simulated device
Solution: Verify bridge is active and device protocols are enabled
```

#### Debug Commands
```bash
# Check bridge status
./NetForge.Player.exe --show-network-bridge --verbose

# Test external connectivity
./NetForge.Player.exe --test-external-access --target-ip 192.168.100.10

# Monitor bridge traffic
./NetForge.Player.exe --monitor-bridge-traffic --interface netsim0
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
- **Vendor-specific web management** interfaces (Cisco IOS-style, Juniper J-Web, Arista EOS, etc.)
- **REST API access** with OpenAPI documentation
- **Authentication and session management**

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
