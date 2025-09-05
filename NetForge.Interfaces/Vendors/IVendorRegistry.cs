using NetForge.Simulation.DataTypes;

namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Central registry for all vendor descriptors
    /// </summary>
    public interface IVendorRegistry
    {
        /// <summary>
        /// Register a vendor descriptor
        /// </summary>
        void RegisterVendor(IVendorDescriptor vendor);

        /// <summary>
        /// Get a vendor descriptor by name
        /// </summary>
        IVendorDescriptor? GetVendor(string vendorName);

        /// <summary>
        /// Get all registered vendors
        /// </summary>
        IEnumerable<IVendorDescriptor> GetAllVendors();

        /// <summary>
        /// Check if a vendor is registered
        /// </summary>
        bool IsVendorRegistered(string vendorName);

        /// <summary>
        /// Get vendors that support a specific protocol
        /// </summary>
        IEnumerable<IVendorDescriptor> GetVendorsForProtocol(NetworkProtocolType protocolType);

        /// <summary>
        /// Get vendors that support a specific device type
        /// </summary>
        IEnumerable<IVendorDescriptor> GetVendorsForDeviceType(DeviceType deviceType);

        /// <summary>
        /// Discover and register vendors from assemblies
        /// </summary>
        int DiscoverAndRegisterVendors();
    }
}