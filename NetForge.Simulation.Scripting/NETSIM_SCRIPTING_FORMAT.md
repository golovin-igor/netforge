# NetSim Scripting Format (.netsim)

The NetSim scripting format provides a declarative, human-readable syntax for automating network simulation tasks, device configuration, and testing scenarios in NetForge.Player.

## Overview

NetSim scripts (`.netsim` files) enable:
- **Automated network topology creation** and device provisioning
- **Batch device configuration** with vendor-specific CLI commands
- **Test scenario execution** with validation and reporting
- **Reproducible network environments** for training and development
- **CI/CD integration** for network infrastructure testing

## Language Syntax

### Basic Structure

```netsim
# Network Setup Script
# Comments begin with # and continue to end of line

# Metadata (optional)
@name "Corporate WAN Test Scenario"
@description "Multi-vendor WAN setup with OSPF and BGP"
@author "Network Engineering Team"
@version "1.2.0"
@created "2025-01-20"

# Variable definitions
$subnet_wan = "10.0.0.0/30"
$subnet_lan1 = "192.168.1.0/24"  
$subnet_lan2 = "192.168.2.0/24"
$ospf_area = 0
$bgp_asn = 65001

# Commands and configurations follow
create-device cisco HQ-Router
create-device juniper Branch-Router
```

### Comments and Documentation

```netsim
# Single-line comment

/*
  Multi-line comment block
  Useful for detailed explanations
  and temporary code disabling
*/

#! Shebang-style comment for script execution metadata
#! requires: NetForge.Player >= 2.0.0
#! timeout: 300
```

### Variables and Data Types

#### Variable Declaration
```netsim
# String variables
$hostname = "Router-01"
$interface = "GigabitEthernet0/0"
$description = "WAN uplink to provider"

# Numeric variables  
$vlan_id = 100
$mtu = 1500
$bandwidth = 1000000
$metric = 10

# Boolean variables
$enable_ospf = true
$shutdown_interface = false
$debug_mode = true

# IP Address and Network variables
$ip_address = 192.168.1.1
$subnet_mask = 255.255.255.0
$network = 10.0.0.0/24
$gateway = 192.168.1.254

# Array variables
$interfaces = ["GigabitEthernet0/0", "GigabitEthernet0/1", "Serial0/0/0"]
$vlans = [10, 20, 30, 40]
$ip_pools = ["192.168.1.0/24", "192.168.2.0/24", "192.168.3.0/24"]

# Dictionary/Map variables
$device_ips = {
  "HQ-Router": "10.0.0.1",
  "Branch1-Router": "10.0.0.5", 
  "Branch2-Router": "10.0.0.9"
}

$interface_config = {
  "name": "GigabitEthernet0/0",
  "ip": "192.168.1.1",
  "mask": "255.255.255.0",
  "description": "LAN interface"
}
```

#### Variable Interpolation
```netsim
$base_ip = "192.168.1"
$device_id = 1

# String interpolation with ${} syntax
$full_ip = "${base_ip}.${device_id}"
$hostname = "Router-${device_id:02d}"  # Formatting: Router-01

# Command interpolation
create-device cisco ${hostname}
configure-interface ${hostname} GigabitEthernet0/0 ip ${full_ip} 255.255.255.0
```

### Device Management Commands

#### Device Creation and Management
```netsim
# Basic device creation
create-device <vendor> <hostname> [options]

# Examples
create-device cisco HQ-Router
create-device juniper Branch-Router --location "New York"  
create-device arista Core-Switch --rack "A1" --slot 2

# Device with initial configuration
create-device cisco R1 {
  location: "Data Center A",
  management_ip: "192.168.100.10",
  snmp_community: "monitoring",
  enable_protocols: ["ospf", "bgp", "snmp"]
}

# Device deletion
delete-device <hostname>

# Device modification
modify-device HQ-Router {
  description: "Primary headquarters router",
  contact: "neteng@company.com"
}
```

#### Interface Management
```netsim
# Create physical links between devices
link <device1> <interface1> <device2> <interface2> [options]

# Examples
link HQ-Router GigabitEthernet0/0 Branch-Router ge-0/0/0
link Core-Switch Ethernet1/1 Access-Switch GigabitEthernet0/24 --speed 1000

# Link with properties
link R1 Serial0/0/0 R2 Serial0/0/0 {
  bandwidth: 1544,        # T1 line
  delay: 20000,          # microseconds
  reliability: 255,      # 0-255 scale
  load: 1,               # 0-255 scale
  encapsulation: "ppp"
}

# Remove links
unlink HQ-Router GigabitEthernet0/0

# Configure interface properties
configure-interface <device> <interface> [properties]
configure-interface R1 GigabitEthernet0/0 {
  ip: "192.168.1.1",
  mask: "255.255.255.0", 
  description: "LAN segment",
  mtu: 1500,
  duplex: "full",
  speed: 1000
}
```

