using NetForge.Simulation.DataTypes;

namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Service for creating vendor-specific protocols and handlers via IoC
    /// </summary>
    public interface IVendorService
    {
        /// <summary>
        /// Create a protocol instance for a specific vendor and protocol type
        /// </summary>
        object? CreateProtocol(string vendorName, NetworkProtocolType protocolType);

        /// <summary>
        /// Create a CLI handler for a specific vendor
        /// </summary>
        object? CreateHandler(string vendorName, string handlerName);

        /// <summary>
        /// Get all protocols supported by a vendor
        /// </summary>
        IEnumerable<object> GetVendorProtocols(string vendorName);

        /// <summary>
        /// Get all handlers for a vendor
        /// </summary>
        IEnumerable<object> GetVendorHandlers(string vendorName);

        /// <summary>
        /// Register handlers for a specific device
        /// </summary>
        void RegisterDeviceHandlers(object device, object handlerManager);

        /// <summary>
        /// Initialize protocols for a specific device
        /// </summary>
        void InitializeDeviceProtocols(object device);
    }
}