# SNMP Handlers Implementation Plan

## Overview

This document outlines the comprehensive plan for implementing SNMP (Simple Network Management Protocol) handlers in the NetForge network simulation system. The SNMP handlers are designed to work in conjunction with the SNMP protocol implementation, providing vendor-specific SNMP functionality similar to how CLI handlers provide vendor-specific command line interfaces.

## Architecture Design

### Core Components

```
NetForge.Simulation.SnmpHandlers/                    # SNMP Handler Architecture
├── NetForge.Simulation.SnmpHandlers.Common/         # Core SNMP interfaces and base classes
│   ├── Interfaces/
│   │   ├── ISnmpHandler.cs                        # Core SNMP handler interface
│   │   ├── ISnmpAgent.cs                          # SNMP agent interface
│   │   ├── ISnmpMibProvider.cs                    # MIB provider interface
│   │   └── ISnmpTrapGenerator.cs                  # Trap generation interface
│   ├── Base/
│   │   ├── BaseSnmpHandler.cs                     # Base SNMP handler implementation
│   │   ├── BaseSnmpAgent.cs                       # Base SNMP agent
│   │   ├── SnmpContext.cs                         # SNMP request context
│   │   └── SnmpResult.cs                          # SNMP response result
│   ├── Models/
│   │   ├── SnmpRequest.cs                         # SNMP request model
│   │   ├── SnmpVariable.cs                        # SNMP variable binding
│   │   ├── SnmpOid.cs                             # Object identifier handling
│   │   └── SnmpTrap.cs                            # SNMP trap/notification
│   └── Services/
│       ├── SnmpHandlerDiscoveryService.cs         # Auto-discovery service
│       └── SnmpHandlerManager.cs                  # Handler management
│
├── NetForge.Simulation.SnmpHandlers.Cisco/          # Cisco-specific SNMP handlers
├── NetForge.Simulation.SnmpHandlers.Juniper/        # Juniper-specific SNMP handlers
├── NetForge.Simulation.SnmpHandlers.Arista/         # Arista-specific SNMP handlers
├── NetForge.Simulation.SnmpHandlers.Dell/           # Dell-specific SNMP handlers
├── NetForge.Simulation.SnmpHandlers.Huawei/         # Huawei-specific SNMP handlers
├── NetForge.Simulation.SnmpHandlers.Nokia/          # Nokia-specific SNMP handlers
└── NetForge.Simulation.SnmpHandlers.Generic/        # Generic/standards-based handlers
```

## Phase 1: Foundation (Weeks 1-2)

### Core SNMP Infrastructure

#### 1.1 Core Interfaces

```csharp
// NetForge.Simulation.SnmpHandlers.Common/Interfaces/ISnmpHandler.cs
namespace NetForge.Simulation.SnmpHandlers.Common
{
    public interface ISnmpHandler
    {
        /// <summary>
        /// Vendor name this handler supports
        /// </summary>
        string VendorName { get; }
        
        /// <summary>
        /// Priority for handler selection (higher = preferred)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Supported SNMP versions
        /// </summary>
        IEnumerable<SnmpVersion> SupportedVersions { get; }
        
        /// <summary>
        /// Handle SNMP GET request
        /// </summary>
        Task<SnmpResult> HandleGetRequest(SnmpContext context, SnmpRequest request);
        
        /// <summary>
        /// Handle SNMP SET request
        /// </summary>
        Task<SnmpResult> HandleSetRequest(SnmpContext context, SnmpRequest request);
        
        /// <summary>
        /// Handle SNMP GETNEXT request
        /// </summary>
        Task<SnmpResult> HandleGetNextRequest(SnmpContext context, SnmpRequest request);
        
        /// <summary>
        /// Handle SNMP GETBULK request (SNMPv2c/v3)
        /// </summary>
        Task<SnmpResult> HandleGetBulkRequest(SnmpContext context, SnmpRequest request);
        
        /// <summary>
        /// Get supported OIDs for this vendor
        /// </summary>
        IEnumerable<string> GetSupportedOids();
        
        /// <summary>
        /// Check if OID is supported
        /// </summary>
        bool SupportsOid(string oid);
        
        /// <summary>
        /// Generate vendor-specific traps
        /// </summary>
        Task<SnmpTrap> GenerateTrap(string trapOid, Dictionary<string, object> variables);
    }
}
```

```csharp
// NetForge.Simulation.SnmpHandlers.Common/Interfaces/ISnmpMibProvider.cs
namespace NetForge.Simulation.SnmpHandlers.Common
{
    public interface ISnmpMibProvider
    {
        /// <summary>
        /// Get MIB entries for vendor
        /// </summary>
        Task<Dictionary<string, SnmpMibEntry>> GetMibEntries();
        
        /// <summary>
        /// Get MIB entry by OID
        /// </summary>
        Task<SnmpMibEntry?> GetMibEntry(string oid);
        
        /// <summary>
        /// Get next OID in lexicographical order
        /// </summary>
        Task<string?> GetNextOid(string currentOid);
        
        /// <summary>
        /// Update MIB entry with current device state
        /// </summary>
        Task UpdateMibEntry(string oid, object value);
        
        /// <summary>
        /// Validate SET operation on OID
        /// </summary>
        Task<bool> ValidateSetOperation(string oid, object value);
    }
}
```

#### 1.2 Base Implementation

