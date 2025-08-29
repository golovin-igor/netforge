using System.Reflection;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Services
{
    /// <summary>
    /// Service for discovering and managing protocol plugins
    /// Similar to VendorHandlerDiscoveryService but for protocols
    /// </summary>
    public class ProtocolDiscoveryService
    {
        private readonly List<IProtocolPlugin> _plugins = new();
        private bool _isDiscovered = false;
        private readonly object _discoveryLock = new();

        /// <summary>
        /// Discover all available protocol plugins from loaded assemblies
        /// </summary>
        /// <returns>Enumerable of protocol plugins ordered by priority</returns>
        public IEnumerable<IProtocolPlugin> DiscoverProtocolPlugins()
        {
            if (!_isDiscovered)
            {
                lock (_discoveryLock)
                {
                    if (!_isDiscovered)
                    {
                        DiscoverAndRegisterPlugins();
                        _isDiscovered = true;
                    }
                }
            }
            return _plugins.OrderByDescending(p => p.Priority).ToList();
        }

        /// <summary>
        /// Get protocols for a specific vendor, with Telnet always included
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Enumerable of protocol instances</returns>
        public IEnumerable<IDeviceProtocol> GetProtocolsForVendor(string vendorName)
        {
            var protocols = new List<IDeviceProtocol>();

            // Always include Telnet for management (when available)
            var telnetPlugin = DiscoverProtocolPlugins()
                .FirstOrDefault(p => p.ProtocolType == NetworkProtocolType.TELNET);
            if (telnetPlugin != null)
            {
                try
                {
                    protocols.Add(telnetPlugin.CreateProtocol());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Telnet protocol: {ex.Message}");
                }
            }

            // Add vendor-specific protocols (excluding Telnet which is already added)
            foreach (var plugin in DiscoverProtocolPlugins())
            {
                if (plugin.ProtocolType != NetworkProtocolType.TELNET && plugin.SupportsVendor(vendorName))
                {
                    try
                    {
                        var protocol = plugin.CreateProtocol();
                        protocols.Add(protocol);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create protocol {plugin.PluginName}: {ex.Message}");
                    }
                }
            }

            return protocols;
        }

        /// <summary>
        /// Get a specific protocol instance by type and vendor
        /// </summary>
        /// <param name="networkProtocolType">Protocol type to create</param>
        /// <param name="vendorName">Vendor name (default: Generic)</param>
        /// <returns>Protocol instance or null if not available</returns>
        public IDeviceProtocol GetProtocol(NetworkProtocolType networkProtocolType, string vendorName = "Generic")
        {
            var plugin = DiscoverProtocolPlugins()
                .Where(p => p.ProtocolType == networkProtocolType && p.SupportsVendor(vendorName))
                .OrderByDescending(p => p.Priority)
                .FirstOrDefault();

            return plugin?.CreateProtocol();
        }

        /// <summary>
        /// Get all plugins for a specific protocol type
        /// </summary>
        /// <param name="networkProtocolType">Protocol type</param>
        /// <returns>Enumerable of plugins</returns>
        public IEnumerable<IProtocolPlugin> GetPluginsForProtocol(NetworkProtocolType networkProtocolType)
        {
            return DiscoverProtocolPlugins()
                .Where(p => p.ProtocolType == networkProtocolType)
                .OrderByDescending(p => p.Priority);
        }

        /// <summary>
        /// Get all supported vendors across all plugins
        /// </summary>
        /// <returns>Enumerable of vendor names</returns>
        public IEnumerable<string> GetSupportedVendors()
        {
            return DiscoverProtocolPlugins()
                .SelectMany(p => p.GetSupportedVendors())
                .Distinct()
                .OrderBy(v => v);
        }

        /// <summary>
        /// Check if a specific protocol type is available
        /// </summary>
        /// <param name="networkProtocolType">Protocol type to check</param>
        /// <returns>True if available, false otherwise</returns>
        public bool IsProtocolAvailable(NetworkProtocolType networkProtocolType)
        {
            return DiscoverProtocolPlugins().Any(p => p.ProtocolType == networkProtocolType);
        }

        /// <summary>
        /// Manually register a protocol plugin
        /// </summary>
        /// <param name="plugin">Plugin to register</param>
        public void RegisterPlugin(IProtocolPlugin plugin)
        {
            if (plugin != null && !_plugins.Any(p => p.PluginName == plugin.PluginName))
            {
                _plugins.Add(plugin);
            }
        }

        /// <summary>
        /// Get discovery statistics
        /// </summary>
        /// <returns>Discovery statistics</returns>
        public Dictionary<string, object> GetDiscoveryStatistics()
        {
            var plugins = DiscoverProtocolPlugins().ToList();

            return new Dictionary<string, object>
            {
                ["TotalPlugins"] = plugins.Count,
                ["ProtocolTypes"] = plugins.Select(p => p.ProtocolType).Distinct().Count(),
                ["SupportedVendors"] = GetSupportedVendors().Count(),
                ["PluginsByType"] = plugins.GroupBy(p => p.ProtocolType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                ["PluginsByVendor"] = plugins.SelectMany(p => p.GetSupportedVendors())
                    .GroupBy(v => v)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        /// <summary>
        /// Discover and register protocol plugins from loaded assemblies
        /// </summary>
        private void DiscoverAndRegisterPlugins()
        {
            try
            {
                // Find assemblies that contain protocol plugins
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic &&
                               (a.FullName?.Contains("NetForge.Simulation.Protocols.") ?? false) &&
                               !a.FullName.Contains("Common"))
                    .ToList();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // Skip system assemblies for performance
                        if (IsSystemAssembly(assembly))
                            continue;

                        var pluginTypes = assembly.GetTypes()
                            .Where(t => typeof(IProtocolPlugin).IsAssignableFrom(t) &&
                                       !t.IsInterface &&
                                       !t.IsAbstract &&
                                       t.GetConstructor(Type.EmptyTypes) != null)
                            .ToList();

                        foreach (var pluginType in pluginTypes)
                        {
                            try
                            {
                                var plugin = (IProtocolPlugin)Activator.CreateInstance(pluginType);
                                if (plugin != null && plugin.IsValid())
                                {
                                    _plugins.Add(plugin);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log but ignore failed plugin instantiation
                                Console.WriteLine($"Failed to instantiate protocol plugin {pluginType.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Log and ignore reflection errors
                        Console.WriteLine($"Failed to load types from assembly {assembly.FullName}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Log and ignore other assembly scanning errors
                        Console.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and ignore critical discovery errors
                Console.WriteLine($"Critical error during protocol plugin discovery: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if an assembly is a system assembly (for performance optimization)
        /// </summary>
        /// <param name="assembly">Assembly to check</param>
        /// <returns>True if system assembly, false otherwise</returns>
        private static bool IsSystemAssembly(Assembly assembly)
        {
            var assemblyName = assembly.FullName ?? "";

            return assemblyName.StartsWith("System.") ||
                   assemblyName.StartsWith("Microsoft.") ||
                   assemblyName.StartsWith("netstandard") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("Newtonsoft.") ||
                   assemblyName.StartsWith("xunit") ||
                   assemblyName.StartsWith("Moq") ||
                   assemblyName == "System" ||
                   assemblyName.Contains("resources");
        }
    }
}
