using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common.Registration;

namespace NetForge.Examples
{
    /// <summary>
    /// Example demonstrating static protocol registration without reflection
    /// Shows compile-time type safety and declarative vendor configuration
    /// </summary>
    public class StaticProtocolRegistrationExample
    {
        /// <summary>
        /// Example 1: Register all vendor protocols automatically
        /// </summary>
        public static async Task RegisterAllVendorProtocolsExample(INetworkDevice device)
        {
            // Automatic registration based on vendor
            await StaticDeviceInitializer.InitializeDeviceAsync(device);

            // The device now has all protocols supported by its vendor
            // For Cisco: ARP, STP, CDP, LLDP, OSPF, BGP, EIGRP, RIP, ISIS, VRRP, HSRP, SNMP, SSH, Telnet, HTTP
            // For Juniper: ARP, STP, LLDP, OSPF, BGP, RIP, ISIS, VRRP, SNMP, SSH, Telnet, HTTP
            // etc.
        }

        /// <summary>
        /// Example 2: Register specific protocols selectively
        /// </summary>
        public static async Task RegisterSpecificProtocolsExample(INetworkDevice device)
        {
            var registrationService = new StaticProtocolRegistrationService();

            // Register only OSPF and BGP
            await registrationService.RegisterProtocolAsync(device, NetworkProtocolType.OSPF);
            await registrationService.RegisterProtocolAsync(device, NetworkProtocolType.BGP);

            // Try to register EIGRP - will only work for Cisco
            var eigrpRegistered = await registrationService.RegisterProtocolAsync(device, NetworkProtocolType.EIGRP);
            if (eigrpRegistered)
            {
                Console.WriteLine("EIGRP registered successfully (Cisco device)");
            }
            else
            {
                Console.WriteLine("EIGRP not supported by this vendor");
            }
        }

        /// <summary>
        /// Example 3: Check vendor capabilities before registration
        /// </summary>
        public static void CheckVendorCapabilitiesExample(INetworkDevice device)
        {
            var vendor = device.Vendor ?? "Generic";

            // Get all supported protocols for this vendor
            var supportedProtocols = StaticVendorProtocolConfiguration.GetVendorProtocols(vendor);
            Console.WriteLine($"Vendor {vendor} supports {supportedProtocols.Length} protocols:");

            foreach (var protocol in supportedProtocols)
            {
                Console.WriteLine($"  - {protocol}");
            }

            // Check specific protocol support
            var supportsCdp = StaticVendorProtocolConfiguration.VendorSupportsProtocol(vendor, NetworkProtocolType.CDP);
            Console.WriteLine($"CDP support: {supportsCdp}");

            var supportsEigrp = StaticVendorProtocolConfiguration.VendorSupportsProtocol(vendor, NetworkProtocolType.EIGRP);
            Console.WriteLine($"EIGRP support: {supportsEigrp}");
        }

        /// <summary>
        /// Example 4: Get default protocols for automatic enablement
        /// </summary>
        public static void GetDefaultProtocolsExample(INetworkDevice device)
        {
            var vendor = device.Vendor ?? "Generic";
            var defaultProtocols = StaticVendorProtocolConfiguration.GetVendorDefaultProtocols(vendor);

            Console.WriteLine($"Default protocols for {vendor}:");
            foreach (var protocol in defaultProtocols)
            {
                Console.WriteLine($"  - {protocol} (auto-enabled)");
            }
        }

        /// <summary>
        /// Example 5: Batch initialize multiple devices
        /// </summary>
        public static async Task BatchInitializeDevicesExample(IEnumerable<INetworkDevice> devices)
        {
            // Initialize all devices in parallel
            await StaticDeviceInitializer.InitializeDevicesAsync(devices);

            Console.WriteLine($"Initialized {devices.Count()} devices with vendor-specific protocols");
        }

        /// <summary>
        /// Example 6: Manual protocol creation without registration service
        /// </summary>
        public static void ManualProtocolCreationExample(INetworkDevice device)
        {
            // Direct protocol creation - no reflection
            var ospfProtocol = StaticProtocolFactory.CreateOspf();
            var bgpProtocol = StaticProtocolFactory.CreateBgp();

            // Add to device manually
            device.AddProtocol(ospfProtocol);
            device.AddProtocol(bgpProtocol);

            Console.WriteLine("Manually added OSPF and BGP protocols");
        }

        /// <summary>
        /// Example showing the complete flow
        /// </summary>
        public static async Task CompleteFlowExample()
        {
            // Create mock devices (normally would come from your device factory)
            var ciscoRouter = CreateMockDevice("Router1", "Cisco", "ISR4000");
            var juniperSwitch = CreateMockDevice("Switch1", "Juniper", "EX4400");
            var aristaSwitch = CreateMockDevice("Switch2", "Arista", "7050X");

            var devices = new[] { ciscoRouter, juniperSwitch, aristaSwitch };

            // 1. Check capabilities
            foreach (var device in devices)
            {
                Console.WriteLine($"\n=== {device.Name} ({device.Vendor} {device.Model}) ===");
                CheckVendorCapabilitiesExample(device);
                GetDefaultProtocolsExample(device);
            }

            // 2. Initialize all devices
            Console.WriteLine("\n=== Initializing Devices ===");
            await BatchInitializeDevicesExample(devices);

            // 3. Add specific protocols to individual devices
            Console.WriteLine("\n=== Adding Specific Protocols ===");
            await RegisterSpecificProtocolsExample(ciscoRouter);

            Console.WriteLine("\nStatic protocol registration complete!");
        }

        // Mock device creation (replace with your actual device factory)
        private static INetworkDevice CreateMockDevice(string name, string vendor, string model)
        {
            // This would be your actual device implementation
            throw new NotImplementedException("Replace with your INetworkDevice implementation");
        }
    }

    /// <summary>
    /// Benefits of this static approach:
    ///
    /// 1. **No Reflection**: All protocol creation uses direct constructor calls
    /// 2. **Compile-Time Safety**: All types are known at compile time
    /// 3. **Performance**: No runtime type discovery or dynamic casting
    /// 4. **Maintainability**: Clear, declarative vendor configurations
    /// 5. **Testability**: Easy to unit test with mock factories
    /// 6. **Extensibility**: New vendors/protocols added by extending static configs
    /// 7. **Debugging**: Full stack traces and IntelliSense support
    /// 8. **AOT Compatibility**: Works with Native AOT compilation
    ///
    /// Migration from reflection-based system:
    /// - Replace VendorProtocolRegistrationService with StaticProtocolRegistrationService
    /// - Replace dynamic protocol discovery with StaticProtocolFactory
    /// - Replace vendor capability reflection with StaticVendorProtocolConfiguration
    /// - Update device initialization to use StaticDeviceInitializer
    /// </summary>
    public static class MigrationNotes
    {
        // This class exists for documentation purposes
    }
}