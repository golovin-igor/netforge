# NetSim.Simulation.Core

A comprehensive C# network device simulation core that provides device implementations, factories, and the simulation engine for emulating CLI-based network devices from multiple vendors.

## Features

- **Multi-vendor support**: Cisco, Juniper, Arista, Nokia, Alcatel, Huawei, Fortinet, MikroTik, Aruba, Extreme, Dell, Broadcom, Anira, Linux, F5
- **Layer 2 protocols**: VLANs, STP/RSTP, LACP
- **Layer 3 protocols**: Static routing, OSPF, BGP, RIP, EIGRP, IS-IS, IGRP
- **Security features**: Access Control Lists (ACLs)
- **Network services**: ARP, CDP (Cisco Discovery Protocol), LLDP
- **Network simulation**: Inter-device connectivity, ping simulation, interface counters
- **Configuration management**: NVRAM configuration parsers for each vendor
- **Realistic CLI behavior**: Vendor-specific command syntax and outputs
- **Socket Terminal Server**: Remote terminal access to simulated devices via TCP/telnet
- **WebSocket Terminal Server**: Real-time terminal access via WebSocket for web integration

## Project Structure

```
NetSim.Simulation.Core/
├── Core/                           # Core interfaces and factories
│   ├── ICommandProcessor.cs        # Interface for command processing
│   ├── INetworkProtocol.cs        # Interface for network protocols
│   └── DeviceFactory.cs           # Factory for device creation
├── Configuration/                  # Configuration classes
│   └── InterfaceConfig.cs         # Interface configuration
├── Protocols/                      # Protocol implementations
│   ├── Routing/                   # Routing protocols
│   │   ├── Route.cs
│   │   ├── OspfConfig.cs
│   │   ├── OspfNeighbor.cs
│   │   ├── OspfInterface.cs
│   │   ├── BgpConfig.cs
│   │   ├── BgpNeighbor.cs
│   │   ├── RipConfig.cs
│   │   ├── RipNeighbor.cs
│   │   └── RipRoute.cs
│   ├── Switching/                 # Layer 2 protocols
│   │   ├── VlanConfig.cs
│   │   ├── StpConfig.cs
│   │   └── PortChannel.cs
│   ├── Security/                  # Security features
│   │   ├── AccessList.cs
│   │   └── AclEntry.cs
│   └── Discovery/                 # Discovery protocols
│       └── CdpNeighbor.cs
├── Devices/                       # Device implementations
│   ├── Cisco/
│   │   └── CiscoDevice.cs
│   ├── Juniper/
│   │   └── JuniperDevice.cs
│   ├── Arista/
│   │   └── AristaDevice.cs
│   ├── Nokia/
│   │   └── NokiaDevice.cs
│   ├── Huawei/
│   │   └── HuaweiDevice.cs
│   ├── Fortinet/
│   │   └── FortinetDevice.cs
│   ├── MikroTik/
│   │   └── MikroTikDevice.cs
│   ├── Aruba/
│   │   └── ArubaDevice.cs
│   ├── Extreme/
│   │   └── ExtremeDevice.cs
│   ├── Dell/
│   │   └── DellDevice.cs
│   └── Linux/
│       └── LinuxDevice.cs
├── Terminal/                      # Terminal Server implementations
│   ├── SocketTerminalServer.cs    # TCP/telnet terminal server
│   ├── WebSocketTerminalServer.cs # WebSocket terminal server
│   ├── WebSocketTerminalExtensions.cs # WebSocket service extensions
│   ├── TerminalSession.cs         # Individual terminal sessions
│   ├── TerminalEmulator.cs        # Terminal emulation logic
│   └── SocketTerminalExample.cs   # Usage example
├── CommandHandlers/               # Command processing handlers
├── Events/                        # Event system components
├── Factories/                     # Factory pattern implementations
├── Examples/                      # Example usage
│   └── Program.cs
├── NetworkDevice.cs              # Base class for all devices
├── Network.cs                    # Network topology management
└── Common/                       # Common utilities and helpers

NetSim.Simulation.CliHandlers.Tests/          # Unit tests
├── Devices/                     # Device-specific tests
├── Protocols/                   # Protocol tests
├── CommandHandlers/             # Command handler tests
├── CounterTests/               # Counter validation tests
│   ├── CiscoCounterTests.cs
│   ├── JuniperCounterTests.cs
│   ├── AristaCounterTests.cs
│   ├── NokiaCounterTests.cs
│   ├── HuaweiCounterTests.cs
│   ├── FortinetCounterTests.cs
│   ├── MikroTikCounterTests.cs
│   ├── ArubaCounterTests.cs
│   ├── ExtremeCounterTests.cs
│   ├── DellCounterTests.cs
│   └── MultiVendorCounterTests.cs
├── NetworkTests.cs
├── NetworkDeviceTests.cs
├── DeviceFactoryTests.cs
└── Configuration/
```

