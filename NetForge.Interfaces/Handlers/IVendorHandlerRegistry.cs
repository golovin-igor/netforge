using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Common.CLI.Interfaces
{
    /// <summary>
    /// Interface for registering vendor-specific CLI handlers
    /// </summary>
    public interface IVendorHandlerRegistry
    {
        /// <summary>
        /// The vendor name this registry handles
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Register all CLI handlers for this vendor
        /// </summary>
        void RegisterHandlers(ICliHandlerManager manager);

        /// <summary>
        /// Create vendor context for devices of this vendor
        /// </summary>
        IVendorContext CreateVendorContext(INetworkDevice device);

        /// <summary>
        /// Get the priority of this registry (higher priority = loaded first)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this registry can handle the given vendor name
        /// </summary>
        bool CanHandle(string vendorName);

        /// <summary>
        /// Get supported device types for this vendor
        /// </summary>
        IEnumerable<string> GetSupportedDeviceTypes();

        /// <summary>
        /// Initialize the registry (called once at startup)
        /// </summary>
        void Initialize();

        /// <summary>
        /// Cleanup resources (called at shutdown)
        /// </summary>
        void Cleanup();
    }


}