### Device Configuration

#### CLI Command Execution
```netsim
# Connect to device and execute commands
configure <device> {
  # Cisco-style commands (automatic vendor detection)
  "enable"
  "configure terminal"
  "hostname ${hostname}"
  "interface GigabitEthernet0/0"
  "ip address 192.168.1.1 255.255.255.0"  
  "no shutdown"
  "exit"
  "router ospf 1"
  "network 192.168.1.0 0.0.0.255 area 0"
  "exit"
  "exit"
}

# Multi-vendor configuration with vendor blocks
configure-multi {
  cisco: {
    "enable"
    "configure terminal"  
    "router bgp ${bgp_asn}"
    "neighbor ${neighbor_ip} remote-as ${remote_asn}"
  }
  
  juniper: {
    "configure"
    "set protocols bgp group external neighbor ${neighbor_ip} peer-as ${remote_asn}"
    "commit"
  }
  
  arista: {
    "configure"
    "router bgp ${bgp_asn}"
    "neighbor ${neighbor_ip} remote-as ${remote_asn}"
    "exit"
  }
}
```

#### Template-based Configuration
```netsim
# Define reusable configuration templates
template ospf_config($area_id, $networks[]) {
  "router ospf 1"
  for $network in $networks {
    "network ${network.address} ${network.wildcard} area ${area_id}"
  }
  "exit"
}

template vlan_config($vlan_id, $name, $interfaces[]) {
  "vlan ${vlan_id}"
  "name ${name}"
  "exit"
  for $interface in $interfaces {
    "interface ${interface}"
    "switchport mode access"
    "switchport access vlan ${vlan_id}"
    "exit"
  }
}

# Use templates
configure R1 {
  "enable"
  "configure terminal"
  @ospf_config(0, [
    {address: "192.168.1.0", wildcard: "0.0.0.255"},
    {address: "10.0.0.0", wildcard: "0.0.0.3"}
  ])
}
```

### Control Flow and Logic

#### Conditional Execution
```netsim
# If-else statements
if ($enable_ospf == true) {
  configure R1 {
    "router ospf 1" 
    "network 192.168.1.0 0.0.0.255 area 0"
  }
} else {
  configure R1 {
    "router rip"
    "network 192.168.1.0"
  }
}

# Switch statements for vendor-specific logic
switch ($device.vendor) {
  case "cisco":
    configure ${device.name} {
      "enable"
      "configure terminal"
      "ip cef"
    }
  case "juniper":
    configure ${device.name} {
      "configure"
      "set forwarding-options packet-mode"
    }
  default:
    print "Unsupported vendor: ${device.vendor}"
}
```

#### Loops and Iteration
```netsim
# For loops with arrays
$vlans = [10, 20, 30, 40]
for $vlan in $vlans {
  configure SW1 {
    "vlan ${vlan}"
    "name VLAN_${vlan}"
    "exit"
  }
}

# For loops with ranges  
for $i in range(1, 5) {
  $hostname = "Router${i:02d}"
  create-device cisco ${hostname}
  
  configure ${hostname} {
    "hostname ${hostname}"
    "interface loopback0"
    "ip address 192.168.${i}.1 255.255.255.255"
  }
}

# While loops with conditions
$subnet_octet = 1
while ($subnet_octet <= 10) {
  $network = "192.168.${subnet_octet}.0/24"
  configure R1 {
    "ip route ${network} null0"
  }
  $subnet_octet = $subnet_octet + 1
}

# Foreach with device collections
foreach $device in devices {
  if ($device.vendor == "cisco") {
    configure ${device.name} {
      "snmp-server community public ro"
      "snmp-server contact netops@company.com"
    }
  }
}
```

### Testing and Validation

#### Test Assertions
```netsim
# Connectivity tests
test connectivity {
  ping HQ-Router Branch-Router --count 5 --timeout 5
  
  assert ping.success == true {
    message: "Branch router unreachable from HQ"
    severity: critical
  }
  
  assert ping.avg_time < 100 {
    message: "High latency detected: ${ping.avg_time}ms"
    severity: warning  
  }
}

# Protocol state verification
test ospf_convergence {
  wait 30  # Allow time for convergence
  
  assert ospf_neighbors(HQ-Router).count >= 2 {
    message: "Insufficient OSPF neighbors"
  }
  
  assert ospf_routes(HQ-Router).contains("192.168.2.0/24") {
    message: "Branch2 subnet not in OSPF database"
  }
}

# Configuration validation
test device_compliance {
  foreach $device in devices {
    assert snmp_enabled($device) {
      message: "SNMP not configured on ${device.name}"
    }
    
    assert ntp_configured($device) {
      message: "NTP not configured on ${device.name}"
    }
  }
}
```