## Vendor Support Status

### ✅ Fully Working Vendors (15)
| Vendor | Device Types | CLI Style | Test Coverage | Special Features |
|--------|-------------|-----------|---------------|------------------|
| **Cisco** | Router, Switch, Firewall | IOS/IOS-XE | 🟢 Comprehensive | CDP, EIGRP, VLANs, ACLs |
| **Anira** | Router, Switch | Custom | 🟢 Basic | BGP, OSPF, VLAN |
| **Aruba** | Switch, Router | ArubaOS | 🟢 Basic | LLDP, VLANs, ACLs |
| **Linux** | Server, Router | Linux CLI | 🟢 Basic | IP routing, iptables |
| **Juniper** | Router, Switch | JunOS | 🟢 Basic | OSPF, BGP, Configuration modes |
| **Nokia** | Router, Switch | SR OS | 🟢 Basic | Nokia-specific routing protocols |
| **Huawei** | Router, Switch | VRP | 🟢 Basic | Huawei VRP command structure |
| **Arista** | Switch, Router | EOS | 🟢 Basic | EOS-specific features |
| **Dell** | Switch, Router | OS10 | 🟢 Basic | Dell OS10 command structure |
| **Fortinet** | Firewall, Router | FortiOS | 🟢 Basic | Security-focused commands |
| **MikroTik** | Router, Switch | RouterOS | 🟢 Basic | RouterOS command structure |
| **Extreme** | Switch, Router | EXOS | 🟢 Basic | EXOS-specific features |
| **Broadcom** | Switch | Broadcom | 🟢 Basic | Basic switch functionality |
| **Alcatel** | Router, Switch | Alcatel | 🟢 Basic | Basic routing/switching |
| **F5** | Load Balancer, ADC | BIG-IP | 🟢 Basic | LTM, GTM, ASM, APM modules |

### 🎉 All Vendors Are Working!

All 15 network vendors now compile successfully and are ready for use. The compilation errors that were previously reported have been resolved.

## Socket Terminal Server

The Socket Terminal Server provides remote terminal access to simulated network devices via TCP/telnet connections. This allows users to connect to devices using standard terminal clients like telnet, PuTTY, or terminal emulators.

### Features

- **Multi-session support**: Handle multiple concurrent terminal sessions
- **Authentication**: Optional username/password authentication
- **Device selection**: Menu-driven device selection interface
- **Vendor-specific prompts**: Accurate CLI prompts for each network vendor
- **Command history**: Per-session command history with navigation
- **Terminal emulation**: Basic terminal control sequence support
- **Session management**: Automatic session cleanup and timeout handling
- **Event notifications**: Comprehensive event system for monitoring

### Usage

#### Basic Setup

```csharp
using NetSim.Simulation.Terminal;
using NetSim.Simulation.Common;

// Create a network with devices
var network = new Network();
// ... add devices to network ...

// Configure terminal server options
var options = new SocketTerminalServerOptions
{
    ListenAddress = "127.0.0.1",
    Port = 2323,
    RequireAuthentication = true,
    Username = "admin",
    Password = "password",
    MaxConcurrentSessions = 10,
    SessionTimeoutMinutes = 30
};

// Create and start the terminal server
using var terminalServer = new SocketTerminalServer(network, options);
await terminalServer.StartAsync();

Console.WriteLine("Terminal server started on port 2323");
Console.WriteLine("Connect using: telnet 127.0.0.1 2323");

// Server runs until stopped
Console.ReadKey();
await terminalServer.StopAsync();
```

