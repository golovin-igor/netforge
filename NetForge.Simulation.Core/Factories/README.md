# NetworkTopologyFactory

## Overview

The `NetworkTopologyFactory` is a comprehensive factory class that converts `NetForge.Entities.Topology.NetworkTopology` objects into `NetForge.Simulation.Common.Network` instances ready for simulation. This factory handles the complete conversion process including devices, interfaces, and physical connections.

## Features

### Device Conversion
- **Multi-vendor Support**: Supports all major network vendors (Cisco, Juniper, Arista, Huawei, Fortinet, etc.)
- **Device Type Mapping**: Automatically maps device types (router, switch, firewall) to appropriate implementations
- **Interface Configuration**: Converts interface properties including IP addresses, MAC addresses, and status
- **NVRAM Processing**: Optionally applies stored device configurations

### Connection Management
- **Physical Connection Simulation**: Creates `PhysicalConnection` entities with realistic properties
- **Connection Type Mapping**: Maps string connection types to appropriate physical connection types
- **State Management**: Respects connection status from source topology

### Configuration Options
- **Flexible Conversion**: Extensive configuration options for different use cases
- **Protocol Initialization**: Optional protocol setup after conversion
- **Error Handling**: Comprehensive error tracking and reporting
- **Custom Mappings**: Support for custom device factories and connection types

## Usage

### Basic Usage

```csharp
using NetForge.Simulation.Factories;
using NetForge.Entities.Topology;

// Create factory
var factory = new NetworkTopologyFactory();

// Load topology (from JSON, database, etc.)
var topology = LoadTopologyFromSource();

// Convert with default options
var result = await factory.ConvertTopologyAsync(topology);

if (result.Success)
{
    var network = result.Network;
    // Use the converted network for simulation
}
else
{
    Console.WriteLine($"Conversion failed: {result.Summary}");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

### Advanced Usage with Custom Options

```csharp
using NetForge.Simulation.Factories;
using NetForge.Simulation.Devices;

var factory = new NetworkTopologyFactory();

// Create custom options
var options = new NetworkConversionOptions
{
    ApplyNvramConfiguration = true,
    EnableProtocolInitialization = true,
    EnableOspf = true,
    VerboseLogging = true,
    
    // Custom device mappings
    CustomDeviceFactories = new Dictionary<string, Func<string, NetworkDevice>>
    {
        ["customvendor:router"] = name => new CiscoDevice(name)
    },
    
    // Device name remapping
    DeviceNameMappings = new Dictionary<string, string>
    {
        ["OldDeviceName"] = "NewDeviceName"
    },
    
    // Global settings
    GlobalSystemSettings = new Dictionary<string, string>
    {
        ["domain_name"] = "example.com",
        ["ntp_server"] = "pool.ntp.org"
    }
};

