using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Extensions
{
    /// <summary>
    /// Factory for creating vendor-aware CLI handler managers
    /// </summary>
    public static class VendorHandlerFactory
    {
        private static IVendorHandlerDiscoveryService? _discoveryService;

        /// <summary>
        /// Gets or creates the global discovery service instance
        /// </summary>
        public static IVendorHandlerDiscoveryService GetDiscoveryService()
        {
            return _discoveryService ??= new VendorHandlerDiscoveryService();
        }

        /// <summary>
        /// Sets a custom discovery service (for testing or custom implementations)
        /// </summary>
        /// <param name="discoveryService">The custom discovery service</param>
        public static void SetDiscoveryService(IVendorHandlerDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        /// <summary>
        /// Creates a vendor-aware CLI handler manager for a device
        /// </summary>
        /// <param name="device">The network device</param>
        /// <param name="discoveryService">Optional discovery service (uses global if not provided)</param>
        /// <returns>A configured vendor-aware CLI handler manager</returns>
        public static VendorAwareCliHandlerManager CreateCliHandlerManager(INetworkDevice device,
            IVendorHandlerDiscoveryService? discoveryService = null)
        {
            discoveryService ??= GetDiscoveryService();
            return new VendorAwareCliHandlerManager(device, discoveryService);
        }

        /// <summary>
        /// Creates a vendor-aware CLI handler manager with automatic vendor discovery
        /// </summary>
        /// <param name="device">The network device</param>
        /// <returns>A configured vendor-aware CLI handler manager</returns>
        public static VendorAwareCliHandlerManager CreateWithDiscovery(INetworkDevice device)
        {
            var discoveryService = GetDiscoveryService();
            var manager = new VendorAwareCliHandlerManager(device, discoveryService);

            // Trigger discovery and registration
            manager.RegisterVendorHandlers();

            return manager;
        }

        /// <summary>
        /// Resets the discovery service (useful for testing)
        /// </summary>
        public static void Reset()
        {
            _discoveryService = null;
        }
    }


}