#### Advanced Configuration

```csharp
var options = new SocketTerminalServerOptions
{
    ListenAddress = "0.0.0.0",        // Listen on all interfaces
    Port = 23,                        // Standard telnet port
    RequireAuthentication = true,
    Username = "admin",
    Password = "secure_password",
    MaxConcurrentSessions = 50,
    SessionTimeoutMinutes = 60,
    TerminalType = "vt100",
    EnableTelnetProtocol = true,
    CommandHistorySize = 100,
    BufferSize = 8192
};
```

#### Event Handling

```csharp
// Register event handlers for monitoring
terminalServer.SessionStarted += (sender, e) =>
{
    Console.WriteLine($"Session started: {e.SessionId} from {e.ClientEndpoint}");
};

terminalServer.SessionEnded += (sender, e) =>
{
    Console.WriteLine($"Session ended: {e.SessionId}");
};

terminalServer.CommandExecuted += (sender, e) =>
{
    Console.WriteLine($"Command on {e.DeviceName}: {e.Command}");
};
```

### Terminal Session Flow

1. **Connection**: Client connects via telnet/TCP
2. **Authentication**: Username/password prompt (if enabled)
3. **Device Selection**: Menu showing available devices
4. **Device Connection**: Connect to selected device
5. **Command Processing**: Execute commands on device
6. **Device Disconnection**: Return to device selection menu
7. **Session End**: Client disconnects

### Supported Terminal Commands

#### Terminal-level Commands (before connecting to device)
- `connect <device_name>` - Connect to a specific device
- `list` - Show available devices
- `help` - Show help information
- `quit` / `exit` - Disconnect from terminal

#### Device-level Commands (when connected to device)
- All vendor-specific commands supported by the device
- `disconnect` / `exit` - Return to terminal menu

### Vendor-Specific Prompts

The terminal emulator provides accurate CLI prompts for each vendor:

#### Cisco IOS
```
Router1>              # User mode
Router1#              # Privileged mode
Router1(config)#      # Global configuration
Router1(config-if)#   # Interface configuration
```

#### Juniper Junos
```
user@Router2>         # Operational mode
user@Router2#         # Configuration mode
```

#### Fortinet FortiOS
```
Firewall1 #           # Command mode
Firewall1 (global) #  # Configuration mode
```

#### Linux
```
user@Server1:~$       # User mode
root@Server1:~#       # Root mode
```

### Connection Examples

#### Using telnet (Windows/Linux)
```bash
telnet 127.0.0.1 2323
```

#### Using PuTTY
1. Set Host Name: `127.0.0.1`
2. Set Port: `2323`
3. Connection type: `Telnet`
4. Click "Open"

#### Using terminal emulator
```bash
nc 127.0.0.1 2323
```

### Security Considerations

- **Authentication**: Enable authentication for production use
- **Network Access**: Bind to specific IP addresses in production
- **Session Limits**: Configure appropriate session limits
- **Timeouts**: Set reasonable session timeouts
- **Logging**: Enable logging for security monitoring

### Integration with NetSim Platform

The Socket Terminal Server integrates seamlessly with the NetSim platform:

- **Device Access**: Direct access to all simulated devices
- **Command Processing**: Full command processing through existing device implementations
- **Protocol Support**: All device protocols accessible via terminal
- **Network Topology**: Real-time access to current network state
- **Configuration Changes**: Live configuration updates through terminal

## WebSocket Terminal Server

The WebSocket Terminal Server provides real-time terminal access to simulated network devices via WebSocket connections, designed for web-based applications and modern frontend frameworks.

### Features

- **Real-time Communication**: Bidirectional WebSocket communication
- **Web Integration**: Seamless integration with React, Angular, Vue.js applications
- **Device Selection**: Interactive device selection with metadata
- **Authentication**: JWT token-based authentication
- **Session Management**: User-isolated terminal sessions
- **Connection Management**: Automatic reconnection and error handling
- **Full CLI Support**: Complete keyboard support including special keys

