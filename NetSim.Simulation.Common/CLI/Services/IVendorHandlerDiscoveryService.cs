using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Services
{
    /// <summary>
    /// Service for discovering and managing vendor-specific CLI handler registries
    /// </summary>
    public interface IVendorHandlerDiscoveryService
    {
        /// <summary>
        /// Discovers all available vendor handler registries
        /// </summary>
        /// <returns>Collection of vendor handler registries ordered by priority</returns>
        IEnumerable<IVendorHandlerRegistry> DiscoverVendorRegistries();

        /// <summary>
        /// Gets the appropriate vendor registry for a device
        /// </summary>
        /// <param name="device">The network device</param>
        /// <returns>The vendor registry that can handle the device, or null if none found</returns>
        IVendorHandlerRegistry? GetVendorRegistry(INetworkDevice device);

        /// <summary>
        /// Registers a vendor handler registry
        /// </summary>
        /// <param name="registry">The vendor registry to register</param>
        void RegisterVendorRegistry(IVendorHandlerRegistry registry);

        /// <summary>
        /// Gets all registered vendor names
        /// </summary>
        /// <returns>Collection of registered vendor names</returns>
        IEnumerable<string> GetRegisteredVendors();

        /// <summary>
        /// Checks if a vendor is supported
        /// </summary>
        /// <param name="vendorName">The vendor name to check</param>
        /// <returns>True if the vendor is supported</returns>
        bool IsVendorSupported(string vendorName);
    }
} 
