# NetForge CLI Handler Migration Guide
*Command Pattern Architecture*

## Overview

The NetForge CLI handlers have been refactored to use a **Command Pattern** architecture that separates business logic from vendor-specific formatting. This eliminates code duplication while maintaining authentic vendor CLI experiences.

## ✅ Completed: Ping Command Migration

### Architecture Components

1. **`IPingService`** - Business logic interface
2. **`PingService`** - Concrete implementation with all ping functionality
3. **`PingCommand`** - Vendor-agnostic command wrapper
4. **`IVendorCommandFormatter`** - Vendor-specific output formatting
5. **`UnifiedCliHandler`** - CLI handler that combines command + formatter

## Usage Examples

### Basic Usage with Default Formatter

```csharp
// Create services
var pingService = new PingService();

// Create unified handler with default formatting
var pingHandler = UnifiedCliHandlerFactory.CreatePingHandler(pingService);

// Register with CLI manager
manager.RegisterHandler(pingHandler);
```

### Cisco-Specific Formatting

```csharp
// Create services and Cisco formatter
var pingService = new PingService();
var ciscoFormatter = new CiscoPingFormatter();

// Create unified handler with Cisco formatting
var ciscoPingHandler = UnifiedCliHandlerFactory.CreatePingHandler(pingService, ciscoFormatter);

// Register with Cisco CLI manager
ciscoManager.RegisterHandler(ciscoPingHandler);
```

### Juniper-Specific Formatting

```csharp
// Create services and Juniper formatter
var pingService = new PingService();
var juniperFormatter = new JuniperPingFormatter();

// Create unified handler with JunOS formatting
var juniperPingHandler = UnifiedCliHandlerFactory.CreatePingHandler(pingService, juniperFormatter);

// Register with Juniper CLI manager
juniperManager.RegisterHandler(juniperPingHandler);
```

## Output Comparison

### Cisco IOS Output
```
Type escape sequence to abort.
Sending 5, 64-byte ICMP Echos to 192.168.1.1, timeout is 2 seconds:
!!!!!
Success rate is 100 percent (5/5), round-trip min/avg/max = 1/2/4 ms
```

### JunOS Output
```
PING 192.168.1.1 (192.168.1.1): 56 data bytes
64 bytes from 192.168.1.1: icmp_seq=0 ttl=64 time=0.123 ms
64 bytes from 192.168.1.1: icmp_seq=1 ttl=64 time=0.089 ms
64 bytes from 192.168.1.1: icmp_seq=2 ttl=64 time=0.156 ms
64 bytes from 192.168.1.1: icmp_seq=3 ttl=64 time=0.134 ms
64 bytes from 192.168.1.1: icmp_seq=4 ttl=64 time=0.098 ms

--- 192.168.1.1 ping statistics ---
5 packets transmitted, 5 packets received, 0% packet loss
round-trip min/avg/max/stddev = 0.089/0.120/0.156/0.025 ms
```

### Default Generic Output
```
PING 192.168.1.1 (192.168.1.1)
64 bytes from 192.168.1.1: seq=0 ttl=64 time=1ms
64 bytes from 192.168.1.1: seq=1 ttl=64 time=2ms
64 bytes from 192.168.1.1: seq=2 ttl=64 time=4ms
64 bytes from 192.168.1.1: seq=3 ttl=64 time=1ms
64 bytes from 192.168.1.1: seq=4 ttl=64 time=3ms

--- 192.168.1.1 ping statistics ---
5 packets transmitted, 5 packets received, 0.0% packet loss
round-trip min/avg/max/stddev = 1/2/4/1.140 ms
```

## Migration Steps for Existing Vendors

### Step 1: Replace Existing Ping Handler

**OLD CODE** (per vendor project):
```csharp
// NetForge.Simulation.CliHandlers.Cisco/Basic/BasicHandlers.cs
public class PingCommandHandler : VendorAgnosticCliHandler
{
    // 100+ lines of duplicated business logic
    // Mixed with Cisco-specific formatting
}
```

**NEW CODE** (in vendor project):
```csharp
// NetForge.Simulation.CliHandlers.Cisco/CiscoHandlerRegistry.cs
public void RegisterHandlers(ICliHandlerManager manager)
{
    var pingService = serviceProvider.GetService<IPingService>() ?? new PingService();
    var ciscoFormatter = new CiscoPingFormatter();
    var ciscoPingHandler = UnifiedCliHandlerFactory.CreatePingHandler(pingService, ciscoFormatter);

    manager.RegisterHandler(ciscoPingHandler);
    // ... other handlers
}
```

### Step 2: Remove Old Handler Files

- Delete vendor-specific ping handler implementations
- Keep only the formatter classes
- Business logic now comes from shared `PingService`

### Step 3: Test Migration

```csharp
[Test]
public void CiscoPingHandler_ShouldFormatCorrectly()
{
    // Arrange
    var pingService = new PingService();
    var ciscoFormatter = new CiscoPingFormatter();
    var handler = UnifiedCliHandlerFactory.CreatePingHandler(pingService, ciscoFormatter);

    // Act
    var result = handler.Handle(CreatePingContext("ping 192.168.1.1"));

    // Assert
    result.Output.Should().StartWith("Type escape sequence to abort.");
    result.Output.Should().Contain("Success rate is 100 percent");
}
```

