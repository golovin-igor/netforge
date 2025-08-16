using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Protocols.Common
{
    /// <summary>
    /// Base implementation for protocol plugins that enables auto-discovery
    /// Similar to the CLI handler plugin pattern but simplified for protocols
    /// </summary>
    public abstract class ProtocolPluginBase : IProtocolPlugin
    {
        /// <summary>
        /// Human-readable name of the plugin
        /// </summary>
        public abstract string PluginName { get; }
        
        /// <summary>
        /// Version of the plugin
        /// </summary>
        public virtual string Version => "1.0.0";
        
        /// <summary>
        /// The type of protocol this plugin provides
        /// </summary>
        public abstract ProtocolType ProtocolType { get; }
        
        /// <summary>
        /// Priority for this plugin (higher values loaded first)
        /// Use higher priorities for vendor-specific implementations
        /// Default: 100 (generic), Vendor-specific: 200+
        /// </summary>
        public virtual int Priority => 100;
        
        /// <summary>
        /// Create a new instance of the protocol
        /// </summary>
        /// <returns>New protocol instance</returns>
        public abstract IDeviceProtocol CreateProtocol();
        
        /// <summary>
        /// Check if this plugin supports a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to check</param>
        /// <returns>True if supported, false otherwise</returns>
        public virtual bool SupportsVendor(string vendorName)
        {
            return GetSupportedVendors().Contains(vendorName, StringComparer.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Get the list of vendor names this plugin supports
        /// Override for vendor-specific plugins
        /// </summary>
        /// <returns>Enumerable of supported vendor names</returns>
        public virtual IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Generic" };
        }
        
        /// <summary>
        /// Validate that the plugin can create protocols
        /// </summary>
        /// <returns>True if plugin is valid, false otherwise</returns>
        public virtual bool IsValid()
        {
            try
            {
                var protocol = CreateProtocol();
                return protocol != null && protocol.Type == ProtocolType;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get plugin information for debugging and monitoring
        /// </summary>
        /// <returns>Plugin information</returns>
        public virtual Dictionary<string, object> GetPluginInfo()
        {
            return new Dictionary<string, object>
            {
                ["PluginName"] = PluginName,
                ["Version"] = Version,
                ["ProtocolType"] = ProtocolType.ToString(),
                ["Priority"] = Priority,
                ["SupportedVendors"] = GetSupportedVendors().ToList(),
                ["IsValid"] = IsValid()
            };
        }
        
        /// <summary>
        /// String representation of the plugin
        /// </summary>
        /// <returns>Plugin description</returns>
        public override string ToString()
        {
            return $"{PluginName} v{Version} ({ProtocolType}, Priority: {Priority})";
        }
    }
}