### Usage

#### Basic WebSocket Server Setup

```csharp
using NetSim.Simulation.Terminal;
using NetSim.Simulation.Common;

// Create a network with devices
var network = new Network();
// ... add devices to network ...

// Configure WebSocket terminal server options
var options = new WebSocketTerminalServerOptions
{
    RequireAuthentication = true,
    MaxConcurrentSessions = 100,
    SessionTimeoutMinutes = 30,
    JwtSecret = "your-jwt-secret-key"
};

// Create WebSocket terminal service
var terminalService = new WebSocketTerminalService(options);
terminalService.SetNetwork(network);

// In ASP.NET Core application
app.UseWebSockets();
app.Map("/terminal", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var deviceId = context.Request.Query["deviceId"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(deviceId))
        {
            await terminalService.HandleWebSocketAsync(webSocket, deviceId);
        }
        else
        {
            await terminalService.HandleWebSocketWithDeviceSelectionAsync(webSocket);
        }
    }
});
```

#### Client-Side Integration (JavaScript/TypeScript)

```javascript
// Connect to WebSocket terminal
const ws = new WebSocket('ws://localhost:5000/terminal?token=your-jwt-token');

ws.onopen = () => {
    console.log('Connected to terminal');
};

ws.onmessage = (event) => {
    try {
        const data = JSON.parse(event.data);
        
        if (data.type === 'devices') {
            // Handle device list
            console.log('Available devices:', data.devices);
        } else if (data.type === 'device_selected') {
            // Device connected
            console.log('Connected to device:', data.deviceName);
        }
    } catch {
        // Terminal output
        console.log('Output:', event.data);
    }
};

// Send command to device
ws.send('show ip route\r');

// Select device (if device selection is enabled)
ws.send(JSON.stringify({
    type: 'device_selection',
    deviceId: 'router1'
}));
```

#### React Hook Integration

```typescript
import { useWebSocketTerminal } from './hooks/useWebSocketTerminal';

function TerminalComponent({ deviceId }: { deviceId: string }) {
    const terminal = useWebSocketTerminal({
        deviceId,
        autoConnect: true,
        onConnect: () => console.log('Terminal connected'),
        onDisconnect: () => console.log('Terminal disconnected')
    });

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            terminal.sendInput('\r');
        } else if (e.key.length === 1) {
            terminal.sendInput(e.key);
        }
    };

    return (
        <div>
            <div className="terminal-output">
                {terminal.output}
            </div>
            <input
                onKeyDown={handleKeyDown}
                placeholder="Type commands here..."
            />
        </div>
    );
}
```

### WebSocket Message Protocol

#### Client to Server Messages

```javascript
// Device selection
{
    "type": "device_selection",
    "deviceId": "router1"
}

// Command execution (raw text)
"show ip route\r"

// Special key sequences
"\x1b[A"  // Up arrow
"\x1b[B"  // Down arrow
"\x1b[C"  // Right arrow
"\x1b[D"  // Left arrow
"\t"      // Tab completion
```

#### Server to Client Messages

```javascript
// Device list
{
    "type": "devices",
    "devices": [
        {
            "deviceId": "router1",
            "deviceName": "Router1",
            "deviceType": "router",
            "vendor": "cisco"
        }
    ]
}

// Device selection confirmation
{
    "type": "device_selected",
    "deviceId": "router1",
    "deviceName": "Router1"
}

// Error message
{
    "type": "error",
    "message": "Device not found"
}

// Terminal output (raw text)
"Router1#show ip route\r\n..."
```

### Integration with NetSim Platform

The WebSocket Terminal Server is designed for seamless integration with the NetSim platform:

- **JWT Authentication**: Secure authentication using JWT tokens
- **User Sessions**: Isolated terminal sessions per user
- **Device Management**: Access to all simulated devices in user's lab
- **Real-time Updates**: Live terminal interaction with device state changes
- **Multi-Device Support**: Concurrent connections to multiple devices

## Usage

### Creating Devices

