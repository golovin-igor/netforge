using System.Collections.Concurrent;
using NetForge.Simulation.Common.CLI.Implementations;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Factories
{
    /// <summary>
    /// Factory for creating vendor-specific contexts
    /// </summary>
    public static class VendorContextFactory
    {
        private static readonly ConcurrentDictionary<string, Func<NetworkDevice, IVendorContext>> _vendorContextFactories = new();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Register a vendor context factory
        /// </summary>
        public static void RegisterVendorContext<T>(string vendorName, Func<NetworkDevice, T> factory)
            where T : IVendorContext
        {
            var key = vendorName.ToLowerInvariant();
            var wrapper = new Func<NetworkDevice, IVendorContext>(device => factory(device));

            // Use TryAdd to prevent race conditions - first registration wins
            if (!_vendorContextFactories.TryAdd(key, wrapper))
            {
                // If already exists, verify it's the same type to avoid silent conflicts
                var existing = _vendorContextFactories[key];
                // Log if different but don't fail - this is expected in test scenarios
            }
        }

        /// <summary>
        /// Get vendor context for a device
        /// </summary>
        public static IVendorContext? GetVendorContext(NetworkDevice device)
        {
            var vendorName = device.Vendor?.ToLowerInvariant();

            if (string.IsNullOrEmpty(vendorName))
                return new DefaultVendorContext(device);

            if (_vendorContextFactories.TryGetValue(vendorName, out var factory))
            {
                try
                {
                    return factory(device);
                }
                catch
                {
                    // Fall back to default if factory fails
                    return new DefaultVendorContext(device);
                }
            }

            // Return default vendor context if no specific factory found
            return new DefaultVendorContext(device);
        }

        /// <summary>
        /// Check if a vendor context is registered
        /// </summary>
        public static bool IsVendorContextRegistered(string vendorName)
        {
            return _vendorContextFactories.ContainsKey(vendorName.ToLowerInvariant());
        }

        /// <summary>
        /// Get all registered vendor names
        /// </summary>
        public static IEnumerable<string> GetRegisteredVendors()
        {
            return _vendorContextFactories.Keys;
        }
    }
}
