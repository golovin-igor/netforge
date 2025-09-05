using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Vendor-based handler service that replaces the old registry discovery system
    /// </summary>
    public class VendorBasedHandlerService
    {
        private readonly IVendorService _vendorService;
        private readonly IVendorRegistry _vendorRegistry;

        public VendorBasedHandlerService(IVendorService vendorService, IVendorRegistry vendorRegistry)
        {
            _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
            _vendorRegistry = vendorRegistry ?? throw new ArgumentNullException(nameof(vendorRegistry));
        }

        /// <summary>
        /// Get all handlers for a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Enumerable of handler instances</returns>
        public IEnumerable<object> GetHandlersForVendor(string vendorName)
        {
            return _vendorService.GetVendorHandlers(vendorName);
        }

        /// <summary>
        /// Create a specific handler by name and vendor
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="handlerName">Handler name</param>
        /// <returns>Handler instance or null if not found</returns>
        public object? CreateHandler(string vendorName, string handlerName)
        {
            return _vendorService.CreateHandler(vendorName, handlerName);
        }

        /// <summary>
        /// Register all handlers for a device
        /// </summary>
        /// <param name="device">Device to register handlers for</param>
        /// <param name="handlerManager">Handler manager to register with</param>
        public void RegisterDeviceHandlers(object device, object handlerManager)
        {
            _vendorService.RegisterDeviceHandlers(device, handlerManager);
        }

        /// <summary>
        /// Get all registered vendor names
        /// </summary>
        /// <returns>Enumerable of vendor names</returns>
        public IEnumerable<string> GetRegisteredVendors()
        {
            return _vendorRegistry.GetAllVendors().Select(v => v.VendorName);
        }

        /// <summary>
        /// Check if a vendor is supported
        /// </summary>
        /// <param name="vendorName">Vendor name to check</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool IsVendorSupported(string vendorName)
        {
            return _vendorRegistry.IsVendorRegistered(vendorName);
        }

        /// <summary>
        /// Get vendor descriptor for a device
        /// </summary>
        /// <param name="device">Device to get vendor for</param>
        /// <returns>Vendor descriptor or null if not found</returns>
        public IVendorDescriptor? GetVendorForDevice(object device)
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

            return _vendorRegistry.GetVendor(vendorName);
        }

        /// <summary>
        /// Get supported device types for a vendor
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Enumerable of supported device types</returns>
        public IEnumerable<string> GetSupportedDeviceTypes(string vendorName)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return Enumerable.Empty<string>();

            return vendor.SupportedModels.Select(m => m.DeviceType.ToString()).Distinct();
        }

        /// <summary>
        /// Get vendor configuration (prompts, etc.)
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Vendor configuration or null if vendor not found</returns>
        public object? GetVendorConfiguration(string vendorName)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            return vendor?.Configuration;
        }

        /// <summary>
        /// Get discovery statistics
        /// </summary>
        /// <returns>Dictionary containing statistics</returns>
        public Dictionary<string, object> GetDiscoveryStatistics()
        {
            var vendors = _vendorRegistry.GetAllVendors().ToList();
            var totalHandlers = vendors.SelectMany(v => v.CliHandlers).Count();

            return new Dictionary<string, object>
            {
                ["TotalVendors"] = vendors.Count,
                ["TotalHandlers"] = totalHandlers,
                ["SupportedVendors"] = vendors.Select(v => v.VendorName).ToList(),
                ["HandlersByVendor"] = vendors.ToDictionary(
                    v => v.VendorName,
                    v => v.CliHandlers.Count()),
                ["HandlersByType"] = vendors.SelectMany(v => v.CliHandlers)
                    .GroupBy(h => h.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }
    }
}