```csharp
// NetForge.Simulation.SnmpHandlers.Common/Base/BaseSnmpHandler.cs
namespace NetForge.Simulation.SnmpHandlers.Common
{
    public abstract class BaseSnmpHandler : ISnmpHandler
    {
        protected readonly ISnmpMibProvider _mibProvider;
        protected readonly Dictionary<string, SnmpMibEntry> _mibCache = new();
        
        public abstract string VendorName { get; }
        public virtual int Priority => 100;
        public virtual IEnumerable<SnmpVersion> SupportedVersions => new[] { SnmpVersion.V1, SnmpVersion.V2c, SnmpVersion.V3 };
        
        protected BaseSnmpHandler(ISnmpMibProvider mibProvider)
        {
            _mibProvider = mibProvider;
        }
        
        public virtual async Task<SnmpResult> HandleGetRequest(SnmpContext context, SnmpRequest request)
        {
            var variables = new List<SnmpVariable>();
            
            foreach (var oid in request.VariableBindings.Select(vb => vb.Oid))
            {
                try
                {
                    var mibEntry = await _mibProvider.GetMibEntry(oid);
                    if (mibEntry != null)
                    {
                        var value = await GetOidValue(context, oid, mibEntry);
                        variables.Add(new SnmpVariable(oid, value, mibEntry.Type));
                    }
                    else
                    {
                        // Handle vendor-specific OIDs
                        var vendorResult = await HandleVendorSpecificGet(context, oid);
                        if (vendorResult != null)
                        {
                            variables.Add(vendorResult);
                        }
                        else
                        {
                            variables.Add(new SnmpVariable(oid, null, SnmpType.NoSuchObject));
                        }
                    }
                }
                catch (Exception ex)
                {
                    variables.Add(new SnmpVariable(oid, null, SnmpType.GenError));
                    LogError($"Error processing GET for OID {oid}: {ex.Message}");
                }
            }
            
            return new SnmpResult(SnmpErrorCode.NoError, variables);
        }
        
        public virtual async Task<SnmpResult> HandleSetRequest(SnmpContext context, SnmpRequest request)
        {
            var variables = new List<SnmpVariable>();
            
            // Validate all SET operations first
            foreach (var vb in request.VariableBindings)
            {
                var isValid = await _mibProvider.ValidateSetOperation(vb.Oid, vb.Value);
                if (!isValid)
                {
                    return new SnmpResult(SnmpErrorCode.NotWritable, new List<SnmpVariable>());
                }
            }
            
            // Perform SET operations
            foreach (var vb in request.VariableBindings)
            {
                try
                {
                    var success = await HandleVendorSpecificSet(context, vb.Oid, vb.Value);
                    if (success)
                    {
                        await _mibProvider.UpdateMibEntry(vb.Oid, vb.Value);
                        variables.Add(new SnmpVariable(vb.Oid, vb.Value, vb.Type));
                    }
                    else
                    {
                        return new SnmpResult(SnmpErrorCode.GenError, new List<SnmpVariable>());
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error processing SET for OID {vb.Oid}: {ex.Message}");
                    return new SnmpResult(SnmpErrorCode.GenError, new List<SnmpVariable>());
                }
            }
            
            return new SnmpResult(SnmpErrorCode.NoError, variables);
        }
        
        public virtual async Task<SnmpResult> HandleGetNextRequest(SnmpContext context, SnmpRequest request)
        {
            var variables = new List<SnmpVariable>();
            
            foreach (var vb in request.VariableBindings)
            {
                try
                {
                    var nextOid = await _mibProvider.GetNextOid(vb.Oid);
                    if (nextOid != null)
                    {
                        var mibEntry = await _mibProvider.GetMibEntry(nextOid);
                        if (mibEntry != null)
                        {
                            var value = await GetOidValue(context, nextOid, mibEntry);
                            variables.Add(new SnmpVariable(nextOid, value, mibEntry.Type));
                        }
                    }
                    else
                    {
                        variables.Add(new SnmpVariable(vb.Oid, null, SnmpType.EndOfMibView));
                    }
                }
                catch (Exception ex)
                {
                    variables.Add(new SnmpVariable(vb.Oid, null, SnmpType.GenError));
                    LogError($"Error processing GETNEXT for OID {vb.Oid}: {ex.Message}");
                }
            }
            
            return new SnmpResult(SnmpErrorCode.NoError, variables);
        }
        
        public virtual async Task<SnmpResult> HandleGetBulkRequest(SnmpContext context, SnmpRequest request)
        {
            // Implement GETBULK operation for SNMPv2c/v3
            var variables = new List<SnmpVariable>();
            var maxRepetitions = request.MaxRepetitions;
            var nonRepeaters = request.NonRepeaters;
            
            // Process non-repeating variables first
            for (int i = 0; i < Math.Min(nonRepeaters, request.VariableBindings.Count); i++)
            {
                var result = await HandleGetNextRequest(context, 
                    new SnmpRequest { VariableBindings = new[] { request.VariableBindings[i] } });
                variables.AddRange(result.Variables);
            }
            
            // Process repeating variables
            var repeatingVars = request.VariableBindings.Skip(nonRepeaters).ToArray();
            for (int rep = 0; rep < maxRepetitions && repeatingVars.Any(); rep++)
            {
                var currentVars = new List<SnmpVariableBinding>();
                
                foreach (var vb in repeatingVars)
                {
                    var nextOid = await _mibProvider.GetNextOid(vb.Oid);
                    if (nextOid != null)
                    {
                        var mibEntry = await _mibProvider.GetMibEntry(nextOid);
                        if (mibEntry != null)
                        {
                            var value = await GetOidValue(context, nextOid, mibEntry);
                            variables.Add(new SnmpVariable(nextOid, value, mibEntry.Type));
                            currentVars.Add(new SnmpVariableBinding { Oid = nextOid });
                        }
                        else
                        {
                            variables.Add(new SnmpVariable(vb.Oid, null, SnmpType.EndOfMibView));
                        }
                    }
                    else
                    {
                        variables.Add(new SnmpVariable(vb.Oid, null, SnmpType.EndOfMibView));
                    }
                }
                
                repeatingVars = currentVars.ToArray();
            }
            
            return new SnmpResult(SnmpErrorCode.NoError, variables);
        }
        
        // Abstract methods for vendor-specific implementations
        protected abstract Task<SnmpVariable?> HandleVendorSpecificGet(SnmpContext context, string oid);
        protected abstract Task<bool> HandleVendorSpecificSet(SnmpContext context, string oid, object value);
        protected abstract Task<object> GetOidValue(SnmpContext context, string oid, SnmpMibEntry mibEntry);
        
        public abstract IEnumerable<string> GetSupportedOids();
        public abstract bool SupportsOid(string oid);
        public abstract Task<SnmpTrap> GenerateTrap(string trapOid, Dictionary<string, object> variables);
        
        protected virtual void LogError(string message)
        {
            // TODO: Implement logging
            Console.WriteLine($"[SNMP Handler Error] {message}");
        }
    }
}
```

#### 1.3 SNMP Models

