using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Dell
{
    /// <summary>
    /// Registry for Dell-specific CLI handlers
    /// </summary>
    public class DellHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Dell";
        public int Priority => 140; // Dell priority

        public bool SupportsDevice(INetworkDevice device)
        {
            return device?.Vendor?.Equals("Dell", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new DellVendorContext(device);
        }

        public void RegisterHandlers(ICliHandlerManager manager)
        {
            // Register Dell basic handlers
            manager.RegisterHandler(new Basic.EnableCommandHandler());
            manager.RegisterHandler(new Basic.DisableCommandHandler());
            manager.RegisterHandler(new Basic.PingCommandHandler());
            manager.RegisterHandler(new Basic.TracerouteCommandHandler());
            manager.RegisterHandler(new Basic.WriteCommandHandler());
            manager.RegisterHandler(new Basic.ReloadCommandHandler());
            manager.RegisterHandler(new Basic.HistoryCommandHandler());
            manager.RegisterHandler(new Basic.HelpCommandHandler());
            manager.RegisterHandler(new Basic.CopyCommandHandler());
            manager.RegisterHandler(new Basic.ClearCommandHandler());

            // Register Dell show handlers - COMPREHENSIVE 795-line functionality
            manager.RegisterHandler(new Show.ShowCommandHandler());

            // Register Dell configuration handlers - COMPREHENSIVE 763-line functionality
            manager.RegisterHandler(new Configuration.ConfigureCommandHandler());
            manager.RegisterHandler(new Configuration.InterfaceCommandHandler());
            manager.RegisterHandler(new Configuration.InterfaceModeCommandHandler());
            manager.RegisterHandler(new Configuration.HostnameCommandHandler());
            manager.RegisterHandler(new Configuration.VlanCommandHandler());
            manager.RegisterHandler(new Configuration.RouterCommandHandler());
            manager.RegisterHandler(new Configuration.RouterModeCommandHandler());
            manager.RegisterHandler(new Configuration.IpRouteCommandHandler());
            manager.RegisterHandler(new Configuration.ExitCommandHandler());
        }

        public bool CanHandle(string vendorName)
        {
            return VendorName.Equals(vendorName, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "switch", "router" };
        }

        public void Initialize()
        {
            // Initialization logic if needed
        }

        public void Cleanup()
        {
            // Cleanup logic if needed
        }

        public List<ICliHandler> GetHandlers()
        {
            return new List<ICliHandler>
            {
                // Basic Commands - will be implemented later
                // new Basic.BasicHandlers.DellPingHandler(),
                // new Basic.BasicHandlers.DellHostnameHandler(),

                // Configuration Commands - will be implemented later
                // new Configuration.ConfigurationHandlers.DellConfigureHandler(),
                // new Configuration.ConfigurationHandlers.DellExitHandler(),

                // Show Commands - will be implemented later
                // new Show.ShowHandlers.DellShowHandler()
            };
        }
    }
}
