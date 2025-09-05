using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Simple handler manager that uses vendor descriptors
    /// This is a simplified version that avoids complex CLI interface dependencies
    /// </summary>
    public class VendorAwareHandlerManager
    {
        private readonly IVendorService _vendorService;
        private readonly IVendorRegistry _vendorRegistry;
        private readonly INetworkDevice _device;
        private bool _initialized = false;

        public VendorAwareHandlerManager(
            INetworkDevice device,
            IVendorService vendorService,
            IVendorRegistry vendorRegistry)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
            _vendorRegistry = vendorRegistry ?? throw new ArgumentNullException(nameof(vendorRegistry));
        }

        /// <summary>
        /// Initialize handlers from vendor descriptor
        /// </summary>
        private void InitializeHandlers()
        {
            if (_initialized || string.IsNullOrEmpty(_device.Vendor))
                return;

            // Get handlers for this device's vendor
            var handlers = _vendorService.GetVendorHandlers(_device.Vendor);
            
            // Note: In the full implementation, these would be registered with the device's CLI system
            // For now, we just mark as initialized to avoid repeated initialization
            _initialized = true;
        }

        /// <summary>
        /// Get the number of available handlers for this device's vendor
        /// </summary>
        public int GetHandlerCount()
        {
            InitializeHandlers();
            
            if (string.IsNullOrEmpty(_device.Vendor))
                return 0;
                
            var handlers = _vendorService.GetVendorHandlers(_device.Vendor);
            return handlers.Count();
        }

        /// <summary>
        /// Check if handlers are available for the device's vendor
        /// </summary>
        public bool HasVendorHandlers()
        {
            InitializeHandlers();
            return GetHandlerCount() > 0;
        }

        /// <summary>
        /// Get vendor information
        /// </summary>
        public string GetVendorInfo()
        {
            if (string.IsNullOrEmpty(_device.Vendor))
                return "No vendor specified";

            var vendor = _vendorRegistry.GetVendor(_device.Vendor);
            if (vendor == null)
                return $"Unknown vendor: {_device.Vendor}";

            return $"Vendor: {vendor.DisplayName}, Handlers: {GetHandlerCount()}";
        }
    }
}