```csharp
// NetForge.Simulation.SnmpHandlers.Common/Models/SnmpRequest.cs
namespace NetForge.Simulation.SnmpHandlers.Common
{
    public class SnmpRequest
    {
        public SnmpVersion Version { get; set; } = SnmpVersion.V2c;
        public string Community { get; set; } = "public";
        public SnmpPduType PduType { get; set; }
        public int RequestId { get; set; }
        public int NonRepeaters { get; set; } = 0;
        public int MaxRepetitions { get; set; } = 10;
        public ICollection<SnmpVariableBinding> VariableBindings { get; set; } = new List<SnmpVariableBinding>();
        
        // SNMPv3 specific
        public string UserName { get; set; } = "";
        public string AuthPassword { get; set; } = "";
        public string PrivPassword { get; set; } = "";
        public SnmpAuthProtocol AuthProtocol { get; set; } = SnmpAuthProtocol.None;
        public SnmpPrivProtocol PrivProtocol { get; set; } = SnmpPrivProtocol.None;
    }
    
    public class SnmpVariableBinding
    {
        public string Oid { get; set; } = "";
        public object? Value { get; set; }
        public SnmpType Type { get; set; } = SnmpType.Null;
    }
    
    public class SnmpVariable
    {
        public string Oid { get; set; }
        public object? Value { get; set; }
        public SnmpType Type { get; set; }
        
        public SnmpVariable(string oid, object? value, SnmpType type)
        {
            Oid = oid;
            Value = value;
            Type = type;
        }
    }
    
    public class SnmpMibEntry
    {
        public string Oid { get; set; } = "";
        public string Name { get; set; } = "";
        public SnmpType Type { get; set; } = SnmpType.OctetString;
        public SnmpAccess Access { get; set; } = SnmpAccess.ReadOnly;
        public string Description { get; set; } = "";
        public object? DefaultValue { get; set; }
        public Func<SnmpContext, Task<object>>? ValueProvider { get; set; }
        public Func<SnmpContext, object, Task<bool>>? ValueSetter { get; set; }
    }
}
```

```csharp
// NetForge.Simulation.SnmpHandlers.Common/Models/SnmpEnums.cs
namespace NetForge.Simulation.SnmpHandlers.Common
{
    public enum SnmpVersion
    {
        V1 = 0,
        V2c = 1,
        V3 = 3
    }
    
    public enum SnmpPduType
    {
        Get = 0xA0,
        GetNext = 0xA1,
        Response = 0xA2,
        Set = 0xA3,
        TrapV1 = 0xA4,
        GetBulk = 0xA5,
        Inform = 0xA6,
        TrapV2 = 0xA7,
        Report = 0xA8
    }
    
    public enum SnmpType
    {
        Integer = 0x02,
        OctetString = 0x04,
        Null = 0x05,
        ObjectIdentifier = 0x06,
        IpAddress = 0x40,
        Counter32 = 0x41,
        Gauge32 = 0x42,
        TimeTicks = 0x43,
        Opaque = 0x44,
        Counter64 = 0x46,
        NoSuchObject = 0x80,
        NoSuchInstance = 0x81,
        EndOfMibView = 0x82,
        GenError = 0xFF
    }
    
    public enum SnmpErrorCode
    {
        NoError = 0,
        TooBig = 1,
        NoSuchName = 2,
        BadValue = 3,
        ReadOnly = 4,
        GenError = 5,
        NoAccess = 6,
        WrongType = 7,
        WrongLength = 8,
        WrongEncoding = 9,
        WrongValue = 10,
        NoCreation = 11,
        InconsistentValue = 12,
        ResourceUnavailable = 13,
        CommitFailed = 14,
        UndoFailed = 15,
        AuthorizationError = 16,
        NotWritable = 17,
        InconsistentName = 18
    }
    
    public enum SnmpAccess
    {
        NotAccessible,
        ReadOnly,
        WriteOnly,
        ReadWrite,
        ReadCreate
    }
    
    public enum SnmpAuthProtocol
    {
        None,
        MD5,
        SHA1,
        SHA224,
        SHA256,
        SHA384,
        SHA512
    }
    
    public enum SnmpPrivProtocol
    {
        None,
        DES,
        TripleDES,
        AES128,
        AES192,
        AES256
    }
}
```

## Phase 2: Vendor-Specific Implementations (Weeks 3-6)

### 2.1 Cisco SNMP Handler

