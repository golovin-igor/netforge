# Testing Strategy

This document outlines recommended unit testing practices for the NetSim network simulation framework and proposes concrete test cases for the existing implementation. It focuses on validating protocol logic, vendor behaviour, and cross-protocol interactions.

## Guiding Principles

- **xUnit + Moq**: Use xUnit for all test projects and Moq for complex dependencies.
- **Arrange–Act–Assert**: Keep tests readable and deterministic; prefer one assert per logical scenario.
- **Naming**: `{Component}_{Scenario}_{Expectation}` to clarify intent.
- **Isolation**: Mock external dependencies such as network I/O and timers.
- **Coverage**: Target all protocol state machines, vendor customisations, and error paths.

## Core Areas

1. **CLI Handlers**
   - Command parsing and execution.
   - Privilege transitions and configuration modes.
   - Alias resolution (`sh run` → `show running-config`).
   - Error handling for unsupported commands.

2. **Protocols**
   - State-machine transitions (up/down, active/standby).
   - Timer handling and retries.
   - Packet parsing and serialisation.
   - Route or neighbour table updates.

3. **Device & Factory Logic**
   - Device creation for 15+ vendors.
   - Interface configuration and counters.
   - Vendor detection and default settings.

4. **SNMP / Management**
   - GET/SET request handling.
   - Community/credential validation.
   - Error codes for unsupported OIDs.

## Protocol Test Scenarios

| Protocol | Recommended Cases |
|----------|------------------|
| **ARP** | Cache add/remove, timeout expiry, unknown host handling. |
| **BGP** | Neighbour establishment, attribute selection (AS_PATH, MED), graceful restart, vendor-specific policy differences. |
| **OSPF** | Adjacency states, LSA flooding, cost changes triggering SPF recalculation. |
| **EIGRP** | DUAL convergence, query/reply handling, unequal-cost load balancing. |
| **RIP/IGRP** | Periodic updates, split horizon, poison reverse. |
| **IS-IS** | Area ID mismatches, DIS election, TLV parsing errors. |
| **CDP/LLDP** | Device discovery across vendor boundaries, TLV validation. |
| **STP** | Root bridge election, port state transitions, BPDU guard. |
| **HSRP/VRRP** | Active/standby role changes, pre-emption, priority tie-breaks. |
| **SNMP** | GET vs SET operations, read-only community rejection, malformed PDU handling. |
| **SSH/Telnet** | Session negotiation, authentication failures, command echo suppression. |

## Vendor Scenarios

The framework supports 15 vendors including Cisco, Juniper, Arista, Nokia, Huawei, Aruba, Fortinet, MikroTik, Dell, F5, Extreme, Broadcom, Alcatel, Anira and Linux. Test cases should cover:

- **Core CLI**: `show` and configuration commands unique to each vendor.
- **Aliases & Shortcuts**: e.g., `conf t` (Cisco) vs `edit` (Juniper).
- **Protocol Variants**: vendor-specific default timers or extensions (e.g., Cisco EIGRP named mode, Juniper OSPF areas).
- **Error Messages**: ensure authenticity and localisation per vendor.
- **Cross-Vendor Interop**: multi-vendor topologies exchanging routes or discovery packets.

## Running Tests

Run the full suite:

```bash
dotnet test
```

Filter by area:

```bash
# Protocol-only
dotnet test --filter "namespace~Protocols"

# Cisco vendor tests
dotnet test --filter "ClassName~Cisco"
```

## Future Enhancements

- Property-based testing for protocol parsers.
- Mutation testing to ensure branch coverage.
- Performance benchmarks for large topologies.

