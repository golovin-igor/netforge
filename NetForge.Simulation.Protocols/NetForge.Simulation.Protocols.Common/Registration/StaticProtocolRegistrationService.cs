using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Protocol;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Registration
{
    /// <summary>
    /// Static protocol registration service that avoids reflection entirely
    /// Uses compile-time type safety and declarative vendor configuration
    /// </summary>
    public class StaticProtocolRegistrationService : IProtocolRegistrationService
    {
        /// <summary>
        /// Static protocol factory mapping - no reflection needed
        /// </summary>
        private static readonly Dictionary<NetworkProtocolType, Func<IDeviceProtocol>> _protocolFactories =
            InitializeProtocolFactories();

        private static Dictionary<NetworkProtocolType, Func<IDeviceProtocol>> InitializeProtocolFactories()
        {
            return new Dictionary<NetworkProtocolType, Func<IDeviceProtocol>>
            {
                [NetworkProtocolType.ARP] = StaticProtocolFactory.CreateArp,
                [NetworkProtocolType.BGP] = StaticProtocolFactory.CreateBgp,
                [NetworkProtocolType.CDP] = StaticProtocolFactory.CreateCdp,
                [NetworkProtocolType.EIGRP] = StaticProtocolFactory.CreateEigrp,
                [NetworkProtocolType.HSRP] = StaticProtocolFactory.CreateHsrp,
                [NetworkProtocolType.HTTP] = StaticProtocolFactory.CreateHttp,
                [NetworkProtocolType.IGRP] = StaticProtocolFactory.CreateIgrp,
                [NetworkProtocolType.ISIS] = StaticProtocolFactory.CreateIsis,
                [NetworkProtocolType.LLDP] = StaticProtocolFactory.CreateLldp,
                [NetworkProtocolType.OSPF] = StaticProtocolFactory.CreateOspf,
                [NetworkProtocolType.RIP] = StaticProtocolFactory.CreateRip,
                [NetworkProtocolType.SNMP] = StaticProtocolFactory.CreateSnmp,
                [NetworkProtocolType.SSH] = StaticProtocolFactory.CreateSsh,
                [NetworkProtocolType.STP] = StaticProtocolFactory.CreateStp,
                [NetworkProtocolType.Telnet] = StaticProtocolFactory.CreateTelnet,
                [NetworkProtocolType.VRRP] = StaticProtocolFactory.CreateVrrp
            };
        }

        /// <summary>
        /// Register all protocols for a device based on vendor configuration
        /// No reflection - all static type-safe calls
        /// </summary>
        public async Task RegisterProtocolsAsync(INetworkDevice device)
        {
            if (device == null)
                return;

            // Use static vendor configuration to register protocols
            StaticVendorProtocolConfiguration.RegisterProtocolsForDevice(device, CreateProtocol);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Register a specific protocol if supported by the device vendor
        /// </summary>
        public async Task<bool> RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType)
        {
            if (device == null)
                return false;

            // Check if vendor supports this protocol
            if (!device.VendorSupportsProtocol(protocolType))
                return false;

            // Create and register the protocol
            var protocol = CreateProtocol(protocolType);
            if (protocol != null)
            {
                device.AddProtocol(protocol);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all supported protocols for a vendor
        /// </summary>
        public NetworkProtocolType[] GetSupportedProtocols(string vendor)
        {
            return StaticVendorProtocolConfiguration.GetVendorProtocols(vendor);
        }

        /// <summary>
        /// Check if a protocol is supported by a vendor
        /// </summary>
        public bool IsProtocolSupported(string vendor, NetworkProtocolType protocolType)
        {
            return StaticVendorProtocolConfiguration.VendorSupportsProtocol(vendor, protocolType);
        }

        /// <summary>
        /// Create a protocol instance without reflection
        /// </summary>
        private static IDeviceProtocol? CreateProtocol(NetworkProtocolType protocolType)
        {
            if (_protocolFactories.TryGetValue(protocolType, out var factory))
            {
                return factory();
            }
            return null;
        }
    }

    /// <summary>
    /// Interface for protocol registration service
    /// </summary>
    public interface IProtocolRegistrationService
    {
        Task RegisterProtocolsAsync(INetworkDevice device);
        Task<bool> RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType);
        NetworkProtocolType[] GetSupportedProtocols(string vendor);
        bool IsProtocolSupported(string vendor, NetworkProtocolType protocolType);
    }

    /// <summary>
    /// Static device initialization using compile-time type safety
    /// </summary>
    public static class StaticDeviceInitializer
    {
        /// <summary>
        /// Initialize a device with vendor-specific protocols
        /// Replaces the old reflection-based system
        /// </summary>
        public static async Task InitializeDeviceAsync(INetworkDevice device)
        {
            if (device == null)
                return;

            var registrationService = new StaticProtocolRegistrationService();
            await registrationService.RegisterProtocolsAsync(device);

            // Log the registration results
            var vendor = device.Vendor ?? "Generic";
            var supportedProtocols = registrationService.GetSupportedProtocols(vendor);

            device.AddLogEntry($"Device {device.Name} initialized with {supportedProtocols.Length} supported protocols for vendor {vendor}");
        }

        /// <summary>
        /// Register protocols on multiple devices
        /// </summary>
        public static async Task InitializeDevicesAsync(IEnumerable<INetworkDevice> devices)
        {
            var tasks = devices.Select(InitializeDeviceAsync);
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Add a specific protocol to a device if supported
        /// </summary>
        public static async Task<bool> AddProtocolToDeviceAsync(INetworkDevice device, NetworkProtocolType protocolType)
        {
            var registrationService = new StaticProtocolRegistrationService();
            return await registrationService.RegisterProtocolAsync(device, protocolType);
        }
    }
}