```csharp
// NetForge.Simulation.SnmpHandlers.Cisco/CiscoSnmpHandler.cs
namespace NetForge.Simulation.SnmpHandlers.Cisco
{
    public class CiscoSnmpHandler : BaseSnmpHandler
    {
        public override string VendorName => "Cisco";
        public override int Priority => 200; // Higher priority for Cisco devices
        
        // Cisco Enterprise OID: 1.3.6.1.4.1.9
        private const string CISCO_ENTERPRISE_OID = "1.3.6.1.4.1.9";
        
        private readonly Dictionary<string, string> _ciscoMibs = new()
        {
            // Cisco-specific MIBs
            ["1.3.6.1.4.1.9.2.1.1.0"] = "avgBusy1",        // CPU utilization (1 min)
            ["1.3.6.1.4.1.9.2.1.2.0"] = "avgBusy5",        // CPU utilization (5 min)
            ["1.3.6.1.4.1.9.9.48.1.1.1.2"] = "cpmCPUTotal1min", // CPU utilization
            ["1.3.6.1.4.1.9.9.48.1.1.1.5"] = "cpmCPUTotal5min", // CPU utilization
            ["1.3.6.1.4.1.9.9.109.1.1.1.1.8"] = "ciscoMemoryPoolUsed", // Memory usage
            ["1.3.6.1.4.1.9.9.109.1.1.1.1.7"] = "ciscoMemoryPoolFree", // Memory free
            ["1.3.6.1.4.1.9.9.13.1.3.1.3"] = "ciscoEnvMonTemperatureStatusValue", // Temperature
            ["1.3.6.1.4.1.9.9.13.1.4.1.3"] = "ciscoEnvMonFanState", // Fan status
            ["1.3.6.1.4.1.9.9.13.1.5.1.3"] = "ciscoEnvMonSupplyState", // Power supply
        };
        
        public CiscoSnmpHandler(ISnmpMibProvider mibProvider) : base(mibProvider)
        {
        }
        
        protected override async Task<SnmpVariable?> HandleVendorSpecificGet(SnmpContext context, string oid)
        {
            if (!oid.StartsWith(CISCO_ENTERPRISE_OID))
                return null;
            
            // Handle Cisco-specific OIDs
            return oid switch
            {
                var o when o.StartsWith("1.3.6.1.4.1.9.2.1") => await GetCiscoCpuUtilization(context, oid),
                var o when o.StartsWith("1.3.6.1.4.1.9.9.48") => await GetCiscoCpuProcessorMib(context, oid),
                var o when o.StartsWith("1.3.6.1.4.1.9.9.109") => await GetCiscoMemoryMib(context, oid),
                var o when o.StartsWith("1.3.6.1.4.1.9.9.13") => await GetCiscoEnvironmentalMib(context, oid),
                var o when o.StartsWith("1.3.6.1.4.1.9.9.46") => await GetCiscoVlanMib(context, oid),
                var o when o.StartsWith("1.3.6.1.4.1.9.9.23") => await GetCiscoCdpMib(context, oid),
                _ => await GetGenericCiscoOid(context, oid)
            };
        }
        
        protected override async Task<bool> HandleVendorSpecificSet(SnmpContext context, string oid, object value)
        {
            if (!oid.StartsWith(CISCO_ENTERPRISE_OID))
                return false;
            
            // Handle Cisco-specific SET operations
            return oid switch
            {
                var o when o.StartsWith("1.3.6.1.4.1.9.2.2") => await SetCiscoConfiguration(context, oid, value),
                var o when o.StartsWith("1.3.6.1.4.1.9.9.46") => await SetCiscoVlanConfiguration(context, oid, value),
                _ => false
            };
        }
        
        private async Task<SnmpVariable> GetCiscoCpuUtilization(SnmpContext context, string oid)
        {
            var device = context.Device;
            
            return oid switch
            {
                "1.3.6.1.4.1.9.2.1.1.0" => new SnmpVariable(oid, GetCpuUtilization(device, 1), SnmpType.Integer),
                "1.3.6.1.4.1.9.2.1.2.0" => new SnmpVariable(oid, GetCpuUtilization(device, 5), SnmpType.Integer),
                _ => new SnmpVariable(oid, null, SnmpType.NoSuchObject)
            };
        }
        
        private async Task<SnmpVariable> GetCiscoMemoryMib(SnmpContext context, string oid)
        {
            var device = context.Device;
            
            // Cisco Memory Pool MIB
            if (oid.StartsWith("1.3.6.1.4.1.9.9.109.1.1.1.1"))
            {
                var memoryInfo = GetMemoryInformation(device);
                var instanceOid = oid.Substring("1.3.6.1.4.1.9.9.109.1.1.1.1.".Length);
                var parts = instanceOid.Split('.');
                
                if (parts.Length >= 2)
                {
                    var leafNode = parts[0];
                    var poolIndex = parts[1];
                    
                    return leafNode switch
                    {
                        "7" => new SnmpVariable(oid, memoryInfo.FreeMemory, SnmpType.Gauge32),  // ciscoMemoryPoolFree
                        "8" => new SnmpVariable(oid, memoryInfo.UsedMemory, SnmpType.Gauge32),  // ciscoMemoryPoolUsed
                        "9" => new SnmpVariable(oid, memoryInfo.TotalMemory, SnmpType.Gauge32), // ciscoMemoryPoolSize
                        _ => new SnmpVariable(oid, null, SnmpType.NoSuchObject)
                    };
                }
            }
            
            return new SnmpVariable(oid, null, SnmpType.NoSuchObject);
        }
        
        private async Task<SnmpVariable> GetCiscoEnvironmentalMib(SnmpContext context, string oid)
        {
            var device = context.Device;
            
            // Cisco Environmental Monitor MIB
            return oid switch
            {
                var o when o.Contains("1.3.6.1.4.1.9.9.13.1.3.1.3") => await GetTemperatureValue(context, oid),
                var o when o.Contains("1.3.6.1.4.1.9.9.13.1.4.1.3") => await GetFanStatus(context, oid),
                var o when o.Contains("1.3.6.1.4.1.9.9.13.1.5.1.3") => await GetPowerSupplyStatus(context, oid),
                _ => new SnmpVariable(oid, null, SnmpType.NoSuchObject)
            };
        }
        
        private async Task<SnmpVariable> GetCiscoCdpMib(SnmpContext context, string oid)
        {
            var device = context.Device;
            var cdpProtocol = device.GetProtocol(ProtocolType.CDP);
            
            if (cdpProtocol == null)
                return new SnmpVariable(oid, null, SnmpType.NoSuchObject);
            
            // Cisco Discovery Protocol MIB
            return oid switch
            {
                "1.3.6.1.4.1.9.9.23.1.1.1.0" => new SnmpVariable(oid, true, SnmpType.Integer), // cdpGlobalRun
                "1.3.6.1.4.1.9.9.23.1.1.2.0" => new SnmpVariable(oid, 60, SnmpType.Integer),  // cdpGlobalMessageInterval
                "1.3.6.1.4.1.9.9.23.1.1.3.0" => new SnmpVariable(oid, 180, SnmpType.Integer), // cdpGlobalHoldTime
                _ => await GetCdpNeighborEntry(context, oid, cdpProtocol)
            };
        }
        
        public override IEnumerable<string> GetSupportedOids()
        {
            var standardOids = new[]
            {
                // Standard MIB-II OIDs that Cisco supports
                "1.3.6.1.2.1.1",    // System group
                "1.3.6.1.2.1.2",    // Interfaces group
                "1.3.6.1.2.1.3",    // Address Translation group
                "1.3.6.1.2.1.4",    // IP group
                "1.3.6.1.2.1.5",    // ICMP group
                "1.3.6.1.2.1.6",    // TCP group
                "1.3.6.1.2.1.7",    // UDP group
                "1.3.6.1.2.1.10",   // Transmission group
                "1.3.6.1.2.1.11"    // SNMP group
            };
            
            return standardOids.Concat(_ciscoMibs.Keys);
        }
        
        public override bool SupportsOid(string oid)
        {
            return oid.StartsWith("1.3.6.1.2.1") ||      // Standard MIB-II
                   oid.StartsWith(CISCO_ENTERPRISE_OID);   // Cisco Enterprise
        }
        
        public override async Task<SnmpTrap> GenerateTrap(string trapOid, Dictionary<string, object> variables)
        {
            var trap = new SnmpTrap
            {
                TrapOid = trapOid,
                EnterpriseOid = CISCO_ENTERPRISE_OID,
                Variables = variables,
                Timestamp = DateTimeOffset.UtcNow,
                SourceAddress = "127.0.0.1" // TODO: Get actual device IP
            };
            
            // Add Cisco-specific trap variables
            trap.Variables["1.3.6.1.4.1.9.0.1"] = "Cisco Systems";
            
            return trap;
        }
        
        protected override async Task<object> GetOidValue(SnmpContext context, string oid, SnmpMibEntry mibEntry)
        {
            if (mibEntry.ValueProvider != null)
            {
                return await mibEntry.ValueProvider(context);
            }
            
            return mibEntry.DefaultValue ?? "";
        }
        
        // Helper methods
        private int GetCpuUtilization(NetworkDevice device, int minutes)
        {
            // Simulate CPU utilization based on device activity
            var baseUtilization = 10; // 10% base
            var protocolCount = device.GetAllProtocols()?.Count() ?? 0;
            var interfaceCount = device.GetAllInterfaces().Count;
            
            return Math.Min(95, baseUtilization + (protocolCount * 5) + (interfaceCount * 2));
        }
        
        private (long TotalMemory, long UsedMemory, long FreeMemory) GetMemoryInformation(NetworkDevice device)
        {
            // Simulate memory information based on device complexity
            long totalMemory = 1024 * 1024 * 256; // 256MB base
            var protocolCount = device.GetAllProtocols()?.Count() ?? 0;
            var interfaceCount = device.GetAllInterfaces().Count;
            
            long usedMemory = (long)(totalMemory * 0.3) + (protocolCount * 1024 * 1024) + (interfaceCount * 512 * 1024);
            long freeMemory = totalMemory - usedMemory;
            
            return (totalMemory, usedMemory, Math.Max(0, freeMemory));
        }
        
        // Additional Cisco-specific helper methods would be implemented here
    }
}
```