## Benefits of Migration

### ✅ Code Reduction
- **Before**: 100+ lines per vendor ping handler
- **After**: ~30 lines per vendor formatter
- **Savings**: 70%+ code reduction

### ✅ Maintainability
- Business logic changes in one place (`PingService`)
- Vendor formatting isolated in separate classes
- Easy to add new vendors (just create formatter)

### ✅ Testing
- Business logic fully unit testable
- Formatting logic separately testable
- Integration testing with real CLI contexts

### ✅ Consistency
- Same ping behavior across all vendors
- Only output format differs
- Reliable network simulation

## Creating New Vendor Formatters

```csharp
public class MyVendorPingFormatter : BaseCommandFormatter
{
    public override string VendorName => "MyVendor";

    protected override string FormatPingResult(PingCommandData pingData)
    {
        var result = pingData.PingResult;
        var output = new StringBuilder();

        // Implement your vendor's specific ping output format
        output.AppendLine($"MyVendor Ping to {result.Destination}");

        foreach (var reply in result.Replies)
        {
            if (reply.Success)
                output.AppendLine($"Reply from {reply.FromAddress}: time={reply.RoundTripTime}ms TTL={reply.Ttl}");
            else
                output.AppendLine($"Request timed out.");
        }

        output.AppendLine($"Statistics: {result.PacketsReceived}/{result.PacketsSent} success ({result.PacketLossPercentage:F1}% loss)");

        return output.ToString();
    }
}
```

## ✅ Completed: Show Version Command Migration

### Architecture Components

1. **`IDeviceInformationService`** - Device information business logic interface
2. **`DeviceInformationService`** - Concrete implementation with comprehensive device info
3. **`ShowVersionCommand`** - Vendor-agnostic command wrapper
4. **`CiscoShowVersionFormatter`** - Cisco IOS-style show version output
5. **`JuniperShowVersionFormatter`** - JunOS-style show version output

### Usage Examples

#### Cisco Show Version
```csharp
var deviceInfoService = new DeviceInformationService();
var ciscoFormatter = new CiscoShowVersionFormatter();
var ciscoShowVersionHandler = ShowVersionHandlerFactory.CreateShowVersionHandler(deviceInfoService, ciscoFormatter);

manager.RegisterHandler(ciscoShowVersionHandler);
```

#### Juniper Show Version
```csharp
var deviceInfoService = new DeviceInformationService();
var juniperFormatter = new JuniperShowVersionFormatter();
var juniperShowVersionHandler = ShowVersionHandlerFactory.CreateShowVersionHandler(deviceInfoService, juniperFormatter);

manager.RegisterHandler(juniperShowVersionHandler);
```

### Output Comparison

#### Cisco IOS Show Version
```
Cisco IOS Software, Catalyst 2960-24TT-L Software (C2960-LANBASEK9-M), Version 15.2(4)E7, RELEASE SOFTWARE (fc2)
Technical Support: http://www.cisco.com/techsupport
Copyright (c) 1986-2016 by Cisco Systems, Inc.
Compiled Mar 21 2023 by prod_rel_team

ROM: Bootstrap program is Catalyst 2960-24TT-L boot loader
BOOTLDR: Catalyst 2960-24TT-L Boot Loader (C2960-HBOOT-M) Version 12.2(44)SE5, RELEASE SOFTWARE (fc1)

Router1 uptime is 15 days, 8 hours, 23 minutes
System returned to ROM by power-on
System restarted at 14:35:22 UTC Mon Jan 30 2023
System image file is "bootflash:c2960-lanbasek9-mz.152-4.E7.bin"

cisco WS-C2960-24TT-L (PowerPC405) processor (revision A0) with 64K/32K bytes of memory.
Processor board ID FCW12345X678
Last reset from power-on
24 FastEthernet interfaces
The password-recovery mechanism is enabled.

Configuration register is 0x2102
```

#### JunOS Show Version
```
Hostname: Router1
Model: EX2200-24T-4G
Junos: 12.3R12-S13
JUNOS Software Release [12.3R12-S13] (Export edition)

{master:0}
fpc0:
  CPU utilization          :    2 percent (5 sec avg)
  CPU utilization          :    1 percent (1 min avg)
  CPU utilization          :    1 percent (5 min avg)
  Memory utilization       :   25 percent
  Total CPU DRAM installed :    1024 MB
  Memory utilization       :   25 percent
  Total memory             :    1024 MB Max
  Reserved memory          :    128 MB
  Available memory         :    768 MB

15 days, 08:23
```

## Next Commands Ready for Migration

Following the same pattern, these commands are ready for migration:

1. **Show Interfaces** - Interface status and statistics
2. **Show IP Route** - Routing table display
3. **Show ARP** - ARP table display
4. **Show MAC Address Table** - MAC learning table display

Each follows the same pattern: `Service` → `Command` → `Formatter` → `UnifiedHandler`.