```csharp
using NetSim.Simulation;
using NetSim.Simulation.Factories;

// Create devices using the factory pattern
var cisco = DeviceFactory.CreateDevice("cisco", "Router1");
var juniper = DeviceFactory.CreateDevice("juniper", "Router2");
var arista = DeviceFactory.CreateDevice("arista", "Switch1");
```

### Building a Network

```csharp
// Create network and add devices
var network = new Network();
network.AddDevice(cisco);
network.AddDevice(juniper);
network.AddDevice(arista);

// Create links between devices
network.AddLink("Router1", "GigabitEthernet0/0", "Router2", "ge-0/0/0");
network.AddLink("Router2", "ge-0/0/1", "Switch1", "Ethernet1");
```

### Configuring Devices

```csharp

// Cisco configuration (async)
await cisco.ProcessCommandAsync("enable");
await cisco.ProcessCommandAsync("configure terminal");
await cisco.ProcessCommandAsync("interface GigabitEthernet0/0");
await cisco.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
await cisco.ProcessCommandAsync("no shutdown");

// Juniper configuration (async)
await juniper.ProcessCommandAsync("configure");
await juniper.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/24");
await juniper.ProcessCommandAsync("commit");
```

### Testing Connectivity

```csharp
// Update network protocols
network.UpdateProtocols();

// Test ping
var result = await cisco.ProcessCommandAsync("ping 10.0.0.2");
Console.WriteLine(result);
```

## Supported Commands

### Cisco IOS
- `enable`, `configure terminal`
- `interface <name>`, `ip address <ip> <mask>`, `shutdown/no shutdown`
- `vlan <id>`, `switchport mode access`, `switchport access vlan <id>`
- `router ospf <id>`, `network <ip> <wildcard> area <area>`
- `router bgp <asn>`, `neighbor <ip> remote-as <asn>`
- `access-list <num> <permit/deny> <source>`
- `show running-config`, `show ip route`, `show interfaces`
- `ping <ip>`

### Juniper Junos
- `configure`, `commit`, `rollback`
- `set interfaces <name> unit 0 family inet address <ip>/<cidr>`
- `set vlans <name> vlan-id <id>`
- `set protocols ospf area <area> interface <name>`
- `set protocols bgp group <name> neighbor <ip> peer-as <asn>`
- `show configuration`, `show route`, `show interfaces`

### Arista EOS
- `enable`, `configure`
- `interface <name>`, `ip address <ip>/<cidr>`, `no switchport`
- `vlan <id>`, `name <name>`
- `router ospf <id>`, `network <ip>/<cidr> area <area>`
- `ip access-list standard <name>`
- `show running-config`, `show ip route`, `show vlan`

### Nokia SR OS
- `configure`, `exit`, `back`
- `port <id>`, `no shutdown`
- `router interface <name>`, `address <ip>/<cidr>`
- `router ospf area <area> interface <name>`
- `show configuration`, `show router route-table`

## Architecture

The simulator follows a modular architecture:

1. **Core Interfaces**: Define contracts for command processing and protocols
2. **Base NetworkDevice**: Abstract base class with common functionality
3. **Device Implementations**: Specific CLI behavior for each vendor
4. **Protocol Classes**: Encapsulate protocol-specific data and behavior
5. **Network Class**: Manages topology and protocol updates
6. **Factory Pattern**: Simplifies device creation
7. **Command Handlers**: Process and validate device commands
8. **Event System**: Handle device state changes and notifications
9. **Terminal Server**: Provides remote access to devices via socket connections

## Testing

The project includes comprehensive XUnit tests for each vendor implementation:

```bash
# Run all simulation tests
dotnet test NetSim.Simulation.CliHandlers.Tests/

# Run counter tests specifically
dotnet test --filter "namespace~CounterTests"

# Run vendor-specific tests
dotnet test --filter "ClassName~CiscoCounterTests"
```

## Contributing

1. Add new vendor support by creating a class in `Devices/` folder
2. Inherit from `NetworkDevice` or an existing vendor class
3. Implement vendor-specific CLI behavior
4. Add corresponding unit tests
5. Update `DeviceFactory` to include the new vendor

## License

This project is for educational and testing purposes. 