### 2.2 Generic SNMP Handler

```csharp
// NetForge.Simulation.SnmpHandlers.Generic/GenericSnmpHandler.cs
namespace NetForge.Simulation.SnmpHandlers.Generic
{
    public class GenericSnmpHandler : BaseSnmpHandler
    {
        public override string VendorName => "Generic";
        public override int Priority => 50; // Lower priority, fallback handler
        
        private readonly Dictionary<string, SnmpMibEntry> _standardMibs;
        
        public GenericSnmpHandler(ISnmpMibProvider mibProvider) : base(mibProvider)
        {
            _standardMibs = InitializeStandardMibs();
        }
        
        protected override async Task<SnmpVariable?> HandleVendorSpecificGet(SnmpContext context, string oid)
        {
            // Handle standard MIB-II OIDs
            return oid switch
            {
                var o when o.StartsWith("1.3.6.1.2.1.1") => await GetSystemGroup(context, oid),
                var o when o.StartsWith("1.3.6.1.2.1.2") => await GetInterfacesGroup(context, oid),
                var o when o.StartsWith("1.3.6.1.2.1.4") => await GetIpGroup(context, oid),
                var o when o.StartsWith("1.3.6.1.2.1.6") => await GetTcpGroup(context, oid),
                var o when o.StartsWith("1.3.6.1.2.1.7") => await GetUdpGroup(context, oid),
                _ => null
            };
        }
        
        protected override async Task<bool> HandleVendorSpecificSet(SnmpContext context, string oid, object value)
        {
            // Generic SET operations for standard MIBs
            return oid switch
            {
                "1.3.6.1.2.1.1.4.0" => await SetSysContact(context, value),    // sysContact
                "1.3.6.1.2.1.1.5.0" => await SetSysName(context, value),       // sysName
                "1.3.6.1.2.1.1.6.0" => await SetSysLocation(context, value),   // sysLocation
                _ => false
            };
        }
        
        private async Task<SnmpVariable> GetSystemGroup(SnmpContext context, string oid)
        {
            var device = context.Device;
            
            return oid switch
            {
                "1.3.6.1.2.1.1.1.0" => new SnmpVariable(oid, $"{device.Vendor} {device.Model} running NetForge", SnmpType.OctetString), // sysDescr
                "1.3.6.1.2.1.1.2.0" => new SnmpVariable(oid, "1.3.6.1.4.1.99999", SnmpType.ObjectIdentifier), // sysObjectID
                "1.3.6.1.2.1.1.3.0" => new SnmpVariable(oid, GetSystemUpTime(device), SnmpType.TimeTicks), // sysUpTime
                "1.3.6.1.2.1.1.4.0" => new SnmpVariable(oid, device.Contact ?? "", SnmpType.OctetString), // sysContact
                "1.3.6.1.2.1.1.5.0" => new SnmpVariable(oid, device.Hostname, SnmpType.OctetString), // sysName
                "1.3.6.1.2.1.1.6.0" => new SnmpVariable(oid, device.Location ?? "", SnmpType.OctetString), // sysLocation
                "1.3.6.1.2.1.1.7.0" => new SnmpVariable(oid, 72, SnmpType.Integer), // sysServices
                _ => new SnmpVariable(oid, null, SnmpType.NoSuchObject)
            };
        }
        
        private async Task<SnmpVariable> GetInterfacesGroup(SnmpContext context, string oid)
        {
            var device = context.Device;
            var interfaces = device.GetAllInterfaces();
            
            if (oid == "1.3.6.1.2.1.2.1.0") // ifNumber
            {
                return new SnmpVariable(oid, interfaces.Count, SnmpType.Integer);
            }
            
            // Handle ifTable entries (1.3.6.1.2.1.2.2.1.x.y where x is column, y is interface index)
            if (oid.StartsWith("1.3.6.1.2.1.2.2.1."))
            {
                var parts = oid.Substring("1.3.6.1.2.1.2.2.1.".Length).Split('.');
                if (parts.Length >= 2 && int.TryParse(parts[0], out var column) && int.TryParse(parts[1], out var ifIndex))
                {
                    var interfaceList = interfaces.Values.ToArray();
                    if (ifIndex > 0 && ifIndex <= interfaceList.Length)
                    {
                        var interfaceConfig = interfaceList[ifIndex - 1];
                        return await GetInterfaceTableEntry(oid, column, ifIndex, interfaceConfig);
                    }
                }
            }
            
            return new SnmpVariable(oid, null, SnmpType.NoSuchObject);
        }
        
        private async Task<SnmpVariable> GetInterfaceTableEntry(string oid, int column, int ifIndex, InterfaceConfig interfaceConfig)
        {
            return column switch
            {
                1 => new SnmpVariable(oid, ifIndex, SnmpType.Integer),                    // ifIndex
                2 => new SnmpVariable(oid, interfaceConfig.Name, SnmpType.OctetString),   // ifDescr
                3 => new SnmpVariable(oid, GetInterfaceType(interfaceConfig), SnmpType.Integer), // ifType
                4 => new SnmpVariable(oid, interfaceConfig.Bandwidth, SnmpType.Gauge32), // ifSpeed
                5 => new SnmpVariable(oid, interfaceConfig.MacAddress ?? "", SnmpType.OctetString), // ifPhysAddress
                7 => new SnmpVariable(oid, interfaceConfig.IsShutdown ? 2 : 1, SnmpType.Integer), // ifAdminStatus
                8 => new SnmpVariable(oid, interfaceConfig.IsUp ? 1 : 2, SnmpType.Integer), // ifOperStatus
                10 => new SnmpVariable(oid, interfaceConfig.InOctets, SnmpType.Counter32),   // ifInOctets
                16 => new SnmpVariable(oid, interfaceConfig.OutOctets, SnmpType.Counter32),  // ifOutOctets
                _ => new SnmpVariable(oid, null, SnmpType.NoSuchObject)
            };
        }
        
        public override IEnumerable<string> GetSupportedOids()
        {
            return new[]
            {
                "1.3.6.1.2.1.1",    // System group
                "1.3.6.1.2.1.2",    // Interfaces group  
                "1.3.6.1.2.1.3",    // Address Translation group
                "1.3.6.1.2.1.4",    // IP group
                "1.3.6.1.2.1.5",    // ICMP group
                "1.3.6.1.2.1.6",    // TCP group
                "1.3.6.1.2.1.7",    // UDP group
                "1.3.6.1.2.1.10",   // Transmission group
                "1.3.6.1.2.1.11"    // SNMP group
            };
        }
        
        public override bool SupportsOid(string oid)
        {
            return oid.StartsWith("1.3.6.1.2.1"); // Standard MIB-II
        }
        
        public override async Task<SnmpTrap> GenerateTrap(string trapOid, Dictionary<string, object> variables)
        {
            return new SnmpTrap
            {
                TrapOid = trapOid,
                EnterpriseOid = "1.3.6.1.4.1.99999", // Generic enterprise OID
                Variables = variables,
                Timestamp = DateTimeOffset.UtcNow,
                SourceAddress = "127.0.0.1"
            };
        }
        
        protected override async Task<object> GetOidValue(SnmpContext context, string oid, SnmpMibEntry mibEntry)
        {
            if (mibEntry.ValueProvider != null)
            {
                return await mibEntry.ValueProvider(context);
            }
            
            return mibEntry.DefaultValue ?? "";
        }
        
        private Dictionary<string, SnmpMibEntry> InitializeStandardMibs()
        {
            // Initialize standard MIB-II entries
            return new Dictionary<string, SnmpMibEntry>
            {
                ["1.3.6.1.2.1.1.1.0"] = new SnmpMibEntry
                {
                    Oid = "1.3.6.1.2.1.1.1.0",
                    Name = "sysDescr",
                    Type = SnmpType.OctetString,
                    Access = SnmpAccess.ReadOnly,
                    Description = "A textual description of the entity"
                },
                ["1.3.6.1.2.1.1.5.0"] = new SnmpMibEntry
                {
                    Oid = "1.3.6.1.2.1.1.5.0",
                    Name = "sysName",
                    Type = SnmpType.OctetString,
                    Access = SnmpAccess.ReadWrite,
                    Description = "An administratively-assigned name for this managed node"
                }
                // Additional standard MIB entries would be defined here
            };
        }
        
        // Helper methods
        private long GetSystemUpTime(NetworkDevice device)
        {
            // Return system uptime in hundredths of seconds
            return (long)(DateTime.UtcNow - device.StartTime).TotalMilliseconds / 10;
        }
        
        private int GetInterfaceType(InterfaceConfig interfaceConfig)
        {
            // Return IANA interface type
            return interfaceConfig.Name.ToLower() switch
            {
                var n when n.StartsWith("ethernet") => 6,   // ethernetCsmacd
                var n when n.StartsWith("fastethernet") => 62, // fastEther
                var n when n.StartsWith("gigabitethernet") => 117, // gigabitEthernet
                var n when n.StartsWith("serial") => 22,    // propPointToPointSerial
                var n when n.StartsWith("loopback") => 24,  // softwareLoopback
                _ => 1 // other
            };
        }
    }
}
```