var result = await factory.ConvertTopologyAsync(topology, options);
```

## Supported Device Mappings

| Vendor | Device Type | Simulation Class |
|--------|-------------|------------------|
| Cisco | router, switch, firewall | CiscoDevice |
| Juniper | router, switch, firewall | JuniperDevice |
| Arista | router, switch | AristaDevice |
| Huawei | router, switch | HuaweiDevice |
| Fortinet | firewall, router | FortinetDevice |
| Aruba | switch, router | ArubaDevice |
| MikroTik | router, switch | MikroTikDevice |
| Extreme | switch, router | ExtremeDevice |
| Dell | switch, router | DellDevice |
| Broadcom | switch, router | BroadcomDevice |
| Anira | router, switch | AniraDevice |
| Nokia | router, switch | NokiaDevice |
| Alcatel | router, switch | AlcatelDevice |
| Linux | server, router | LinuxDevice |
| F5 | firewall, loadbalancer | F5Device |

## Supported Connection Types

| Source Link Type | Physical Connection Type |
|------------------|-------------------------|
| ethernet | Ethernet |
| fiber, fibre, optical | Fiber |
| serial | Serial |
| wireless, wifi | Wireless |
| cable, copper | Ethernet |

## Configuration Options

### NetworkConversionOptions Properties

#### Basic Options
- `ApplyNvramConfiguration`: Whether to process NVRAM configuration from devices
- `ConfigureDefaultInterfaceSettings`: Apply default settings based on interface type
- `UpdateConnectedRoutes`: Update routing tables after conversion
- `ValidateConfigurations`: Perform validation during conversion
- `GenerateMissingMacAddresses`: Auto-generate MAC addresses for interfaces without them in topology

#### Protocol Options
- `EnableProtocolInitialization`: Initialize protocols after conversion
- `EnableOspf`: Enable OSPF protocol initialization
- `EnableBgp`: Enable BGP protocol initialization  
- `EnableRip`: Enable RIP protocol initialization

#### Advanced Options
- `MaxErrorsBeforeAbort`: Maximum errors before aborting conversion (-1 for unlimited)
- `VerboseLogging`: Enable detailed logging
- `PreserveOriginalIds`: Store original device IDs as system settings
- `GenerateMissingMacAddresses`: Auto-generate MAC addresses for interfaces

#### Customization Options
- `CustomDeviceFactories`: Custom device factory mappings
- `CustomConnectionTypes`: Custom connection type mappings
- `InterfaceNameMappings`: Interface name remapping
- `DeviceNameMappings`: Device name remapping
- `GlobalSystemSettings`: Global settings applied to all devices

### Predefined Option Sets

```csharp
// For basic conversion
var basicOptions = NetworkConversionOptions.CreateDefault();

// For testing scenarios
var testOptions = NetworkConversionOptions.CreateForTesting();

// For high-fidelity topology import (recommended for preserving MAC addresses)
var importOptions = NetworkConversionOptions.CreateForTopologyImport();
```

### MAC Address Import

The factory automatically imports MAC addresses from the topology data:

1. **Priority**: Uses MAC addresses specified in the topology `Interface.MacAddress` property
2. **Fallback**: Generates deterministic MAC addresses based on device and interface names if:
   - No MAC address is provided in topology AND
   - `GenerateMissingMacAddresses` option is enabled (default: true)
3. **Logging**: Enable `VerboseLogging` to see MAC address import details in device logs

```csharp
// Example topology with MAC addresses
var topology = new GeneratedTopology
{
    Devices = new List<Device>
    {
        new Device
        {
            Hostname = "Router1",
            Interfaces = new List<Interface>
            {
                new Interface
                {
                    Name = "GigabitEthernet0/0",
                    IpAddress = "192.168.1.1",
                    SubnetMask = "255.255.255.0",
                    MacAddress = "00:1E:F7:A1:B2:C3" // This will be imported
                }
            }
        }
    }
};

// Convert with MAC address import
var options = NetworkConversionOptions.CreateForTopologyImport();
var result = await factory.ConvertTopologyAsync(topology, options);
```

## Conversion Process

The factory follows a structured conversion process:

1. **Device Conversion**
   - Map vendor/type to appropriate device class
   - Create device instance with hostname
   - Convert all interfaces with properties
   - Apply NVRAM configuration if enabled
   - Set management IP and system settings

2. **Connection Creation**
   - Map connection types to physical connection types
   - Create PhysicalConnection instances
   - Apply connection status (up/down)
   - Establish physical connectivity

3. **Post-processing**
   - Initialize protocols if enabled
   - Update connected routes
   - Validate configurations
   - Generate comprehensive results

## Error Handling

The factory provides comprehensive error handling and reporting:

### NetworkConversionResult Properties
- `Success`: Overall conversion success status
- `Network`: The converted network instance
- `Errors`: List of errors encountered
- `Warnings`: List of warnings generated
- `ConvertedDevices`: Successfully converted devices
- `FailedDevices`: Devices that failed conversion
- `Statistics`: Detailed conversion statistics

### Example Error Handling

```csharp
var result = await factory.ConvertTopologyAsync(topology);

