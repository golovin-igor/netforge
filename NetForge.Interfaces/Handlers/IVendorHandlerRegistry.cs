using NetForge.Interfaces.Cli;
using NetForge.Interfaces.Vendors;
using NetForge.Interfaces.Devices;

namespace NetForge.Interfaces.Handlers
{
    /// <summary>
    /// DEPRECATED: Use IVendorDescriptor and vendor system instead.
    /// Legacy interface for registering vendor-specific CLI handlers
    /// </summary>
    [Obsolete("Use IVendorDescriptor and vendor system instead")]
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