## Phase 3: Integration and Testing (Weeks 7-8)

### 3.1 SNMP Handler Manager

```csharp
// NetForge.Simulation.SnmpHandlers.Common/Services/SnmpHandlerManager.cs
namespace NetForge.Simulation.SnmpHandlers.Common
{
    public class SnmpHandlerManager
    {
        private readonly Dictionary<string, List<ISnmpHandler>> _handlersByVendor = new();
        private readonly List<ISnmpHandler> _allHandlers = new();
        private readonly SnmpHandlerDiscoveryService _discoveryService;
        
        public SnmpHandlerManager()
        {
            _discoveryService = new SnmpHandlerDiscoveryService();
            DiscoverHandlers();
        }
        
        public ISnmpHandler? GetHandler(string vendorName, string oid)
        {
            // Get vendor-specific handlers first
            if (_handlersByVendor.TryGetValue(vendorName, out var vendorHandlers))
            {
                var handler = vendorHandlers
                    .Where(h => h.SupportsOid(oid))
                    .OrderByDescending(h => h.Priority)
                    .FirstOrDefault();
                
                if (handler != null)
                    return handler;
            }
            
            // Fallback to generic handlers
            return _allHandlers
                .Where(h => h.VendorName == "Generic" && h.SupportsOid(oid))
                .OrderByDescending(h => h.Priority)
                .FirstOrDefault();
        }
        
        public async Task<SnmpResult> ProcessRequest(SnmpContext context, SnmpRequest request)
        {
            var handler = GetHandler(context.Device.Vendor, request.VariableBindings.First().Oid);
            
            if (handler == null)
            {
                return new SnmpResult(SnmpErrorCode.NoSuchName, new List<SnmpVariable>());
            }
            
            return request.PduType switch
            {
                SnmpPduType.Get => await handler.HandleGetRequest(context, request),
                SnmpPduType.Set => await handler.HandleSetRequest(context, request),
                SnmpPduType.GetNext => await handler.HandleGetNextRequest(context, request),
                SnmpPduType.GetBulk => await handler.HandleGetBulkRequest(context, request),
                _ => new SnmpResult(SnmpErrorCode.GenError, new List<SnmpVariable>())
            };
        }
        
        private void DiscoverHandlers()
        {
            var handlers = _discoveryService.DiscoverHandlers();
            
            foreach (var handler in handlers)
            {
                _allHandlers.Add(handler);
                
                if (!_handlersByVendor.ContainsKey(handler.VendorName))
                {
                    _handlersByVendor[handler.VendorName] = new List<ISnmpHandler>();
                }
                
                _handlersByVendor[handler.VendorName].Add(handler);
            }
        }
        
        public IEnumerable<ISnmpHandler> GetAllHandlers() => _allHandlers;
        public IEnumerable<string> GetSupportedVendors() => _handlersByVendor.Keys;
    }
}
```

