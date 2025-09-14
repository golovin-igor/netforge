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

## Next Commands to Migrate

Following the same pattern, these commands are ready for migration:

1. **Show Version** - Device information display
2. **Show Interfaces** - Interface status and statistics
3. **Show IP Route** - Routing table display
4. **Show ARP** - ARP table display

Each will follow the same pattern: `Service` → `Command` → `Formatter` → `UnifiedHandler`.