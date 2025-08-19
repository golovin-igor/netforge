using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Interfaces;

namespace NetForge.Simulation.CliHandlers.Anira
{
    /// <summary>
    /// Registry for Anira CLI handlers
    /// </summary>
    public class AniraHandlerRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "Anira";
        public override int Priority => 100; // Lower priority than Cisco

        public override void RegisterHandlers(CliHandlerManager manager)
        {
            // Register basic handlers (only the ones that actually exist)
            manager.RegisterHandler(new Basic.EnableCommandHandler());
            manager.RegisterHandler(new Basic.DisableCommandHandler());
            manager.RegisterHandler(new Basic.ExitCommandHandler());
            manager.RegisterHandler(new Basic.PingCommandHandler());
            
            // Register configuration handlers (these exist in ConfigurationHandlers.cs)
            manager.RegisterHandler(new Configuration.ConfigureCommandHandler());
            manager.RegisterHandler(new Configuration.InterfaceCommandHandler());
            manager.RegisterHandler(new Configuration.HostnameCommandHandler());
            manager.RegisterHandler(new Configuration.IpAddressCommandHandler());
            manager.RegisterHandler(new Configuration.ShutdownCommandHandler());
            manager.RegisterHandler(new Configuration.NoShutdownCommandHandler());
        }

        public override IVendorContext CreateVendorContext(INetworkDevice device)
        {
            if (device is NetForge.Simulation.Common.NetworkDevice networkDevice)
            {
                return new AniraVendorContext(networkDevice);
            }
            
            throw new ArgumentException($"Device type {device.GetType().Name} is not compatible with Anira handler registry");
        }

        public override IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "anira", "switch", "router" };
        }

        public override void Initialize()
        {
            // Register Anira vendor context factory
            VendorContextFactory.RegisterVendorContext("anira", device => new AniraVendorContext(device));
            
            // Any other Anira-specific initialization
            base.Initialize();
        }

        public override bool CanHandle(string vendorName)
        {
            // Handle Anira vendor name variations
            var aniraNames = new[] { "anira", "anira networks", "anira systems" };
            return aniraNames.Any(name => name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
        }

        public override void Cleanup()
        {
            // Cleanup Anira-specific resources if needed
            base.Cleanup();
        }
    }
} 