### 3.2 Integration with SNMP Protocol

```csharp
// Integration example in NetForge.Simulation.Protocols.SNMP/SnmpProtocol.cs
namespace NetForge.Simulation.Protocols.SNMP
{
    public partial class SnmpProtocol : BaseProtocol
    {
        private SnmpHandlerManager _handlerManager;
        private SnmpAgent _snmpAgent;
        
        protected override void OnInitialized()
        {
            _handlerManager = new SnmpHandlerManager();
            
            var snmpConfig = GetSnmpConfig();
            if (snmpConfig?.IsEnabled == true)
            {
                _snmpAgent = new SnmpAgent(_handlerManager, snmpConfig);
                _snmpAgent.Start();
                
                LogProtocolEvent($"SNMP agent started on port {snmpConfig.Port}");
            }
        }
        
        protected override async Task ProcessTimers(NetworkDevice device)
        {
            // Process SNMP requests if any are queued
            if (_snmpAgent != null)
            {
                await _snmpAgent.ProcessPendingRequests();
            }
        }
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var snmpState = (SnmpState)_state;
            
            // Update MIB database with current device state
            await UpdateMibDatabase(device, snmpState);
            
            // Generate traps for state changes
            await GenerateStateChangeTraps(device, snmpState);
        }
        
        private async Task UpdateMibDatabase(NetworkDevice device, SnmpState state)
        {
            // Update standard MIB variables
            state.UpdateMibVariable("1.3.6.1.2.1.1.3.0", GetSystemUpTime()); // sysUpTime
            state.UpdateMibVariable("1.3.6.1.2.1.1.5.0", device.Hostname);    // sysName
            
            // Update interface counters
            foreach (var (interfaceName, interfaceConfig) in device.GetAllInterfaces())
            {
                var ifIndex = GetInterfaceIndex(interfaceName);
                state.UpdateMibVariable($"1.3.6.1.2.1.2.2.1.8.{ifIndex}", interfaceConfig.IsUp ? 1 : 2); // ifOperStatus
                state.UpdateMibVariable($"1.3.6.1.2.1.2.2.1.10.{ifIndex}", interfaceConfig.InOctets);     // ifInOctets
                state.UpdateMibVariable($"1.3.6.1.2.1.2.2.1.16.{ifIndex}", interfaceConfig.OutOctets);    // ifOutOctets
            }
            
            await Task.CompletedTask;
        }
    }
}
```

## Phase 4: Testing and Documentation (Weeks 9-10)

### 4.1 Unit Tests

```csharp
// NetForge.Simulation.SnmpHandlers.Tests/CiscoSnmpHandlerTests.cs
namespace NetForge.Simulation.SnmpHandlers.Tests
{
    [TestFixture]
    public class CiscoSnmpHandlerTests
    {
        private CiscoSnmpHandler _handler;
        private Mock<ISnmpMibProvider> _mockMibProvider;
        private SnmpContext _testContext;
        
        [SetUp]
        public void Setup()
        {
            _mockMibProvider = new Mock<ISnmpMibProvider>();
            _handler = new CiscoSnmpHandler(_mockMibProvider.Object);
            _testContext = CreateTestContext("Cisco");
        }
        
        [Test]
        public async Task HandleGetRequest_CiscoCpuOid_ReturnsValidValue()
        {
            // Arrange
            var request = new SnmpRequest
            {
                PduType = SnmpPduType.Get,
                VariableBindings = new[]
                {
                    new SnmpVariableBinding { Oid = "1.3.6.1.4.1.9.2.1.1.0" } // avgBusy1
                }
            };
            
            // Act
            var result = await _handler.HandleGetRequest(_testContext, request);
            
            // Assert
            Assert.That(result.ErrorCode, Is.EqualTo(SnmpErrorCode.NoError));
            Assert.That(result.Variables.Count, Is.EqualTo(1));
            Assert.That(result.Variables[0].Type, Is.EqualTo(SnmpType.Integer));
            Assert.That(result.Variables[0].Value, Is.TypeOf<int>());
        }
        
        [Test]
        public void SupportsOid_CiscoEnterpriseOid_ReturnsTrue()
        {
            // Act & Assert
            Assert.That(_handler.SupportsOid("1.3.6.1.4.1.9.2.1.1.0"), Is.True);
        }
        
        [Test]
        public void SupportsOid_NonCiscoOid_ReturnsFalse()
        {
            // Act & Assert
            Assert.That(_handler.SupportsOid("1.3.6.1.4.1.11.1.1.1.0"), Is.False); // HP OID
        }
        
        private SnmpContext CreateTestContext(string vendor)
        {
            var mockDevice = new Mock<NetworkDevice>();
            mockDevice.Setup(d => d.Vendor).Returns(vendor);
            mockDevice.Setup(d => d.Hostname).Returns("TestDevice");
            
            return new SnmpContext
            {
                Device = mockDevice.Object,
                Community = "public",
                Version = SnmpVersion.V2c
            };
        }
    }
}
```

