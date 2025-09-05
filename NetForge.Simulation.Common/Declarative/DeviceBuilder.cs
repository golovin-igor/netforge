using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Fluent builder for creating device specifications declaratively
    /// Follows the declarative device approach: vendor -> physical -> interfaces -> protocols -> handlers -> config
    /// </summary>
    public class DeviceBuilder
    {
        private string? _name;
        private VendorSpec? _vendor;
        private PhysicalSpec? _physical;
        private readonly List<InterfaceSpec> _interfaces = [];
        private readonly List<ProtocolSpec> _protocols = [];
        private readonly List<ManagementProtocolSpec> _management = [];
        private SnmpSpec? _snmp;
        private HttpSpec? _http;
        private NvramSpec? _initialConfig;

        /// <summary>
        /// Create a new device builder
        /// </summary>
        /// <param name="name">Unique device name</param>
        public static DeviceBuilder Create(string name) => new DeviceBuilder { _name = name };

        /// <summary>
        /// Step 2: Set vendor and model
        /// </summary>
        public DeviceBuilder WithVendor(string vendor, string model, string? softwareVersion = null)
        {
            _vendor = new VendorSpec
            {
                Vendor = vendor,
                Model = model,
                SoftwareVersion = softwareVersion
            };
            return this;
        }

        /// <summary>
        /// Step 3: Declare physical characteristics
        /// </summary>
        public DeviceBuilder WithPhysicalSpecs(int memoryMB, int storageMB, CpuSpec? cpu = null, int? powerWatts = null)
        {
            _physical = new PhysicalSpec
            {
                MemoryMB = memoryMB,
                StorageMB = storageMB,
                Cpu = cpu,
                PowerConsumptionWatts = powerWatts
            };
            return this;
        }

        /// <summary>
        /// Step 4: Add a physical interface (at least 1 required)
        /// </summary>
        public DeviceBuilder AddInterface(string name, InterfaceType type, int speedMbps, 
            string? description = null, bool initiallyShutdown = false, VlanInterfaceSpec? vlanConfig = null)
        {
            _interfaces.Add(new InterfaceSpec
            {
                Name = name,
                Type = type,
                SpeedMbps = speedMbps,
                Description = description,
                InitiallyShutdown = initiallyShutdown,
                VlanConfig = vlanConfig
            });
            return this;
        }

        /// <summary>
        /// Add multiple interfaces at once
        /// </summary>
        public DeviceBuilder AddInterfaces(params InterfaceSpec[] interfaces)
        {
            _interfaces.AddRange(interfaces);
            return this;
        }

        /// <summary>
        /// Step 5: Add a routing/network protocol
        /// </summary>
        public DeviceBuilder AddProtocol(NetworkProtocolType protocol, bool enabled = true, 
            Dictionary<string, object>? configuration = null, string? vendorImplementation = null)
        {
            _protocols.Add(new ProtocolSpec
            {
                Protocol = protocol,
                Enabled = enabled,
                Configuration = configuration ?? [],
                VendorImplementation = vendorImplementation
            });
            return this;
        }

        /// <summary>
        /// Add OSPF protocol with process ID
        /// </summary>
        public DeviceBuilder AddOspf(int processId, bool enabled = true, Dictionary<string, object>? additionalConfig = null)
        {
            var config = additionalConfig ?? [];
            config["ProcessId"] = processId;
            return AddProtocol(NetworkProtocolType.OSPF, enabled, config);
        }

        /// <summary>
        /// Add BGP protocol with AS number
        /// </summary>
        public DeviceBuilder AddBgp(int asNumber, bool enabled = true, Dictionary<string, object>? additionalConfig = null)
        {
            var config = additionalConfig ?? [];
            config["ASNumber"] = asNumber;
            return AddProtocol(NetworkProtocolType.BGP, enabled, config);
        }

        /// <summary>
        /// Add EIGRP protocol with AS number
        /// </summary>
        public DeviceBuilder AddEigrp(int asNumber, bool enabled = true, Dictionary<string, object>? additionalConfig = null)
        {
            var config = additionalConfig ?? [];
            config["ASNumber"] = asNumber;
            return AddProtocol(NetworkProtocolType.EIGRP, enabled, config);
        }

        /// <summary>
        /// Step 6: Add CLI management protocols (Telnet/SSH)
        /// </summary>
        public DeviceBuilder AddTelnet(bool enabled = true, int port = 23, Dictionary<string, object>? configuration = null)
        {
            _management.Add(new ManagementProtocolSpec
            {
                Protocol = ManagementProtocolType.Telnet,
                Enabled = enabled,
                Port = port,
                Configuration = configuration ?? []
            });
            return this;
        }

        /// <summary>
        /// Add SSH management
        /// </summary>
        public DeviceBuilder AddSsh(bool enabled = true, int port = 22, Dictionary<string, object>? configuration = null)
        {
            _management.Add(new ManagementProtocolSpec
            {
                Protocol = ManagementProtocolType.SSH,
                Enabled = enabled,
                Port = port,
                Configuration = configuration ?? []
            });
            return this;
        }

        /// <summary>
        /// Step 7: Add SNMP handlers
        /// </summary>
        public DeviceBuilder WithSnmp(bool enabled = true, SnmpVersion version = SnmpVersion.V2c, 
            int port = 161, params string[] communities)
        {
            _snmp = new SnmpSpec
            {
                Enabled = enabled,
                Version = version,
                Port = port,
                Communities = communities.ToList()
            };
            return this;
        }

        /// <summary>
        /// Step 8: Add HTTP handlers
        /// </summary>
        public DeviceBuilder WithHttp(bool httpEnabled = false, bool httpsEnabled = true, 
            int httpPort = 80, int httpsPort = 443)
        {
            _http = new HttpSpec
            {
                HttpEnabled = httpEnabled,
                HttpsEnabled = httpsEnabled,
                HttpPort = httpPort,
                HttpsPort = httpsPort
            };
            return this;
        }

        /// <summary>
        /// Step 9: Set initial NVRAM configuration
        /// </summary>
        public DeviceBuilder WithInitialConfig(string format, string content, bool loadAsStartup = true)
        {
            _initialConfig = new NvramSpec
            {
                Format = format,
                Content = content,
                LoadAsStartupConfig = loadAsStartup
            };
            return this;
        }

        /// <summary>
        /// Build the complete device specification
        /// </summary>
        public DeviceSpec Build()
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new InvalidOperationException("Device name is required");

            if (_vendor == null)
                throw new InvalidOperationException("Vendor specification is required. Call WithVendor() first.");

            if (_physical == null)
                throw new InvalidOperationException("Physical specification is required. Call WithPhysicalSpecs() first.");

            if (_interfaces.Count == 0)
                throw new InvalidOperationException("At least one interface is required. Call AddInterface() first.");

            var spec = new DeviceSpec
            {
                Name = _name,
                Vendor = _vendor,
                Physical = _physical,
                Interfaces = _interfaces,
                Protocols = _protocols,
                Management = _management,
                Snmp = _snmp,
                Http = _http,
                InitialConfig = _initialConfig
            };

            spec.Validate();
            return spec;
        }
    }

    /// <summary>
    /// Helper class for creating common device configurations
    /// </summary>
    public static class CommonDeviceSpecs
    {
        /// <summary>
        /// Create a standard Cisco router specification
        /// </summary>
        public static DeviceBuilder CiscoRouter(string name, string model = "ISR4451")
        {
            return DeviceBuilder.Create(name)
                .WithVendor("Cisco", model, "15.7")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192, 
                    cpu: new CpuSpec { Architecture = "ARM", FrequencyMHz = 1800, Cores = 4 })
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("GigabitEthernet0/1", InterfaceType.GigabitEthernet, 1000)
                .AddSsh()
                .WithSnmp(communities: ["public", "private"]);
        }

        /// <summary>
        /// Create a standard Cisco switch specification
        /// </summary>
        public static DeviceBuilder CiscoSwitch(string name, string model = "Catalyst9300", int portCount = 48)
        {
            var builder = DeviceBuilder.Create(name)
                .WithVendor("Cisco", model, "16.12")
                .WithPhysicalSpecs(memoryMB: 2048, storageMB: 4096)
                .AddSsh()
                .WithSnmp(communities: ["public"]);

            // Add access ports
            for (int i = 1; i <= portCount; i++)
            {
                builder.AddInterface($"GigabitEthernet1/0/{i}", InterfaceType.GigabitEthernet, 1000,
                    vlanConfig: new VlanInterfaceSpec { Mode = SwitchportMode.Access, AccessVlan = 1 });
            }

            return builder;
        }

        /// <summary>
        /// Create a standard Juniper router specification
        /// </summary>
        public static DeviceBuilder JuniperRouter(string name, string model = "MX204")
        {
            return DeviceBuilder.Create(name)
                .WithVendor("Juniper", model, "20.4R3")
                .WithPhysicalSpecs(memoryMB: 8192, storageMB: 16384)
                .AddInterface("ge-0/0/0", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("ge-0/0/1", InterfaceType.GigabitEthernet, 1000)
                .AddSsh()
                .WithSnmp(version: SnmpVersion.V3, communities: ["public"]);
        }
    }
}