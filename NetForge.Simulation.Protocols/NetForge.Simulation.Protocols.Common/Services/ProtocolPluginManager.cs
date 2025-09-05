using NetForge.Interfaces.Protocols;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Services
{
    /// <summary>
    /// Interface for managing protocol plugins - registration, lifecycle, and metadata
    /// Provides advanced plugin management capabilities beyond basic discovery
    /// </summary>
    public interface IProtocolPluginManager
    {
        // Plugin registration and management
        /// <summary>
        /// Register a protocol plugin with the manager
        /// </summary>
        /// <param name="plugin">Plugin to register</param>
        /// <returns>True if successfully registered, false otherwise</returns>
        bool RegisterPlugin(IProtocolPlugin plugin);

        /// <summary>
        /// Unregister a protocol plugin
        /// </summary>
        /// <param name="pluginName">Name of plugin to unregister</param>
        /// <returns>True if successfully unregistered, false otherwise</returns>
        bool UnregisterPlugin(string pluginName);

        /// <summary>
        /// Get a specific plugin by name
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>Plugin instance or null if not found</returns>
        IProtocolPlugin GetPlugin(string pluginName);

        /// <summary>
        /// Get all registered plugins
        /// </summary>
        /// <returns>Enumerable of all registered plugins</returns>
        IEnumerable<IProtocolPlugin> GetAllPlugins();

        /// <summary>
        /// Get plugins for a specific protocol type
        /// </summary>
        /// <param name="networkProtocolType">Protocol type</param>
        /// <returns>Enumerable of plugins supporting the protocol type</returns>
        IEnumerable<IProtocolPlugin> GetPluginsForProtocol(NetworkProtocolType networkProtocolType);

        /// <summary>
        /// Get plugins that support a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Enumerable of plugins supporting the vendor</returns>
        IEnumerable<IProtocolPlugin> GetPluginsForVendor(string vendorName);

        // Plugin lifecycle management
        /// <summary>
        /// Enable a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if successfully enabled, false otherwise</returns>
        bool EnablePlugin(string pluginName);

        /// <summary>
        /// Disable a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if successfully disabled, false otherwise</returns>
        bool DisablePlugin(string pluginName);

        /// <summary>
        /// Check if a plugin is enabled
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if enabled, false otherwise</returns>
        bool IsPluginEnabled(string pluginName);

        /// <summary>
        /// Validate a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidatePlugin(string pluginName);

        // Plugin metadata and information
        /// <summary>
        /// Get detailed information about a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>Dictionary containing plugin metadata</returns>
        Dictionary<string, object> GetPluginInfo(string pluginName);

        /// <summary>
        /// Get plugin dependencies
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>Enumerable of plugin dependencies</returns>
        IEnumerable<string> GetPluginDependencies(string pluginName);

        /// <summary>
        /// Check if plugin dependencies are satisfied
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if dependencies are satisfied, false otherwise</returns>
        bool ArePluginDependenciesSatisfied(string pluginName);

        // Plugin discovery and loading
        /// <summary>
        /// Discover plugins from assemblies
        /// </summary>
        /// <returns>Number of plugins discovered</returns>
        int DiscoverPlugins();

        /// <summary>
        /// Load plugins from a specific assembly
        /// </summary>
        /// <param name="assemblyPath">Path to assembly</param>
        /// <returns>Number of plugins loaded</returns>
        int LoadPluginsFromAssembly(string assemblyPath);

        /// <summary>
        /// Reload all plugins
        /// </summary>
        /// <returns>Number of plugins reloaded</returns>
        int ReloadPlugins();

        // Plugin statistics and monitoring
        /// <summary>
        /// Get plugin manager statistics
        /// </summary>
        /// <returns>Dictionary containing statistics</returns>
        Dictionary<string, object> GetStatistics();

        /// <summary>
        /// Get plugin health status
        /// </summary>
        /// <returns>Dictionary containing health information</returns>
        Dictionary<string, object> GetPluginHealthStatus();
    }

    /// <summary>
    /// Concrete implementation of protocol plugin manager
    /// Provides advanced plugin management capabilities for protocol plugins
    /// </summary>
    public class ProtocolPluginManager : IProtocolPluginManager
    {
        private readonly Dictionary<string, IProtocolPlugin> _plugins = new();
        private readonly Dictionary<string, bool> _pluginEnabledState = new();
        private readonly ProtocolDiscoveryService _discoveryService;
        private readonly object _lockObject = new();

        /// <summary>
        /// Initialize the plugin manager
        /// </summary>
        public ProtocolPluginManager()
        {
            _discoveryService = new ProtocolDiscoveryService();
        }

        /// <summary>
        /// Register a protocol plugin with the manager
        /// </summary>
        /// <param name="plugin">Plugin to register</param>
        /// <returns>True if successfully registered, false otherwise</returns>
        public bool RegisterPlugin(IProtocolPlugin plugin)
        {
            if (plugin == null || string.IsNullOrEmpty(plugin.PluginName))
                return false;

            lock (_lockObject)
            {
                if (_plugins.ContainsKey(plugin.PluginName))
                    return false; // Plugin already registered

                if (!plugin.IsValid())
                    return false; // Plugin validation failed

                _plugins[plugin.PluginName] = plugin;
                _pluginEnabledState[plugin.PluginName] = true;
                return true;
            }
        }

        /// <summary>
        /// Unregister a protocol plugin
        /// </summary>
        /// <param name="pluginName">Name of plugin to unregister</param>
        /// <returns>True if successfully unregistered, false otherwise</returns>
        public bool UnregisterPlugin(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                return false;

            lock (_lockObject)
            {
                var removed = _plugins.Remove(pluginName);
                _pluginEnabledState.Remove(pluginName);
                return removed;
            }
        }

        /// <summary>
        /// Get a specific plugin by name
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>Plugin instance or null if not found</returns>
        public IProtocolPlugin GetPlugin(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                return null;

            lock (_lockObject)
            {
                return _plugins.GetValueOrDefault(pluginName);
            }
        }

        /// <summary>
        /// Get all registered plugins
        /// </summary>
        /// <returns>Enumerable of all registered plugins</returns>
        public IEnumerable<IProtocolPlugin> GetAllPlugins()
        {
            lock (_lockObject)
            {
                return _plugins.Values.ToList();
            }
        }

        /// <summary>
        /// Get plugins for a specific protocol type
        /// </summary>
        /// <param name="networkProtocolType">Protocol type</param>
        /// <returns>Enumerable of plugins supporting the protocol type</returns>
        public IEnumerable<IProtocolPlugin> GetPluginsForProtocol(NetworkProtocolType networkProtocolType)
        {
            lock (_lockObject)
            {
                return _plugins.Values
                    .Where(p => p.ProtocolType == networkProtocolType && IsPluginEnabled(p.PluginName))
                    .OrderByDescending(p => p.Priority)
                    .ToList();
            }
        }

        /// <summary>
        /// Get plugins that support a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Enumerable of plugins supporting the vendor</returns>
        public IEnumerable<IProtocolPlugin> GetPluginsForVendor(string vendorName)
        {
            if (string.IsNullOrEmpty(vendorName))
                return Enumerable.Empty<IProtocolPlugin>();

            lock (_lockObject)
            {
                return _plugins.Values
                    .Where(p => p.SupportsVendor(vendorName) && IsPluginEnabled(p.PluginName))
                    .OrderByDescending(p => p.Priority)
                    .ToList();
            }
        }

        /// <summary>
        /// Enable a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if successfully enabled, false otherwise</returns>
        public bool EnablePlugin(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                return false;

            lock (_lockObject)
            {
                if (!_plugins.ContainsKey(pluginName))
                    return false;

                _pluginEnabledState[pluginName] = true;
                return true;
            }
        }

        /// <summary>
        /// Disable a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if successfully disabled, false otherwise</returns>
        public bool DisablePlugin(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                return false;

            lock (_lockObject)
            {
                if (!_plugins.ContainsKey(pluginName))
                    return false;

                _pluginEnabledState[pluginName] = false;
                return true;
            }
        }

        /// <summary>
        /// Check if a plugin is enabled
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if enabled, false otherwise</returns>
        public bool IsPluginEnabled(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                return false;

            lock (_lockObject)
            {
                return _pluginEnabledState.GetValueOrDefault(pluginName, false);
            }
        }

        /// <summary>
        /// Validate a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidatePlugin(string pluginName)
        {
            var plugin = GetPlugin(pluginName);
            return plugin?.IsValid() ?? false;
        }

        /// <summary>
        /// Get detailed information about a plugin
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>Dictionary containing plugin metadata</returns>
        public Dictionary<string, object> GetPluginInfo(string pluginName)
        {
            var plugin = GetPlugin(pluginName);
            if (plugin == null)
                return new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                ["PluginName"] = plugin.PluginName,
                ["Version"] = plugin.Version,
                ["ProtocolType"] = plugin.ProtocolType.ToString(),
                ["Priority"] = plugin.Priority,
                ["SupportedVendors"] = plugin.GetSupportedVendors().ToList(),
                ["IsEnabled"] = IsPluginEnabled(pluginName),
                ["IsValid"] = plugin.IsValid()
            };
        }

        /// <summary>
        /// Get plugin dependencies
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>Enumerable of plugin dependencies</returns>
        public IEnumerable<string> GetPluginDependencies(string pluginName)
        {
            // For now, return empty list as plugin dependencies are not implemented
            // This can be enhanced in the future
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Check if plugin dependencies are satisfied
        /// </summary>
        /// <param name="pluginName">Plugin name</param>
        /// <returns>True if dependencies are satisfied, false otherwise</returns>
        public bool ArePluginDependenciesSatisfied(string pluginName)
        {
            // For now, return true as plugin dependencies are not implemented
            // This can be enhanced in the future
            return true;
        }

        /// <summary>
        /// Discover plugins from assemblies
        /// </summary>
        /// <returns>Number of plugins discovered</returns>
        public int DiscoverPlugins()
        {
            lock (_lockObject)
            {
                var discovered = _discoveryService.DiscoverProtocolPlugins();
                var count = 0;

                foreach (var plugin in discovered)
                {
                    if (RegisterPlugin(plugin))
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Load plugins from a specific assembly
        /// </summary>
        /// <param name="assemblyPath">Path to assembly</param>
        /// <returns>Number of plugins loaded</returns>
        public int LoadPluginsFromAssembly(string assemblyPath)
        {
            // Implementation would load assembly and discover plugins
            // For now, return 0 as dynamic assembly loading is not implemented
            return 0;
        }

        /// <summary>
        /// Reload all plugins
        /// </summary>
        /// <returns>Number of plugins reloaded</returns>
        public int ReloadPlugins()
        {
            lock (_lockObject)
            {
                _plugins.Clear();
                _pluginEnabledState.Clear();
                return DiscoverPlugins();
            }
        }

        /// <summary>
        /// Get plugin manager statistics
        /// </summary>
        /// <returns>Dictionary containing statistics</returns>
        public Dictionary<string, object> GetStatistics()
        {
            lock (_lockObject)
            {
                var enabledCount = _pluginEnabledState.Values.Count(enabled => enabled);
                var disabledCount = _plugins.Count - enabledCount;

                var protocolTypes = _plugins.Values
                    .Select(p => p.ProtocolType)
                    .Distinct()
                    .Count();

                var vendors = _plugins.Values
                    .SelectMany(p => p.GetSupportedVendors())
                    .Distinct()
                    .Count();

                return new Dictionary<string, object>
                {
                    ["TotalPlugins"] = _plugins.Count,
                    ["EnabledPlugins"] = enabledCount,
                    ["DisabledPlugins"] = disabledCount,
                    ["ProtocolTypes"] = protocolTypes,
                    ["SupportedVendors"] = vendors,
                    ["PluginsByProtocol"] = _plugins.Values
                        .GroupBy(p => p.ProtocolType)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count())
                };
            }
        }

        /// <summary>
        /// Get plugin health status
        /// </summary>
        /// <returns>Dictionary containing health information</returns>
        public Dictionary<string, object> GetPluginHealthStatus()
        {
            lock (_lockObject)
            {
                var validPlugins = 0;
                var invalidPlugins = 0;

                foreach (var plugin in _plugins.Values)
                {
                    if (plugin.IsValid())
                        validPlugins++;
                    else
                        invalidPlugins++;
                }

                var healthStatus = invalidPlugins == 0 ? "Healthy" :
                                  validPlugins > invalidPlugins ? "Warning" : "Critical";

                return new Dictionary<string, object>
                {
                    ["Status"] = healthStatus,
                    ["ValidPlugins"] = validPlugins,
                    ["InvalidPlugins"] = invalidPlugins,
                    ["TotalPlugins"] = _plugins.Count,
                    ["HealthPercentage"] = _plugins.Count > 0 ? (validPlugins * 100 / _plugins.Count) : 100
                };
            }
        }
    }
}
