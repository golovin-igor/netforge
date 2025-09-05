using NetForge.Interfaces.Vendors;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Services;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Protocol manager that uses vendor descriptors instead of plugins
    /// </summary>
    public class VendorAwareProtocolManager : IProtocolPluginManager
    {
        private readonly IVendorService _vendorService;
        private readonly IVendorRegistry _vendorRegistry;
        private readonly Dictionary<string, bool> _enabledProtocols = new();

        public VendorAwareProtocolManager(IVendorService vendorService, IVendorRegistry vendorRegistry)
        {
            _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
            _vendorRegistry = vendorRegistry ?? throw new ArgumentNullException(nameof(vendorRegistry));
        }

        /// <summary>
        /// Create a protocol for a specific device
        /// </summary>
        public object? CreateProtocolForDevice(object device, NetworkProtocolType protocolType)
        {
            if (device == null)
                return null;

            // Get vendor name - try INetworkDevice interface first, then reflection
            string? vendorName = null;
            if (device is INetworkDevice networkDevice)
            {
                vendorName = networkDevice.Vendor;
            }
            else
            {
                // Fall back to reflection for other device types
                var vendorProperty = device.GetType().GetProperty("Vendor");
                vendorName = vendorProperty?.GetValue(device) as string;
            }
            
            if (string.IsNullOrEmpty(vendorName))
                return null;

            return _vendorService.CreateProtocol(vendorName, protocolType);
        }

        /// <summary>
        /// Get all protocols supported by a vendor
        /// </summary>
        public IEnumerable<NetworkProtocolType> GetSupportedProtocols(string vendorName)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return Enumerable.Empty<NetworkProtocolType>();

            return vendor.SupportedProtocols.Select(p => p.ProtocolType);
        }

        /// <summary>
        /// Check if a vendor supports a protocol
        /// </summary>
        public bool VendorSupportsProtocol(string vendorName, NetworkProtocolType protocolType)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            return vendor?.SupportsProtocol(protocolType) ?? false;
        }

        #region IProtocolPluginManager Implementation (Adapter Pattern)

        public bool RegisterPlugin(IProtocolPlugin plugin)
        {
            // Not used in vendor-based system
            return false;
        }

        public bool UnregisterPlugin(string pluginName)
        {
            // Not used in vendor-based system
            return false;
        }

        public IProtocolPlugin GetPlugin(string pluginName)
        {
            // Return null as we don't use plugins
            return null;
        }

        public IEnumerable<IProtocolPlugin> GetAllPlugins()
        {
            // Return empty as we don't use plugins
            return Enumerable.Empty<IProtocolPlugin>();
        }

        public IEnumerable<IProtocolPlugin> GetPluginsForProtocol(NetworkProtocolType networkProtocolType)
        {
            // Return empty as we don't use plugins
            return Enumerable.Empty<IProtocolPlugin>();
        }

        public IEnumerable<IProtocolPlugin> GetPluginsForVendor(string vendorName)
        {
            // Return empty as we don't use plugins
            return Enumerable.Empty<IProtocolPlugin>();
        }

        public bool EnablePlugin(string pluginName)
        {
            _enabledProtocols[pluginName] = true;
            return true;
        }

        public bool DisablePlugin(string pluginName)
        {
            _enabledProtocols[pluginName] = false;
            return true;
        }

        public bool IsPluginEnabled(string pluginName)
        {
            return _enabledProtocols.GetValueOrDefault(pluginName, true);
        }

        public bool ValidatePlugin(string pluginName)
        {
            return true;
        }

        public Dictionary<string, object> GetPluginInfo(string pluginName)
        {
            return new Dictionary<string, object>();
        }

        public IEnumerable<string> GetPluginDependencies(string pluginName)
        {
            return Enumerable.Empty<string>();
        }

        public bool ArePluginDependenciesSatisfied(string pluginName)
        {
            return true;
        }

        public int DiscoverPlugins()
        {
            // Discover vendors instead
            return _vendorRegistry.DiscoverAndRegisterVendors();
        }

        public int LoadPluginsFromAssembly(string assemblyPath)
        {
            return 0;
        }

        public int ReloadPlugins()
        {
            return _vendorRegistry.DiscoverAndRegisterVendors();
        }

        public Dictionary<string, object> GetStatistics()
        {
            var vendors = _vendorRegistry.GetAllVendors().ToList();
            var protocolCount = vendors.SelectMany(v => v.SupportedProtocols).Count();
            var handlerCount = vendors.SelectMany(v => v.CliHandlers).Count();

            return new Dictionary<string, object>
            {
                ["TotalVendors"] = vendors.Count,
                ["TotalProtocols"] = protocolCount,
                ["TotalHandlers"] = handlerCount,
                ["VendorNames"] = vendors.Select(v => v.VendorName).ToList()
            };
        }

        public Dictionary<string, object> GetPluginHealthStatus()
        {
            var vendors = _vendorRegistry.GetAllVendors().Count();
            return new Dictionary<string, object>
            {
                ["Status"] = vendors > 0 ? "Healthy" : "No Vendors",
                ["VendorCount"] = vendors
            };
        }

        #endregion
    }
}