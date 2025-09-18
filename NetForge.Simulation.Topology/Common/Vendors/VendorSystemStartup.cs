using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Protocols.Common.Services;
using NetForge.Simulation.Vendors.Cisco;
using NetForge.Simulation.Vendors.Juniper;
using NetForge.Simulation.Vendors.Arista;
using NetForge.Simulation.Common.Common;
using NetForge.Interfaces;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Startup configuration helper for the vendor-based system
    /// </summary>
    public static class VendorSystemStartup
    {
        /// <summary>
        /// Configure all vendor system services and replace old plugin-based services
        /// </summary>
        public static IServiceCollection ConfigureVendorSystem(this IServiceCollection services)
        {
            // Register core vendor system
            services.AddVendorSystem();

            // Register all vendor descriptors
            services.AddVendor<CiscoVendorDescriptor>();
            services.AddVendor<JuniperVendorDescriptor>();
            services.AddVendor<AristaVendorDescriptor>();

            // Register vendor-based services
            services.AddSingleton<VendorBasedProtocolService>();
            services.AddSingleton<VendorBasedHandlerService>();

            // Vendor-based protocol management is now handled directly by VendorBasedProtocolService

            // Register factory for creating vendor-aware handler managers
            services.AddTransient<Func<INetworkDevice, VendorAwareHandlerManager>>(provider =>
                device => new VendorAwareHandlerManager(device, 
                    provider.GetRequiredService<IVendorService>(), 
                    provider.GetRequiredService<IVendorRegistry>()));

            return services;
        }

        /// <summary>
        /// Initialize device with vendor-based protocols and handlers
        /// </summary>
        public static async Task InitializeDeviceWithVendorSystemAsync(object device, IServiceProvider serviceProvider)
        {
            try
            {
                if (device is INetworkDevice networkDevice)
                {
                    // Register protocols based on vendor capabilities
                    var protocolRegistrationService = serviceProvider.GetService<IVendorProtocolRegistrationService>();
                    if (protocolRegistrationService != null)
                    {
                        await protocolRegistrationService.RegisterProtocolsAsync(networkDevice);
                    }

                    // Initialize protocols using vendor service
                    var vendorService = serviceProvider.GetService<IVendorService>();
                    vendorService?.InitializeDeviceProtocols(device);
                }

                // Note: Handler initialization happens automatically when VendorAwareCliHandlerManager is created
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize device with vendor system: {ex.Message}");
                // Fall back to default behavior
            }
        }

        /// <summary>
        /// Initialize device with vendor-based protocols and handlers (synchronous overload for backward compatibility)
        /// </summary>
        public static void InitializeDeviceWithVendorSystem(object device, IServiceProvider serviceProvider)
        {
            InitializeDeviceWithVendorSystemAsync(device, serviceProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a CLI handler manager for a device using the vendor system
        /// </summary>
        public static object CreateVendorAwareHandlerManager(INetworkDevice device, IServiceProvider serviceProvider)
        {
            try
            {
                var vendorHandlerService = serviceProvider.GetService<VendorBasedHandlerService>();
                if (vendorHandlerService != null)
                {
                    return new VendorAwareCliHandlerManager(device, vendorHandlerService);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create vendor-aware handler manager: {ex.Message}");
            }

            // Fall back to legacy constructor
            return new VendorAwareCliHandlerManager(device);
        }

        /// <summary>
        /// Get protocol service that replaces the old discovery service
        /// </summary>
        public static VendorBasedProtocolService GetProtocolService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<VendorBasedProtocolService>();
        }

        /// <summary>
        /// Migrate from old plugin-based system to vendor-based system
        /// </summary>
        public static void MigrateFromPluginSystem(IServiceProvider serviceProvider)
        {
            try
            {
                // Get the new vendor-based services
                var vendorRegistry = serviceProvider.GetService<IVendorRegistry>();
                var vendorService = serviceProvider.GetService<IVendorService>();

                if (vendorRegistry != null && vendorService != null)
                {
                    // Discover and register all vendors
                    var discoveredCount = vendorRegistry.DiscoverAndRegisterVendors();
                    Console.WriteLine($"Vendor System: Discovered and registered {discoveredCount} vendors");

                    // Log registered vendors
                    var vendors = vendorRegistry.GetAllVendors().ToList();
                    Console.WriteLine($"Registered vendors: {string.Join(", ", vendors.Select(v => v.VendorName))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during vendor system migration: {ex.Message}");
            }
        }
    }
}