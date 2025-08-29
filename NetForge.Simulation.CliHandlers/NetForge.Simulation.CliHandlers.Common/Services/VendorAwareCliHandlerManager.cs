using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Enhanced CLI handler manager that automatically registers vendor-specific handlers
    /// </summary>
    public class VendorAwareCliHandlerManager(INetworkDevice device, IVendorHandlerDiscoveryService? discoveryService = null)
        : CliHandlerManager(device)
    {
        private readonly IVendorHandlerDiscoveryService _discoveryService = discoveryService ?? new VendorHandlerDiscoveryService();
        private readonly INetworkDevice _device = device;
        private bool _vendorHandlersRegistered = false;

        /// <summary>
        /// Processes a command, automatically registering vendor handlers if needed
        /// </summary>
        public new async Task<CliResult> ProcessCommandAsync(string command)
        {
            // Ensure vendor handlers are registered before processing
            EnsureVendorHandlersRegistered();

            return await base.ProcessCommandAsync(command);
        }

        /// <summary>
        /// Gets completions, automatically registering vendor handlers if needed
        /// </summary>
        public new List<string> GetCompletions(string command)
        {
            // Ensure vendor handlers are registered before getting completions
            EnsureVendorHandlersRegistered();

            return base.GetCompletions(command);
        }

        /// <summary>
        /// Manually triggers vendor handler registration
        /// </summary>
        public void RegisterVendorHandlers()
        {
            if (_vendorHandlersRegistered)
                return;

            try
            {
                // Discover and get the appropriate vendor registry for this device
                var vendorRegistry = _discoveryService.GetVendorRegistry(_device);

                if (vendorRegistry != null)
                {
                    // Initialize the vendor registry
                    vendorRegistry.Initialize();

                    // Register all vendor context factories
                    RegisterVendorContextFactories();

                    // Register vendor-specific handlers
                    vendorRegistry.RegisterHandlers(this);

                    _vendorHandlersRegistered = true;
                }
                else
                {
                    // No specific vendor handlers found, device will use default behavior
                    _vendorHandlersRegistered = true;
                }
            }
            catch (Exception)
            {
                // If vendor registration fails, mark as completed to prevent infinite retries
                // The system will fall back to default behavior
                _vendorHandlersRegistered = true;
            }
        }

        /// <summary>
        /// Gets information about registered vendors
        /// </summary>
        public VendorHandlerInfo GetVendorInfo()
        {
            var supportedVendors = _discoveryService.GetRegisteredVendors().ToList();
            var deviceVendor = _device.Vendor;
            var isVendorSupported = !string.IsNullOrEmpty(deviceVendor) &&
                                   _discoveryService.IsVendorSupported(deviceVendor);

            return new VendorHandlerInfo
            {
                DeviceVendor = deviceVendor ?? "Unknown",
                IsVendorSupported = isVendorSupported,
                SupportedVendors = supportedVendors,
                VendorHandlersRegistered = _vendorHandlersRegistered
            };
        }

        /// <summary>
        /// Forces re-discovery and registration of vendor handlers
        /// </summary>
        public void RefreshVendorHandlers()
        {
            _vendorHandlersRegistered = false;
            RegisterVendorHandlers();
        }

        /// <summary>
        /// Ensures vendor handlers are registered (called automatically)
        /// </summary>
        private void EnsureVendorHandlersRegistered()
        {
            if (!_vendorHandlersRegistered)
            {
                RegisterVendorHandlers();
            }
        }

        /// <summary>
        /// Registers vendor context factories with the factory
        /// </summary>
        private void RegisterVendorContextFactories()
        {
            try
            {
                // Discover all vendor registries and register their context factories
                var vendorRegistries = _discoveryService.DiscoverVendorRegistries();

                foreach (var registry in vendorRegistries)
                {
                    // Register the vendor context factory
                    VendorContextFactory.RegisterVendorContext(
                        registry.VendorName,
                        device => registry.CreateVendorContext(device)
                    );
                }
            }
            catch
            {
                // Ignore registration errors - system will fall back to default context
            }
        }


    }

    /// <summary>
    /// Information about vendor handler registration status
    /// </summary>
    public class VendorHandlerInfo
    {
        public string DeviceVendor { get; set; } = "";
        public bool IsVendorSupported { get; set; }
        public List<string> SupportedVendors { get; set; } = new();
        public bool VendorHandlersRegistered { get; set; }
    }
}
