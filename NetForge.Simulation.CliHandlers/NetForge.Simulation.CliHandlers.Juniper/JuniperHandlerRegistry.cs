using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Registry for Juniper CLI handlers
    /// </summary>
    public class JuniperHandlerRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "Juniper";
        public override int Priority => 180;

        public override void RegisterHandlers(CliHandlerManager manager)
        {
            // Register Juniper vendor-specific handlers - CRITICAL functionality
            manager.RegisterHandler(new Basic.ConfigureCommandHandler());
            manager.RegisterHandler(new Basic.CommitCommandHandler());
            manager.RegisterHandler(new Basic.RollbackCommandHandler());
            manager.RegisterHandler(new Basic.ExitCommandHandler());
            manager.RegisterHandler(new Basic.DeleteCommandHandler());
            manager.RegisterHandler(new Basic.PingCommandHandler());
            
            // Register Juniper show handlers
            manager.RegisterHandler(new Show.ShowCommandHandler());
        }

        public override IVendorContext CreateVendorContext(INetworkDevice device)
        {
            if (device is NetworkDevice networkDevice)
            {
                return new JuniperVendorContext(networkDevice);
            }
            
            throw new ArgumentException($"Device type {device.GetType().Name} is not compatible with Juniper handler registry");
        }

        public override IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "switch", "router" };
        }

        public override void Initialize()
        {
            // Register Juniper vendor context factory
            VendorContextFactory.RegisterVendorContext("juniper", device => new JuniperVendorContext(device));
            base.Initialize();
        }

        public override bool CanHandle(string vendorName)
        {
            var juniperNames = new[] { "juniper", "junos", "juniper networks" };
            return juniperNames.Any(name => name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
        }

        public override void Cleanup()
        {
            // Cleanup Juniper-specific resources if needed
            base.Cleanup();
        }
    }
} 