### 4.2 Integration Tests

```csharp
// NetForge.Simulation.SnmpHandlers.Tests/SnmpIntegrationTests.cs
namespace NetForge.Simulation.SnmpHandlers.Tests
{
    [TestFixture]
    public class SnmpIntegrationTests
    {
        private SnmpHandlerManager _handlerManager;
        private NetworkDevice _ciscoDevice;
        private NetworkDevice _juniperDevice;
        
        [SetUp]
        public void Setup()
        {
            _handlerManager = new SnmpHandlerManager();
            _ciscoDevice = DeviceFactory.CreateDevice("Cisco", "2960");
            _juniperDevice = DeviceFactory.CreateDevice("Juniper", "EX4200");
        }
        
        [Test]
        public async Task ProcessRequest_CiscoDevice_UsesCiscoHandler()
        {
            // Arrange
            var context = new SnmpContext { Device = _ciscoDevice, Version = SnmpVersion.V2c };
            var request = new SnmpRequest
            {
                PduType = SnmpPduType.Get,
                VariableBindings = new[] { new SnmpVariableBinding { Oid = "1.3.6.1.4.1.9.2.1.1.0" } }
            };
            
            // Act
            var result = await _handlerManager.ProcessRequest(context, request);
            
            // Assert
            Assert.That(result.ErrorCode, Is.EqualTo(SnmpErrorCode.NoError));
            Assert.That(result.Variables.Count, Is.EqualTo(1));
        }
        
        [Test]
        public async Task ProcessRequest_StandardMib_UsesGenericHandler()
        {
            // Arrange
            var context = new SnmpContext { Device = _juniperDevice, Version = SnmpVersion.V2c };
            var request = new SnmpRequest
            {
                PduType = SnmpPduType.Get,
                VariableBindings = new[] { new SnmpVariableBinding { Oid = "1.3.6.1.2.1.1.1.0" } } // sysDescr
            };
            
            // Act
            var result = await _handlerManager.ProcessRequest(context, request);
            
            // Assert
            Assert.That(result.ErrorCode, Is.EqualTo(SnmpErrorCode.NoError));
            Assert.That(result.Variables[0].Value, Is.Not.Null);
        }
    }
}
```

## Implementation Timeline

### Summary Timeline

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| **Phase 1: Foundation** | 2 weeks | Core interfaces, base classes, SNMP models |
| **Phase 2: Vendor Handlers** | 4 weeks | Cisco, Juniper, Generic handlers |
| **Phase 3: Integration** | 2 weeks | Handler manager, protocol integration |
| **Phase 4: Testing** | 2 weeks | Unit tests, integration tests, documentation |

### Detailed Schedule

**Weeks 1-2: Foundation**
- Day 1-3: Core interfaces and base classes
- Day 4-7: SNMP models and enumerations
- Day 8-10: Handler discovery service and manager skeleton
- Day 11-14: Foundation testing and refinement

**Weeks 3-6: Vendor Implementations**
- Week 3: Cisco SNMP handler implementation
- Week 4: Generic SNMP handler for standard MIBs
- Week 5: Juniper SNMP handler implementation
- Week 6: Arista, Dell, Nokia handler implementations

**Weeks 7-8: Integration**
- Week 7: Handler manager completion and protocol integration
- Week 8: SNMP agent implementation and testing

**Weeks 9-10: Testing and Polish**
- Week 9: Comprehensive unit and integration testing
- Week 10: Documentation, performance optimization, final polish

## Success Criteria

### Functional Requirements
1. **Multi-vendor Support**: SNMP handlers for at least 5 major vendors
2. **Protocol Compliance**: Support for SNMP v1, v2c, and v3
3. **MIB Coverage**: Complete MIB-II support plus vendor-specific MIBs
4. **Integration**: Seamless integration with SNMP protocol implementation
5. **Performance**: Handle 1000+ SNMP requests per second per device

### Quality Metrics
1. **Test Coverage**: Minimum 90% code coverage
2. **Documentation**: Complete API documentation and usage examples
3. **Error Handling**: Graceful handling of all error conditions
4. **Memory Usage**: Efficient memory management with proper disposal
5. **Thread Safety**: Thread-safe implementation for concurrent access

### Integration Points
1. **SNMP Protocol**: Direct integration with SNMP protocol implementation
2. **Device Factory**: Automatic handler selection based on device vendor
3. **CLI Integration**: SNMP configuration via vendor-specific CLI commands
4. **Event System**: SNMP trap generation for network events
5. **Monitoring**: Integration with network monitoring and alerting systems

## Vendor-Specific Implementation Guide

### Cisco SNMP Handler Specifics
- **Enterprise OID**: 1.3.6.1.4.1.9
- **Key MIBs**: CISCO-PROCESS-MIB, CISCO-MEMORY-POOL-MIB, CISCO-ENVMON-MIB
- **Trap Support**: Environmental monitoring, interface state changes
- **Configuration**: Via IOS-style commands through CLI handlers

### Juniper SNMP Handler Specifics  
- **Enterprise OID**: 1.3.6.1.4.1.2636
- **Key MIBs**: JUNIPER-MIB, JUNIPER-IF-MIB, JUNIPER-CHASSIS-MIB
- **Trap Support**: Chassis alarms, interface events, RPM probes
- **Configuration**: Via Junos-style configuration through CLI handlers

### Generic Handler Coverage
- **Standard MIBs**: MIB-II, IF-MIB, IP-MIB, TCP-MIB, UDP-MIB
- **Universal Support**: Works with any vendor for standard OIDs
- **Fallback**: Used when vendor-specific handler unavailable
- **Extension Points**: Easy to extend for new standard MIBs

This comprehensive implementation plan ensures that SNMP handlers will provide robust, vendor-specific SNMP functionality that integrates seamlessly with the existing NetForge architecture while maintaining the high standards established by the CLI handler system.