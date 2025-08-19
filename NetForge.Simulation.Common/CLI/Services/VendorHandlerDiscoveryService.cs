using System.Reflection;
using NetForge.Simulation.Interfaces;

namespace NetForge.Simulation.CliHandlers.Services
{
    /// <summary>
    /// Service for discovering and managing vendor-specific CLI handler registries
    /// </summary>
    public class VendorHandlerDiscoveryService : IVendorHandlerDiscoveryService
    {
        private readonly List<IVendorHandlerRegistry> _registries = new();
        private bool _isDiscovered = false;

        public VendorHandlerDiscoveryService()
        {
        }

        /// <summary>
        /// Discovers all available vendor handler registries from loaded assemblies
        /// </summary>
        public IEnumerable<IVendorHandlerRegistry> DiscoverVendorRegistries()
        {
            if (!_isDiscovered)
            {
                DiscoverAndRegisterVendorRegistries();
                _isDiscovered = true;
            }

            return _registries.OrderByDescending(r => r.Priority).ToList();
        }

        /// <summary>
        /// Gets the appropriate vendor registry for a device
        /// </summary>
        public IVendorHandlerRegistry? GetVendorRegistry(INetworkDevice device)
        {
            if (!_isDiscovered)
            {
                DiscoverVendorRegistries();
            }

            var vendorName = device.Vendor;
            if (string.IsNullOrEmpty(vendorName))
            {
                return null;
            }

            // Find registry that can handle this vendor, ordered by priority
            return _registries
                .OrderByDescending(r => r.Priority)
                .FirstOrDefault(r => r.CanHandle(vendorName));
        }

        /// <summary>
        /// Manually registers a vendor handler registry
        /// </summary>
        public void RegisterVendorRegistry(IVendorHandlerRegistry registry)
        {
            if (!_registries.Contains(registry))
            {
                _registries.Add(registry);
            }
        }

        /// <summary>
        /// Gets all registered vendor names
        /// </summary>
        public IEnumerable<string> GetRegisteredVendors()
        {
            if (!_isDiscovered)
            {
                DiscoverVendorRegistries();
            }

            return _registries.Select(r => r.VendorName).Distinct();
        }

        /// <summary>
        /// Checks if a vendor is supported
        /// </summary>
        public bool IsVendorSupported(string vendorName)
        {
            if (!_isDiscovered)
            {
                DiscoverVendorRegistries();
            }

            return _registries.Any(r => r.CanHandle(vendorName));
        }

        /// <summary>
        /// Discovers vendor registries from loaded assemblies
        /// </summary>
        private void DiscoverAndRegisterVendorRegistries()
        {
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && a.FullName != null)
                    .ToList();

                foreach (var assembly in loadedAssemblies)
                {
                    try
                    {
                        // Skip system assemblies for performance
                        if (IsSystemAssembly(assembly))
                            continue;

                        var registryTypes = assembly.GetTypes()
                            .Where(t => typeof(IVendorHandlerRegistry).IsAssignableFrom(t) 
                                       && !t.IsInterface 
                                       && !t.IsAbstract 
                                       && t.GetConstructor(Type.EmptyTypes) != null)
                            .ToList();

                        foreach (var registryType in registryTypes)
                        {
                            try
                            {
                                var registry = (IVendorHandlerRegistry)Activator.CreateInstance(registryType)!;
                                RegisterVendorRegistry(registry);
                            }
                            catch
                            {
                                // Ignore failed registry instantiation
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Ignore reflection errors
                    }
                    catch
                    {
                        // Ignore other assembly scanning errors
                    }
                }
            }
            catch
            {
                // Ignore critical discovery errors
            }
        }

        /// <summary>
        /// Checks if an assembly is a system assembly (for performance optimization)
        /// </summary>
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