if (!result.Success)
{
    Console.WriteLine("Conversion completed with errors:");
    Console.WriteLine($"Success Rate: {result.Statistics.OverallSuccessRate:F1}%");
    
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"❌ {error}");
    }
    
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"⚠️ {warning}");
    }
}

// Generate detailed report
string detailedReport = result.GetDetailedReport();
File.WriteAllText("conversion-report.txt", detailedReport);
```

## Integration with PhysicalConnection Entity

The factory creates `PhysicalConnection` entities that provide:

- **Realistic Physical Layer Simulation**: Bandwidth, latency, packet loss modeling
- **Connection State Management**: Connected, Disconnected, Failed, Degraded states
- **Quality Metrics**: Connection quality scoring for protocol decision-making
- **Event-driven Updates**: Automatic protocol updates on connection changes

### Example: Testing Physical Connectivity

```csharp
var result = await factory.ConvertTopologyAsync(topology);

if (result.Success)
{
    var network = result.Network;
    
    // Test connection quality
    var connections = network.GetAllPhysicalConnections();
    foreach (var connection in connections)
    {
        var transmissionResult = connection.SimulateTransmission(1500);
        Console.WriteLine($"Connection {connection.Id}: " +
                         $"{(transmissionResult.Success ? "OK" : "FAILED")}");
    }
    
    // Simulate failure
    var testConnection = connections.First();
    await testConnection.SetFailedAsync("Cable failure simulation");
    
    // Protocols automatically update due to physical layer events
}
```

## Best Practices

### For Production Use
1. Use `NetworkConversionOptions.CreateForProduction()`
2. Set reasonable `MaxErrorsBeforeAbort` limit
3. Enable configuration validation
4. Monitor conversion statistics
5. Handle failed devices gracefully

### For Testing
1. Use `NetworkConversionOptions.CreateForTesting()`
2. Enable verbose logging
3. Skip NVRAM configuration for faster conversion
4. Use protocol initialization for quick setup

### For Custom Scenarios
1. Provide custom device factories for unsupported vendors
2. Use device name mappings for standardization
3. Apply global system settings for consistent configuration
4. Implement custom connection type mappings

## Performance Considerations

- **Large Topologies**: The factory handles large topologies efficiently with async processing
- **Memory Usage**: Devices are created incrementally to manage memory usage
- **Error Limits**: Use `MaxErrorsBeforeAbort` to prevent runaway error conditions
- **Logging**: Disable verbose logging in production for better performance

## Example Integration

```csharp
using NetForge.Simulation.Factories;
using NetForge.Entities.Topology;

public class TopologyService
{
    private readonly NetworkTopologyFactory _factory;
    
    public TopologyService()
    {
        _factory = new NetworkTopologyFactory();
    }
    
    public async Task<Network> LoadNetworkFromTopologyAsync(string topologyId)
    {
        // Load topology from database/file
        var topology = await LoadTopologyAsync(topologyId);
        
        // Configure conversion options
        var options = NetworkConversionOptions.CreateDefault();
        options.EnableProtocolInitialization = true;
        options.ValidateConfigurations = true;
        
        // Convert topology
        var result = await _factory.ConvertTopologyAsync(topology, options);
        
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Failed to convert topology: {result.Summary}");
        }
        
        // Log conversion statistics
        LogConversionResults(result);
        
        return result.Network;
    }
    
    private void LogConversionResults(NetworkConversionResult result)
    {
        var stats = result.Statistics;
        Console.WriteLine($"Topology conversion completed:");
        Console.WriteLine($"- Devices: {stats.DevicesConverted}/{stats.TotalDevicesProcessed}");
        Console.WriteLine($"- Connections: {stats.ConnectionsConverted}/{stats.TotalConnectionsProcessed}");
        Console.WriteLine($"- Success Rate: {stats.OverallSuccessRate:F1}%");
        Console.WriteLine($"- Duration: {stats.ConversionDuration.TotalSeconds:F1}s");
    }
}
```

This factory provides a robust, flexible foundation for converting network topologies into fully functional simulation environments that respect physical layer constraints and provide realistic network behavior. 