#### Performance Testing
```netsim
# Traffic generation and measurement
test performance {
  # Generate traffic between devices
  traffic_generator {
    source: HQ-Router,
    destination: Branch-Router,
    protocol: tcp,
    bandwidth: "10Mbps",
    duration: 60
  }
  
  # Measure and validate performance
  measure bandwidth HQ-Router Branch-Router {
    min_throughput: "8Mbps",
    max_latency: "50ms",
    max_jitter: "5ms"
  }
}

# Scalability testing
test scalability {
  $max_devices = 100
  for $i in range(1, $max_devices) {
    create-device cisco TestDevice${i}
    link TestDevice${i} GigabitEthernet0/0 CoreSwitch Ethernet1/${i}
    
    # Monitor resource usage
    if (memory_usage() > 90) {
      print "Memory usage critical at ${i} devices"
      break
    }
  }
  
  assert devices.count == $max_devices {
    message: "Failed to create all test devices"
  }
}
```

### Functions and Procedures

#### Custom Functions
```netsim
# Function definitions
function configure_basic_security($device, $enable_password, $console_password) {
  configure $device {
    "enable secret ${enable_password}"
    "line console 0"
    "password ${console_password}"
    "login"
    "exit"
    "line vty 0 4"
    "password ${console_password}"
    "login"
    "transport input telnet ssh"
    "exit"
  }
}

function create_vlan($switch, $vlan_id, $name, $interfaces[]) {
  configure $switch {
    "vlan ${vlan_id}"
    "name ${name}"
    "exit"
  }
  
  for $interface in $interfaces {
    configure $switch {
      "interface ${interface}"
      "switchport mode access"
      "switchport access vlan ${vlan_id}"
      "exit"
    }
  }
}

# Function with return values
function calculate_subnet($base_ip, $subnet_bits) -> string {
  return "${base_ip}/${subnet_bits}"
}

# Function calls
configure_basic_security(R1, "cisco123", "console123")
create_vlan(SW1, 100, "Finance", ["FastEthernet0/1", "FastEthernet0/2"])

$finance_subnet = calculate_subnet("192.168.100.0", 24)
```

#### Procedure Libraries
```netsim
# Import external procedure libraries
import "common/cisco_procedures.netsim"
import "templates/wan_configs.netsim" as wan
import "testing/validation_suite.netsim"

# Use imported procedures
cisco.configure_ospf_area(R1, 0, ["192.168.1.0/24"])
wan.setup_t1_connection(R1, "Serial0/0/0", R2, "Serial0/0/0")
validation.test_full_connectivity()
```

### Event Handling and Monitoring

#### Event Triggers
```netsim
# Event handler definitions
on device_created($device) {
  print "Device ${device.name} created successfully"
  
  # Apply standard configuration
  configure_basic_security($device, "default123", "console123")
  
  # Enable SNMP monitoring
  configure $device {
    "snmp-server community public ro"
    "snmp-server location ${device.location}"
  }
}

on link_state_changed($link, $old_state, $new_state) {
  if ($new_state == "down") {
    print "ALERT: Link ${link.device1}:${link.interface1} -> ${link.device2}:${link.interface2} went down"
    
    # Trigger convergence testing
    test convergence_after_failure {
      wait 60
      assert all_routes_converged() {
        message: "Routes did not converge after link failure"
      }
    }
  }
}

on protocol_neighbor_changed($device, $protocol, $neighbor, $state) {
  print "${protocol} neighbor ${neighbor} on ${device} changed to ${state}"
}
```

#### Monitoring and Logging
```netsim
# Continuous monitoring tasks
monitor interface_utilization {
  devices: all,
  interval: 30,  # seconds
  
  alert_if utilization > 80 {
    message: "High interface utilization on ${device}:${interface}: ${utilization}%"
    severity: warning
  }
  
  alert_if utilization > 95 {
    message: "Critical interface utilization on ${device}:${interface}: ${utilization}%"  
    severity: critical
  }
}

monitor protocol_states {
  protocols: ["ospf", "bgp"],
  interval: 60,
  
  alert_if neighbor_count < expected_neighbors {
    message: "Missing ${protocol} neighbors on ${device}"
  }
}

# Custom logging
log "Starting network deployment script" level=info
log "Critical error in device configuration" level=error device=R1
```

