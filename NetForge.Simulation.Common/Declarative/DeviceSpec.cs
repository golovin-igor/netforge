using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Declarative specification for a network device
    /// Defines all aspects of a device in a declarative, vendor-agnostic way
    /// </summary>
    public class DeviceSpec
    {
        /// <summary>
        /// Unique device name in the network
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Device vendor and model specification
        /// </summary>
        public required VendorSpec Vendor { get; init; }

        /// <summary>
        /// Physical characteristics of the device
        /// </summary>
        public required PhysicalSpec Physical { get; init; }

        /// <summary>
        /// Physical interfaces declaration (at least 1 required)
        /// </summary>
        public required List<InterfaceSpec> Interfaces { get; init; } = [];

        /// <summary>
        /// Routing and network protocols
        /// </summary>
        public List<ProtocolSpec> Protocols { get; init; } = [];

        /// <summary>
        /// Management protocols (Telnet/SSH CLI handlers)
        /// </summary>
        public List<ManagementProtocolSpec> Management { get; init; } = [];

        /// <summary>
        /// SNMP configuration and handlers
        /// </summary>
        public SnmpSpec? Snmp { get; init; }

        /// <summary>
        /// HTTP/HTTPS management interface
        /// </summary>
        public HttpSpec? Http { get; init; }

        /// <summary>
        /// Initial NVRAM configuration
        /// </summary>
        public NvramSpec? InitialConfig { get; init; }

        /// <summary>
        /// Validate that the device specification is complete and consistent
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Device name is required");

            if (Interfaces.Count == 0)
                throw new InvalidOperationException("At least one interface must be declared");

            // Check for unique interface names
            var interfaceNames = Interfaces.Select(i => i.Name).ToList();
            if (interfaceNames.Count != interfaceNames.Distinct().Count())
                throw new InvalidOperationException("Interface names must be unique within a device");

            Vendor.Validate();
            Physical.Validate();

            foreach (var iface in Interfaces)
                iface.Validate();

            foreach (var protocol in Protocols)
                protocol.Validate();

            foreach (var mgmt in Management)
                mgmt.Validate();

            Snmp?.Validate();
            Http?.Validate();
            InitialConfig?.Validate();
        }
    }

    /// <summary>
    /// Vendor and model specification
    /// </summary>
    public class VendorSpec
    {
        public required string Vendor { get; init; }
        public required string Model { get; init; }
        public string? SoftwareVersion { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Vendor))
                throw new InvalidOperationException("Vendor is required");
            if (string.IsNullOrWhiteSpace(Model))
                throw new InvalidOperationException("Model is required");
        }
    }

    /// <summary>
    /// Physical characteristics specification
    /// </summary>
    public class PhysicalSpec
    {
        /// <summary>
        /// RAM in MB
        /// </summary>
        public required int MemoryMB { get; init; }

        /// <summary>
        /// Storage capacity in MB
        /// </summary>
        public required int StorageMB { get; init; }

        /// <summary>
        /// CPU specifications
        /// </summary>
        public CpuSpec? Cpu { get; init; }

        /// <summary>
        /// Power consumption in watts
        /// </summary>
        public int? PowerConsumptionWatts { get; init; }

        public void Validate()
        {
            if (MemoryMB <= 0)
                throw new InvalidOperationException("Memory must be positive");
            if (StorageMB <= 0)
                throw new InvalidOperationException("Storage must be positive");
        }
    }

    /// <summary>
    /// CPU specification
    /// </summary>
    public class CpuSpec
    {
        public required string Architecture { get; init; }
        public required int FrequencyMHz { get; init; }
        public int Cores { get; init; } = 1;
    }

    /// <summary>
    /// Physical interface specification
    /// </summary>
    public class InterfaceSpec
    {
        /// <summary>
        /// Interface name (unique within device)
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Interface type (e.g., Ethernet, Serial, Loopback)
        /// </summary>
        public required InterfaceType Type { get; init; }

        /// <summary>
        /// Interface speed in Mbps
        /// </summary>
        public required int SpeedMbps { get; init; }

        /// <summary>
        /// Interface description
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Whether the interface starts in shutdown state
        /// </summary>
        public bool InitiallyShutdown { get; init; } = false;

        /// <summary>
        /// VLAN configuration for switched interfaces
        /// </summary>
        public VlanInterfaceSpec? VlanConfig { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Interface name is required");
            if (SpeedMbps <= 0)
                throw new InvalidOperationException("Interface speed must be positive");
        }
    }

    /// <summary>
    /// VLAN interface configuration
    /// </summary>
    public class VlanInterfaceSpec
    {
        public SwitchportMode Mode { get; init; } = SwitchportMode.Access;
        public int? AccessVlan { get; init; }
        public List<int> AllowedVlans { get; init; } = [];
        public int? NativeVlan { get; init; }
    }

    /// <summary>
    /// Protocol specification
    /// </summary>
    public class ProtocolSpec
    {
        /// <summary>
        /// Protocol type
        /// </summary>
        public required NetworkProtocolType Protocol { get; init; }

        /// <summary>
        /// Whether the protocol should be enabled initially
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Protocol-specific configuration parameters
        /// </summary>
        public Dictionary<string, object> Configuration { get; init; } = [];

        /// <summary>
        /// Vendor-specific implementation hint
        /// </summary>
        public string? VendorImplementation { get; init; }

        public void Validate()
        {
            // Validate required configuration based on protocol type
            switch (Protocol)
            {
                case NetworkProtocolType.OSPF:
                    if (!Configuration.ContainsKey("ProcessId"))
                        throw new InvalidOperationException("OSPF requires ProcessId configuration");
                    break;
                case NetworkProtocolType.BGP:
                    if (!Configuration.ContainsKey("ASNumber"))
                        throw new InvalidOperationException("BGP requires ASNumber configuration");
                    break;
                case NetworkProtocolType.EIGRP:
                    if (!Configuration.ContainsKey("ASNumber"))
                        throw new InvalidOperationException("EIGRP requires ASNumber configuration");
                    break;
            }
        }
    }

    /// <summary>
    /// Management protocol specification (Telnet/SSH)
    /// </summary>
    public class ManagementProtocolSpec
    {
        public required ManagementProtocolType Protocol { get; init; }
        public bool Enabled { get; init; } = true;
        public int? Port { get; init; }
        public Dictionary<string, object> Configuration { get; init; } = [];

        public void Validate()
        {
            if (Port.HasValue && (Port <= 0 || Port > 65535))
                throw new InvalidOperationException("Port must be between 1 and 65535");
        }
    }

    /// <summary>
    /// SNMP specification
    /// </summary>
    public class SnmpSpec
    {
        public bool Enabled { get; init; } = true;
        public List<string> Communities { get; init; } = [];
        public SnmpVersion Version { get; init; } = SnmpVersion.V2c;
        public int Port { get; init; } = 161;

        public void Validate()
        {
            if (Port <= 0 || Port > 65535)
                throw new InvalidOperationException("SNMP port must be between 1 and 65535");
        }
    }

    /// <summary>
    /// HTTP management specification
    /// </summary>
    public class HttpSpec
    {
        public bool HttpEnabled { get; init; } = false;
        public bool HttpsEnabled { get; init; } = true;
        public int HttpPort { get; init; } = 80;
        public int HttpsPort { get; init; } = 443;

        public void Validate()
        {
            if (!HttpEnabled && !HttpsEnabled)
                throw new InvalidOperationException("At least one of HTTP or HTTPS must be enabled");
        }
    }

    /// <summary>
    /// NVRAM configuration specification
    /// </summary>
    public class NvramSpec
    {
        /// <summary>
        /// Configuration format (e.g., "ios", "junos", "eos")
        /// </summary>
        public required string Format { get; init; }

        /// <summary>
        /// Configuration content
        /// </summary>
        public required string Content { get; init; }

        /// <summary>
        /// Whether to load this as startup-config
        /// </summary>
        public bool LoadAsStartupConfig { get; init; } = true;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Format))
                throw new InvalidOperationException("Configuration format is required");
            if (string.IsNullOrWhiteSpace(Content))
                throw new InvalidOperationException("Configuration content is required");
        }
    }

    // Supporting enums
    public enum InterfaceType
    {
        Ethernet,
        FastEthernet,
        GigabitEthernet,
        TenGigabitEthernet,
        Serial,
        Loopback,
        Tunnel,
        Vlan,
        PortChannel
    }

    public enum SwitchportMode
    {
        Access,
        Trunk,
        Dynamic
    }

    public enum ManagementProtocolType
    {
        Telnet,
        SSH
    }

    public enum SnmpVersion
    {
        V1,
        V2c,
        V3
    }
}