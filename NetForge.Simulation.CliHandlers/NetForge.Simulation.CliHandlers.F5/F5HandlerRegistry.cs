using NetForge.Interfaces.Cli;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Handlers.Common;

namespace NetForge.Simulation.CliHandlers.F5
{
    /// <summary>
    /// Registry for F5 BIG-IP CLI handlers
    /// </summary>
    public class F5HandlerRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "F5";
        public override int Priority => 160;

        public override void RegisterHandlers(ICliHandlerManager manager)
        {
            // Register F5 BIG-IP vendor-specific handlers
            manager.RegisterHandler(new Basic.EnableCommandHandler());
            manager.RegisterHandler(new Basic.DisableCommandHandler());
            manager.RegisterHandler(new Basic.PingCommandHandler());
            manager.RegisterHandler(new Basic.ExitCommandHandler());
            manager.RegisterHandler(new Basic.HelpCommandHandler());

            // Register F5 show handlers
            manager.RegisterHandler(new Show.ShowCommandHandler());

            // Register F5 configuration handlers
            manager.RegisterHandler(new Configuration.ConfigureCommandHandler());
            manager.RegisterHandler(new Configuration.ExitCommandHandler());
            manager.RegisterHandler(new Configuration.HostnameCommandHandler());
        }

        public override IVendorContext CreateVendorContext(INetworkDevice device)
        {
            if (device is INetworkDevice networkDevice)
            {
                return new F5VendorContext(networkDevice);
            }

            throw new ArgumentException($"Device type {device.GetType().Name} is not compatible with F5 handler registry");
        }

        public override IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "load-balancer", "adc", "firewall", "router" };
        }

        public override void Initialize()
        {
            // Register F5 vendor context factory
            VendorContextFactory.RegisterVendorContext("f5", device => new F5VendorContext(device));
            base.Initialize();
        }

        public override bool CanHandle(string vendorName)
        {
            var f5Names = new[] { "f5", "big-ip", "f5 networks", "bigip" };
            return f5Names.Any(name => name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
        }

        public override void Cleanup()
        {
            // Cleanup F5-specific resources if needed
            base.Cleanup();
        }
    }
}
