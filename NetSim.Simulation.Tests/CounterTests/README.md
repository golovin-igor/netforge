# Multi-Vendor Network Device Counter Tests

This test suite provides comprehensive XUnit tests for verifying RX/TX counter increments across multiple network device vendors in the NetSim simulator environment.

## Overview

The counter tests validate packet and byte counters for various network protocols (ICMP ping, OSPF, BGP, RIP) across 8 major network device vendors:

- **Cisco** (IOS)
- **Juniper** (Junos)
- **Arista** (EOS)
- **Nokia** (SR OS)
- **Huawei** (VRP)
- **HPE/Aruba** (ArubaOS)
- **Fortinet** (FortiOS)
- **MikroTik** (RouterOS)

## Test Structure

### Vendor-Specific Test Files

Each vendor has dedicated counter tests that use vendor-specific CLI syntax:

- `CiscoCounterTests.cs` - Comprehensive tests for Cisco IOS devices
- `JuniperCounterTests.cs` - Tests for Juniper Junos devices
- `AristaCounterTests.cs` - Tests for Arista EOS devices
- `NokiaCounterTests.cs` - Tests for Nokia SR OS devices
- `HuaweiCounterTests.cs` - Tests for Huawei VRP devices
- `ArubaCounterTests.cs` - Tests for HPE/Aruba ArubaOS devices
- `FortinetCounterTests.cs` - Tests for Fortinet FortiOS devices
- `MikroTikCounterTests.cs` - Tests for MikroTik RouterOS devices

### Multi-Vendor Tests

- `MultiVendorCounterTests.cs` - Tests for mixed-vendor topologies and cross-vendor scenarios

## Counter Assumptions

The tests are built on the following packet size assumptions:

| Protocol | Packet Size | Description |
|----------|-------------|-------------|
| ICMP Ping | 64 bytes | Standard ping packet size |
| OSPF Hello | 40 bytes | OSPF hello packet |
| BGP Update | 48 bytes | BGP update message |
| RIP Advertisement | 32 bytes | RIP route advertisement |
| VLAN Tagged | +4 bytes | Additional VLAN tag overhead |

All tests simulate **5 packets** for ping operations (320 bytes total).

## Test Scenarios

### 1. Basic Ping Counter Tests
Verifies that interface counters increment correctly for ICMP ping traffic:
- TX counters on source interface
- RX counters on destination interface
- Correct packet and byte counts

### 2. Interface Status Tests
Tests counter behavior when interfaces are down:
- Shutdown interface (vendor-specific commands)
- Verify no counter increments occur
- Re-enable interface and verify counters resume

### 3. Protocol-Specific Counter Tests
Tests for routing protocol traffic:
- **OSPF Hello Counters**: Validates OSPF neighbor hello packet counting
- **BGP Update Counters**: Tests BGP update message counting
- **RIP Advertisement Counters**: Verifies RIP route advertisement counting

### 4. Traffic Filtering Tests
Tests counter behavior with ACLs/Firewall rules:
- Apply traffic blocking rules
- Verify no counter increments for blocked traffic
- Remove rules and verify counters resume

### 5. Multi-Protocol Tests
Tests cumulative counters when multiple protocols run simultaneously:
- OSPF + BGP traffic
- Verify proper counter accumulation

### 6. Multi-Vendor Topology Tests
Complex scenarios involving multiple vendor devices:
- Cross-vendor ping operations
- Square topology tests (4 vendors)
- Mixed protocol configurations

## Running the Tests

### Prerequisites
- .NET 6.0 or later
- XUnit test runner
- NetSim.Simulation project

### Command Line Execution

```bash
# Run all counter tests
dotnet test --filter "namespace~CounterTests"

# Run specific vendor tests
dotnet test --filter "ClassName~CiscoCounterTests"
dotnet test --filter "ClassName~JuniperCounterTests"

# Run multi-vendor tests only
dotnet test --filter "ClassName~MultiVendorCounterTests"

# Run with detailed output
dotnet test --filter "namespace~CounterTests" --logger "console;verbosity=detailed"
```

### Visual Studio Execution
1. Open the solution in Visual Studio
2. Navigate to Test Explorer
3. Filter by "CounterTests" namespace
4. Run desired tests or entire test suite

## Test Results Interpretation