### File I/O and External Integration

#### File Operations
```netsim
# Read configuration from external files
$devices_config = read_json("devices.json")
$ip_assignments = read_csv("ip_allocations.csv")

# Generate output files
write_file "deployment_report.txt" {
  "Network Deployment Summary"
  "=========================="
  "Devices created: ${devices.count}"
  "Links established: ${links.count}"
  "Tests passed: ${tests_passed}/${tests_total}"
}

# Export network state
export_topology "final_topology.json" format=json
export_configurations "device_configs/" format=cisco_ios
```

#### External System Integration
```netsim
# REST API calls
$monitoring_data = http_get("http://monitoring.company.com/api/devices")

http_post("http://cmdb.company.com/api/devices", {
  name: R1.name,
  ip_address: R1.management_ip,
  location: R1.location,
  vendor: R1.vendor
})

# Database operations
$device_info = sql_query("SELECT * FROM devices WHERE location='HQ'")

# External command execution
$ping_result = exec("ping -c 4 192.168.1.1")
assert $ping_result.exit_code == 0
```

### Advanced Features

#### Parallel Execution
```netsim
# Parallel device creation
parallel {
  create-device cisco R1
  create-device cisco R2  
  create-device cisco R3
  create-device cisco R4
}

# Parallel configuration with synchronization
parallel {
  task configure_hq {
    configure HQ-Router {
      "router ospf 1"
      "network 192.168.1.0 0.0.0.255 area 0"
    }
  }
  
  task configure_branch1 {
    configure Branch1-Router {
      "router ospf 1" 
      "network 192.168.2.0 0.0.0.255 area 0"
    }
  }
}

# Wait for all parallel tasks to complete
wait_all()

# Synchronization barrier
barrier "ospf_configured" {
  participants: ["HQ-Router", "Branch1-Router", "Branch2-Router"]
}
```

#### Error Handling
```netsim
# Try-catch blocks
try {
  create-device cisco ProblemDevice
  configure ProblemDevice {
    "enable"
    "configure terminal"
    "invalid command here"  # This will fail
  }
} catch (ConfigurationError $e) {
  print "Configuration failed: ${e.message}"
  # Cleanup or alternative action
  delete-device ProblemDevice
} catch (DeviceError $e) {
  print "Device error: ${e.message}"
} finally {
  print "Configuration attempt completed"
}

# Retry logic
retry 3 {
  link R1 GigabitEthernet0/0 R2 GigabitEthernet0/0
} on_failure {
  print "Failed to create link after 3 attempts"
}
```

#### Script Execution Control
```netsim
# Script execution metadata
#! requires: NetForge.Player >= 2.0.0
#! timeout: 600
#! max_memory: 2GB
#! allow_external_commands: false

# Execution modes
@execution_mode parallel  # or sequential
@max_parallel_tasks 5
@continue_on_error false

# Script dependencies
@requires_scripts [
  "common/base_setup.netsim",
  "templates/security_baseline.netsim"
]

# Environment requirements
@requires_environment {
  min_memory: "1GB",
  network_connectivity: true,
  admin_privileges: true
}
```

## Script Examples

### Simple Network Setup
```netsim
# simple_wan.netsim
@name "Simple WAN Connection"
@description "Basic two-router WAN setup with OSPF"

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
  "enable"
  "configure terminal"
  "hostname HQ-R1"
  "interface Serial0/0/0"
  "ip address 10.0.0.1 255.255.255.252"
  "no shutdown"
  "exit"
  "router ospf 1"
  "network 10.0.0.0 0.0.0.3 area 0"
  "exit"
}

# Configure Branch router  
configure Branch-R1 {
  "enable"
  "configure terminal"
  "hostname Branch-R1"
  "interface Serial0/0/0"
  "ip address 10.0.0.2 255.255.255.252"
  "no shutdown" 
  "exit"
  "router ospf 1"
  "network 10.0.0.0 0.0.0.3 area 0"
  "exit"
}

# Test connectivity
test connectivity {
  wait 30  # Allow OSPF convergence
  ping HQ-R1 Branch-R1 --count 5
  
  assert ping.success {
    message: "WAN connectivity failed"
  }
}

print "Simple WAN setup completed successfully!"
```

