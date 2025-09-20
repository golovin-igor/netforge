using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Protocol;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Static, declarative protocol registry that avoids reflection and dynamic casts
    /// Provides compile-time type safety for protocol registration
    /// </summary>
    public static class ProtocolRegistry
    {
        /// <summary>
        /// Protocol factory delegate - creates a protocol instance without reflection
        /// </summary>
        public delegate IDeviceProtocol ProtocolFactory();

        /// <summary>
        /// Protocol registration entry with static type information
        /// </summary>
        public readonly struct ProtocolRegistration
        {
            public NetworkProtocolType Type { get; }
            public string Name { get; }
            public ProtocolFactory Factory { get; }
            public string[] SupportedVendors { get; }
            public int Priority { get; }
            public bool IsManagementProtocol { get; }

            public ProtocolRegistration(
                NetworkProtocolType type,
                string name,
                ProtocolFactory factory,
                string[] supportedVendors,
                int priority = 100,
                bool isManagementProtocol = false)
            {
                Type = type;
                Name = name;
                Factory = factory;
                SupportedVendors = supportedVendors;
                Priority = priority;
                IsManagementProtocol = isManagementProtocol;
            }

            public bool SupportsVendor(string vendor)
            {
                if (SupportedVendors == null || SupportedVendors.Length == 0)
                    return false;

                foreach (var supportedVendor in SupportedVendors)
                {
                    if (string.Equals(supportedVendor, vendor, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(supportedVendor, "Generic", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Static protocol registrations - no reflection needed
        private static readonly Dictionary<NetworkProtocolType, ProtocolRegistration> _registrations = new();
        private static readonly List<ProtocolRegistration> _orderedRegistrations = new();
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initialize all protocol registrations statically
        /// This is called once at application startup
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (_isInitialized)
                    return;

                // Register all protocols declaratively - no reflection
                // The actual protocol types would need to be referenced here
                // This is a template showing the pattern

                // Layer 2 Protocols
                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.ARP,
                    "Address Resolution Protocol",
                    () => CreateArpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Generic" },
                    priority: 10
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.STP,
                    "Spanning Tree Protocol",
                    () => CreateStpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Generic" },
                    priority: 20
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.CDP,
                    "Cisco Discovery Protocol",
                    () => CreateCdpProtocol(),
                    new[] { "Cisco" },
                    priority: 30
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.LLDP,
                    "Link Layer Discovery Protocol",
                    () => CreateLldpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Generic" },
                    priority: 30
                ));

                // Layer 3 Routing Protocols
                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.OSPF,
                    "Open Shortest Path First",
                    () => CreateOspfProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" },
                    priority: 100
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.BGP,
                    "Border Gateway Protocol",
                    () => CreateBgpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" },
                    priority: 110
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.EIGRP,
                    "Enhanced Interior Gateway Routing Protocol",
                    () => CreateEigrpProtocol(),
                    new[] { "Cisco" },
                    priority: 90
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.RIP,
                    "Routing Information Protocol",
                    () => CreateRipProtocol(),
                    new[] { "Cisco", "Juniper", "Generic" },
                    priority: 120
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.ISIS,
                    "Intermediate System to Intermediate System",
                    () => CreateIsisProtocol(),
                    new[] { "Cisco", "Juniper", "Nokia", "Generic" },
                    priority: 115
                ));

                // High Availability Protocols
                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.VRRP,
                    "Virtual Router Redundancy Protocol",
                    () => CreateVrrpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Generic" },
                    priority: 200
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.HSRP,
                    "Hot Standby Router Protocol",
                    () => CreateHsrpProtocol(),
                    new[] { "Cisco" },
                    priority: 200
                ));

                // Management Protocols
                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.SNMP,
                    "Simple Network Management Protocol",
                    () => CreateSnmpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Generic" },
                    priority: 300,
                    isManagementProtocol: true
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.SSH,
                    "Secure Shell",
                    () => CreateSshProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Linux", "Generic" },
                    priority: 310,
                    isManagementProtocol: true
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.Telnet,
                    "Telnet",
                    () => CreateTelnetProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Generic" },
                    priority: 320,
                    isManagementProtocol: true
                ));

                RegisterProtocol(new ProtocolRegistration(
                    NetworkProtocolType.HTTP,
                    "Hypertext Transfer Protocol",
                    () => CreateHttpProtocol(),
                    new[] { "Cisco", "Juniper", "Arista", "F5", "Generic" },
                    priority: 330,
                    isManagementProtocol: true
                ));

                // Sort registrations by priority for ordered initialization
                _orderedRegistrations.Sort((a, b) => a.Priority.CompareTo(b.Priority));

                _isInitialized = true;
            }
        }

        private static void RegisterProtocol(ProtocolRegistration registration)
        {
            _registrations[registration.Type] = registration;
            _orderedRegistrations.Add(registration);
        }

        /// <summary>
        /// Get all protocol registrations for a specific vendor
        /// </summary>
        public static IEnumerable<ProtocolRegistration> GetVendorProtocols(string vendor)
        {
            if (!_isInitialized)
                Initialize();

            foreach (var registration in _orderedRegistrations)
            {
                if (registration.SupportsVendor(vendor))
                {
                    yield return registration;
                }
            }
        }

        /// <summary>
        /// Get a specific protocol registration
        /// </summary>
        public static bool TryGetProtocol(NetworkProtocolType type, out ProtocolRegistration registration)
        {
            if (!_isInitialized)
                Initialize();

            return _registrations.TryGetValue(type, out registration);
        }

        /// <summary>
        /// Create a protocol instance statically without reflection
        /// </summary>
        public static IDeviceProtocol? CreateProtocol(NetworkProtocolType type)
        {
            if (TryGetProtocol(type, out var registration))
            {
                return registration.Factory();
            }
            return null;
        }

        /// <summary>
        /// Register protocols on a device based on vendor
        /// </summary>
        public static void RegisterDeviceProtocols(INetworkDevice device)
        {
            if (device == null)
                return;

            var vendor = device.Vendor;
            var protocols = GetVendorProtocols(vendor);

            foreach (var registration in protocols)
            {
                var protocol = registration.Factory();
                if (protocol != null)
                {
                    device.AddProtocol(protocol);
                }
            }
        }

        // Static factory methods - these would be implemented to create actual protocol instances
        // These avoid reflection by directly instantiating the concrete types

        private static IDeviceProtocol CreateArpProtocol()
        {
            // Direct instantiation - no reflection
            // return new ArpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateStpProtocol()
        {
            // return new StpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateCdpProtocol()
        {
            // return new CdpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateLldpProtocol()
        {
            // return new LldpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateOspfProtocol()
        {
            // return new OspfProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateBgpProtocol()
        {
            // return new BgpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateEigrpProtocol()
        {
            // return new EigrpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateRipProtocol()
        {
            // return new RipProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateIsisProtocol()
        {
            // return new IsisProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateVrrpProtocol()
        {
            // return new VrrpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateHsrpProtocol()
        {
            // return new HsrpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateSnmpProtocol()
        {
            // return new SnmpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateSshProtocol()
        {
            // return new SshProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateTelnetProtocol()
        {
            // return new TelnetProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }

        private static IDeviceProtocol CreateHttpProtocol()
        {
            // return new HttpProtocol();
            throw new NotImplementedException("Protocol factory not yet implemented");
        }
    }

    /// <summary>
    /// Extension methods for easier protocol registration
    /// </summary>
    public static class ProtocolRegistryExtensions
    {
        /// <summary>
        /// Register all supported protocols on this device based on vendor
        /// </summary>
        public static void RegisterVendorProtocols(this INetworkDevice device)
        {
            ProtocolRegistry.RegisterDeviceProtocols(device);
        }

        /// <summary>
        /// Register a specific protocol on this device if supported
        /// </summary>
        public static bool TryRegisterProtocol(this INetworkDevice device, NetworkProtocolType type)
        {
            if (ProtocolRegistry.TryGetProtocol(type, out var registration))
            {
                if (registration.SupportsVendor(device.Vendor))
                {
                    var protocol = registration.Factory();
                    if (protocol != null)
                    {
                        device.AddProtocol(protocol);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}