### Successful Test Output
```
✓ Cisco_PingCounters_ShouldIncrementCorrectly
✓ Cisco_OspfHelloCounters_ShouldIncrementCorrectly
✓ MultiVendor_CrossVendorPingCounters_ShouldIncrementCorrectly
```

### Counter Validation Points

Each test validates:
1. **Packet Counts**: Exact number of packets transmitted/received
2. **Byte Counts**: Correct byte count based on packet size assumptions
3. **Interface Status**: Counters only increment when interfaces are up
4. **Protocol Activity**: Protocol-specific show commands confirm activity

### Sample Counter Verification

```csharp
// Before traffic
var txPacketsBefore = sourceInterface.TxPackets;  // e.g., 0
var rxPacketsBefore = destInterface.RxPackets;    // e.g., 0

// After 5-packet ping
var txPacketsAfter = sourceInterface.TxPackets;   // Should be 5
var rxPacketsAfter = destInterface.RxPackets;     // Should be 5
var txBytesAfter = sourceInterface.TxBytes;       // Should be +320 bytes
```

## Vendor-Specific CLI Examples

### Cisco IOS
```bash
# Interface configuration
configure terminal
interface GigabitEthernet0/0
ip address 192.168.1.1 255.255.255.0
no shutdown

# OSPF configuration
router ospf 1
network 192.168.1.0 0.0.0.255 area 0

# Counter verification
show interface GigabitEthernet0/0
```

### Juniper Junos
```bash
# Interface configuration
configure
set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24
commit

# OSPF configuration
set protocols ospf area 0.0.0.0 interface ge-0/0/0
commit

# Counter verification
show interfaces ge-0/0/0
```

### MikroTik RouterOS
```bash
# Interface configuration
/ip address add address=192.168.1.1/24 interface=ether1

# OSPF configuration
/routing ospf instance set default router-id=1.1.1.1
/routing ospf interface add interface=ether1 area=backbone

# Counter verification
/interface print stats
```

## Troubleshooting

### Common Issues

1. **Test Failures Due to Interface Names**
   - Ensure interface names match vendor conventions
   - Check device initialization in test setup

2. **Counter Increment Failures**
   - Verify interface status (IsUp property)
   - Check that simulation methods are properly incrementing counters

3. **Protocol Configuration Issues**
   - Ensure vendor-specific CLI syntax is correct
   - Verify protocol enabling commands are properly executed

### Debug Output

Enable verbose logging to see detailed test execution:
```bash
dotnet test --logger "console;verbosity=detailed" --filter "namespace~CounterTests"
```

## Extending the Tests

### Adding New Vendors
1. Create new vendor test file (e.g., `NewVendorCounterTests.cs`)
2. Implement vendor-specific CLI commands
3. Follow existing test patterns for consistency

### Adding New Protocols
1. Define protocol packet size constants
2. Implement protocol simulation methods
3. Add protocol-specific counter verification tests

### Adding New Scenarios
1. Create test methods following naming convention: `Vendor_Scenario_ShouldExpectedBehavior`
2. Include setup, action, and assertion phases
3. Document assumptions and expected results

## Test Coverage Matrix

| Vendor | Ping | OSPF | BGP | RIP | Interface Down | ACL/Firewall | Multi-Protocol |
|--------|------|------|-----|-----|----------------|--------------|-----------------|
| Cisco | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Juniper | ✓ | ✓ | ✓ | - | ✓ | ✓ | ✓ |
| Arista | ✓ | ✓ | - | - | - | - | - |
| Nokia | ✓ | ✓ | - | - | - | - | - |
| Huawei | ✓ | ✓ | - | - | ✓ | - | - |
| Aruba | ✓ | ✓ | - | - | - | - | - |
| Fortinet | ✓ | ✓ | - | - | - | - | - |
| MikroTik | ✓ | ✓ | ✓ | - | ✓ | ✓ | ✓ |

## Performance Considerations

- Tests use in-memory simulation only
- No actual network traffic generated
- Fast execution suitable for CI/CD pipelines
- Vendor-specific tests can run in parallel

## Contributing

When adding new tests:
1. Follow existing naming conventions
2. Include comprehensive documentation
3. Ensure proper test isolation
4. Add vendor-specific CLI syntax examples
5. Update this README with new coverage

## License

These tests are part of the NetSim project and are for educational and testing purposes. 