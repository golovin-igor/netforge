using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;

namespace NetForge.Interfaces.Protocols
{
    /// <summary>
    /// DEPRECATED: Use IVendorDescriptor and vendor system instead.
    /// Legacy interface for protocol plugins that enable auto-discovery of protocols
    /// </summary>
    [Obsolete("Use IVendorDescriptor and vendor system instead")]
    public interface IProtocolPlugin
    {
        /// <summary>
        /// Human-readable name of the plugin
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Version of the plugin
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The type of protocol this plugin provides
        /// </summary>
        NetworkProtocolType ProtocolType { get; }

        /// <summary>
        /// Priority for this plugin (higher values loaded first)
        /// Used for vendor-specific protocol implementations
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Create a new instance of the protocol
        /// </summary>
        /// <returns>New protocol instance</returns>
        IDeviceProtocol CreateProtocol();

        /// <summary>
        /// Check if this plugin supports a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to check</param>
        /// <returns>True if supported, false otherwise</returns>
        bool SupportsVendor(string vendorName);

        /// <summary>
        /// Get the list of vendor names this plugin supports
        /// </summary>
        /// <returns>Enumerable of supported vendor names</returns>
        IEnumerable<string> GetSupportedVendors();

        /// <summary>
        /// Validate that the plugin can create protocols correctly
        /// </summary>
        /// <returns>True if plugin is valid, false otherwise</returns>
        bool IsValid();
    }
}
