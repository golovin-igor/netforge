using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Common.Vendors;

namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Declarative descriptor for a network device vendor, defining all supported protocols and handlers
    /// </summary>
    public interface IVendorDescriptor
    {
        /// <summary>
        /// Vendor name (e.g., "Cisco", "Juniper", "Arista")
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Vendor display name for UI purposes
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Vendor description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Supported device models/types for this vendor
        /// </summary>
        IEnumerable<DeviceModelDescriptor> SupportedModels { get; }

        /// <summary>
        /// Supported protocols for this vendor
        /// </summary>
        IEnumerable<ProtocolDescriptor> SupportedProtocols { get; }

        /// <summary>
        /// CLI handler configurations for this vendor
        /// </summary>
        IEnumerable<HandlerDescriptor> CliHandlers { get; }

        /// <summary>
        /// Vendor-specific configuration
        /// </summary>
        VendorConfiguration Configuration { get; }

        /// <summary>
        /// Priority for this vendor (higher values have priority in conflicts)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this vendor supports a specific device model
        /// </summary>
        bool SupportsModel(string modelName);

        /// <summary>
        /// Check if this vendor supports a specific protocol
        /// </summary>
        bool SupportsProtocol(NetworkProtocolType protocolType);

        /// <summary>
        /// Get protocol descriptor for a specific protocol type
        /// </summary>
        ProtocolDescriptor? GetProtocolDescriptor(NetworkProtocolType protocolType);

        /// <summary>
        /// Get device model descriptor by name
        /// </summary>
        DeviceModelDescriptor? GetModelDescriptor(string modelName);
    }
}