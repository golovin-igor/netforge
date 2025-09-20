using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Enhanced CLI handler manager that automatically registers vendor-specific handlers using the new vendor system
    /// </summary>
    public class VendorAwareCliHandlerManager : CliHandlerManager
    {
        private readonly VendorBasedHandlerService? _vendorHandlerService;
        private readonly INetworkDevice _device;
        private bool _vendorHandlersRegistered = false;

        // Constructor for new vendor-based system
        public VendorAwareCliHandlerManager(INetworkDevice device, VendorBasedHandlerService vendorHandlerService)
            : base(device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _vendorHandlerService = vendorHandlerService ?? throw new ArgumentNullException(nameof(vendorHandlerService));
        }

        // Legacy constructor for backward compatibility
        public VendorAwareCliHandlerManager(INetworkDevice device, IVendorHandlerDiscoveryService? discoveryService = null)
            : base(device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _vendorHandlerService = null; // Will fall back to legacy behavior
        }

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
                if (_vendorHandlerService != null)
                {
                    // Use new vendor-based system
                    _vendorHandlerService.RegisterDeviceHandlers(_device, this);
                    _vendorHandlersRegistered = true;
                }
                else
                {
                    // Fall back to legacy behavior
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
            if (_vendorHandlerService != null)
            {
                // Use new vendor-based system
                var supportedVendors = _vendorHandlerService.GetRegisteredVendors().ToList();
                var deviceVendor = _device.Vendor;
                var isVendorSupported = !string.IsNullOrEmpty(deviceVendor) &&
                                       _vendorHandlerService.IsVendorSupported(deviceVendor);

                return new VendorHandlerInfo
                {
                    DeviceVendor = deviceVendor ?? "Unknown",
                    IsVendorSupported = isVendorSupported,
                    SupportedVendors = supportedVendors,
                    VendorHandlersRegistered = _vendorHandlersRegistered
                };
            }
            else
            {
                // Legacy fallback
                return new VendorHandlerInfo
                {
                    DeviceVendor = _device.Vendor ?? "Unknown",
                    IsVendorSupported = false,
                    SupportedVendors = new List<string>(),
                    VendorHandlersRegistered = _vendorHandlersRegistered
                };
            }
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
            // This method is kept for backward compatibility but no longer used
            // in the new vendor-based system
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
