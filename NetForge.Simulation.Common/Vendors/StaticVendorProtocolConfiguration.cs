using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Protocol;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Static, declarative vendor protocol configuration
    /// No reflection or dynamic casts - all compile-time type safe
    /// </summary>
    public static class StaticVendorProtocolConfiguration
    {
        /// <summary>
        /// Protocol configuration for a specific vendor
        /// </summary>
        public readonly struct VendorProtocolConfig
        {
            public string VendorName { get; }
            public NetworkProtocolType[] SupportedProtocols { get; }
            public NetworkProtocolType[] DefaultProtocols { get; }
            public Dictionary<NetworkProtocolType, int> ProtocolPriorities { get; }

            public VendorProtocolConfig(
                string vendorName,
                NetworkProtocolType[] supportedProtocols,
                NetworkProtocolType[] defaultProtocols = null,
                Dictionary<NetworkProtocolType, int> protocolPriorities = null)
            {
                VendorName = vendorName;
                SupportedProtocols = supportedProtocols;
                DefaultProtocols = defaultProtocols ?? Array.Empty<NetworkProtocolType>();
                ProtocolPriorities = protocolPriorities ?? new Dictionary<NetworkProtocolType, int>();
            }

            public bool SupportsProtocol(NetworkProtocolType type)
            {
                return Array.IndexOf(SupportedProtocols, type) >= 0;
            }

            public bool IsDefaultProtocol(NetworkProtocolType type)
            {
                return Array.IndexOf(DefaultProtocols, type) >= 0;
            }

            public int GetProtocolPriority(NetworkProtocolType type)
            {
                return ProtocolPriorities.TryGetValue(type, out var priority) ? priority : 100;
            }
        }

        // Static vendor configurations - no reflection needed
        private static readonly Dictionary<string, VendorProtocolConfig> _vendorConfigs = InitializeVendorConfigs();

        private static Dictionary<string, VendorProtocolConfig> InitializeVendorConfigs()
        {
            var configs = new Dictionary<string, VendorProtocolConfig>(StringComparer.OrdinalIgnoreCase);

            // Cisco configuration
            configs["Cisco"] = new VendorProtocolConfig(
                "Cisco",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.STP,
                    NetworkProtocolType.CDP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.EIGRP,
                    NetworkProtocolType.RIP,
                    NetworkProtocolType.ISIS,
                    NetworkProtocolType.VRRP,
                    NetworkProtocolType.HSRP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet,
                    NetworkProtocolType.HTTP
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.CDP,
                    NetworkProtocolType.STP
                },
                protocolPriorities: new Dictionary<NetworkProtocolType, int>
                {
                    [NetworkProtocolType.ARP] = 10,
                    [NetworkProtocolType.STP] = 20,
                    [NetworkProtocolType.CDP] = 30,
                    [NetworkProtocolType.EIGRP] = 90,
                    [NetworkProtocolType.OSPF] = 100,
                    [NetworkProtocolType.BGP] = 110
                }
            );

            // Juniper configuration
            configs["Juniper"] = new VendorProtocolConfig(
                "Juniper",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.STP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.RIP,
                    NetworkProtocolType.ISIS,
                    NetworkProtocolType.VRRP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet,
                    NetworkProtocolType.HTTP
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.STP
                },
                protocolPriorities: new Dictionary<NetworkProtocolType, int>
                {
                    [NetworkProtocolType.ARP] = 10,
                    [NetworkProtocolType.STP] = 20,
                    [NetworkProtocolType.LLDP] = 30,
                    [NetworkProtocolType.OSPF] = 100,
                    [NetworkProtocolType.BGP] = 110,
                    [NetworkProtocolType.ISIS] = 115
                }
            );

            // Arista configuration
            configs["Arista"] = new VendorProtocolConfig(
                "Arista",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.STP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.ISIS,
                    NetworkProtocolType.VRRP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet,
                    NetworkProtocolType.HTTP
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.STP
                }
            );

            // Dell configuration
            configs["Dell"] = new VendorProtocolConfig(
                "Dell",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.STP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.VRRP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.STP
                }
            );

            // Huawei configuration
            configs["Huawei"] = new VendorProtocolConfig(
                "Huawei",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.STP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.VRRP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.STP
                }
            );

            // Nokia configuration
            configs["Nokia"] = new VendorProtocolConfig(
                "Nokia",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.ISIS,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP
                }
            );

            // F5 configuration
            configs["F5"] = new VendorProtocolConfig(
                "F5",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.HTTP
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.HTTP
                }
            );

            // Linux configuration
            configs["Linux"] = new VendorProtocolConfig(
                "Linux",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.SSH
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.SSH
                }
            );

            // Generic/Unknown vendor configuration
            configs["Generic"] = new VendorProtocolConfig(
                "Generic",
                supportedProtocols: new[]
                {
                    NetworkProtocolType.ARP,
                    NetworkProtocolType.STP,
                    NetworkProtocolType.LLDP,
                    NetworkProtocolType.OSPF,
                    NetworkProtocolType.BGP,
                    NetworkProtocolType.RIP,
                    NetworkProtocolType.VRRP,
                    NetworkProtocolType.SNMP,
                    NetworkProtocolType.SSH,
                    NetworkProtocolType.Telnet
                },
                defaultProtocols: new[]
                {
                    NetworkProtocolType.ARP
                }
            );

            return configs;
        }

        /// <summary>
        /// Get vendor configuration
        /// </summary>
        public static VendorProtocolConfig GetVendorConfig(string vendor)
        {
            if (_vendorConfigs.TryGetValue(vendor, out var config))
            {
                return config;
            }

            // Return generic config if vendor not found
            return _vendorConfigs["Generic"];
        }

        /// <summary>
        /// Check if a vendor supports a specific protocol
        /// </summary>
        public static bool VendorSupportsProtocol(string vendor, NetworkProtocolType protocol)
        {
            var config = GetVendorConfig(vendor);
            return config.SupportsProtocol(protocol);
        }

        /// <summary>
        /// Get all supported protocols for a vendor
        /// </summary>
        public static NetworkProtocolType[] GetVendorProtocols(string vendor)
        {
            var config = GetVendorConfig(vendor);
            return config.SupportedProtocols;
        }

        /// <summary>
        /// Get default protocols that should be auto-enabled for a vendor
        /// </summary>
        public static NetworkProtocolType[] GetVendorDefaultProtocols(string vendor)
        {
            var config = GetVendorConfig(vendor);
            return config.DefaultProtocols;
        }

        /// <summary>
        /// Register protocols on a device based on static vendor configuration
        /// </summary>
        public static void RegisterProtocolsForDevice(INetworkDevice device, Func<NetworkProtocolType, IDeviceProtocol> protocolFactory)
        {
            if (device == null || protocolFactory == null)
                return;

            var vendor = device.Vendor ?? "Generic";
            var config = GetVendorConfig(vendor);

            // Create a sorted list of protocols by priority
            var protocolsWithPriority = new List<(NetworkProtocolType type, int priority)>();

            foreach (var protocolType in config.SupportedProtocols)
            {
                var priority = config.GetProtocolPriority(protocolType);
                protocolsWithPriority.Add((protocolType, priority));
            }

            // Sort by priority (lower number = higher priority)
            protocolsWithPriority.Sort((a, b) => a.priority.CompareTo(b.priority));

            // Register protocols in priority order
            foreach (var (protocolType, _) in protocolsWithPriority)
            {
                var protocol = protocolFactory(protocolType);
                if (protocol != null)
                {
                    device.AddProtocol(protocol);

                    // Auto-enable default protocols
                    if (config.IsDefaultProtocol(protocolType))
                    {
                        // Protocol should be enabled by default
                        // This would be handled by the protocol's initialization
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for static vendor protocol configuration
    /// </summary>
    public static class VendorProtocolExtensions
    {
        /// <summary>
        /// Register all vendor-supported protocols using static configuration
        /// </summary>
        public static void RegisterVendorProtocolsStatic(this INetworkDevice device, Func<NetworkProtocolType, IDeviceProtocol> protocolFactory)
        {
            StaticVendorProtocolConfiguration.RegisterProtocolsForDevice(device, protocolFactory);
        }

        /// <summary>
        /// Check if this device's vendor supports a specific protocol
        /// </summary>
        public static bool VendorSupportsProtocol(this INetworkDevice device, NetworkProtocolType protocol)
        {
            return StaticVendorProtocolConfiguration.VendorSupportsProtocol(device.Vendor ?? "Generic", protocol);
        }

        /// <summary>
        /// Get all protocols supported by this device's vendor
        /// </summary>
        public static NetworkProtocolType[] GetVendorSupportedProtocols(this INetworkDevice device)
        {
            return StaticVendorProtocolConfiguration.GetVendorProtocols(device.Vendor ?? "Generic");
        }
    }
}