### Enterprise Network Deployment
```netsim
# enterprise_deployment.netsim
@name "Enterprise Network Deployment"
@description "Multi-site enterprise network with OSPF and BGP"
@version "2.1.0"

# Configuration variables
$hq_subnet = "192.168.1.0/24"
$branch1_subnet = "192.168.2.0/24"  
$branch2_subnet = "192.168.3.0/24"
$wan_subnet = "10.0.0.0/24"
$ospf_area = 0
$bgp_asn = 65001

# Device templates
template enterprise_router($hostname, $mgmt_ip) {
  create-device cisco ${hostname} {
    management_ip: $mgmt_ip,
    location: "Enterprise",
    snmp_community: "monitoring"
  }
  
  configure ${hostname} {
    "hostname ${hostname}"
    "enable secret cisco123"
    "service password-encryption"
    "ip cef"
    "snmp-server community monitoring ro"
    "ntp server 192.168.1.10"
  }
}

# Create enterprise infrastructure
enterprise_router("HQ-Core-R1", "192.168.100.1")
enterprise_router("Branch1-R1", "192.168.100.2") 
enterprise_router("Branch2-R1", "192.168.100.3")

create-device cisco HQ-Dist-SW1 {
  management_ip: "192.168.100.10",
  location: "HQ Distribution"
}

create-device cisco HQ-Access-SW1 {
  management_ip: "192.168.100.11", 
  location: "HQ Access"
}

# Create network topology
link HQ-Core-R1 GigabitEthernet0/0 HQ-Dist-SW1 GigabitEthernet0/1
link HQ-Dist-SW1 GigabitEthernet0/2 HQ-Access-SW1 GigabitEthernet0/1

link HQ-Core-R1 Serial0/0/0 Branch1-R1 Serial0/0/0 {
  bandwidth: 1544,
  delay: 20000
}

link HQ-Core-R1 Serial0/0/1 Branch2-R1 Serial0/0/0 {
  bandwidth: 1544,
  delay: 20000
}

# OSPF configuration function
function configure_ospf($device, $networks[]) {
  configure $device {
    "router ospf 1"
    "router-id ${device.management_ip}"
  }
  
  for $network in $networks {
    configure $device {
      "network ${network.address} ${network.wildcard} area ${ospf_area}"
    }
  }
}

# Configure OSPF on all routers
configure_ospf(HQ-Core-R1, [
  {address: "192.168.1.0", wildcard: "0.0.0.255"},
  {address: "10.0.0.0", wildcard: "0.0.0.3"},
  {address: "10.0.0.4", wildcard: "0.0.0.3"}
])

configure_ospf(Branch1-R1, [
  {address: "192.168.2.0", wildcard: "0.0.0.255"},
  {address: "10.0.0.0", wildcard: "0.0.0.3"}
])

configure_ospf(Branch2-R1, [
  {address: "192.168.3.0", wildcard: "0.0.0.255"}, 
  {address: "10.0.0.4", wildcard: "0.0.0.3"}
])

# Comprehensive testing suite
test network_validation {
  print "Starting network validation tests..."
  
  # Test 1: Device reachability
  test device_reachability {
    ping HQ-Core-R1 Branch1-R1 --count 5
    ping HQ-Core-R1 Branch2-R1 --count 5 
    ping Branch1-R1 Branch2-R1 --count 5
    
    assert all_pings_successful {
      message: "Not all devices are reachable"
    }
  }
  
  # Test 2: OSPF convergence
  test ospf_convergence {
    wait 60  # Allow convergence time
    
    assert ospf_neighbors(HQ-Core-R1).count == 2 {
      message: "HQ-Core-R1 missing OSPF neighbors"
    }
    
    assert ospf_routes(HQ-Core-R1).contains("192.168.2.0/24") {
      message: "Branch1 subnet not in HQ routing table"
    }
    
    assert ospf_routes(HQ-Core-R1).contains("192.168.3.0/24") {
      message: "Branch2 subnet not in HQ routing table"  
    }
  }
  
  # Test 3: End-to-end connectivity
  test end_to_end_connectivity {
    # Simulate hosts on each LAN segment
    simulate_host("192.168.1.10", HQ-Access-SW1)
    simulate_host("192.168.2.10", Branch1-R1)  
    simulate_host("192.168.3.10", Branch2-R1)
    
    ping "192.168.1.10" "192.168.2.10" --count 5
    ping "192.168.1.10" "192.168.3.10" --count 5
    ping "192.168.2.10" "192.168.3.10" --count 5
    
    assert all_host_connectivity {
      message: "End-to-end host connectivity failed"
    }
  }
}

# Generate deployment report
generate_report "enterprise_deployment_report.html" {
  include_topology: true,
  include_configurations: true,
  include_test_results: true,
  format: "html"
}

print "Enterprise network deployment completed successfully!"
print "Report generated: enterprise_deployment_report.html"
```

This comprehensive documentation covers the complete NetSim scripting format with all its features, syntax, and capabilities for network automation and testing.