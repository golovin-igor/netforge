using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.CliHandlers.Broadcom
{
    public class BroadcomHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Broadcom";
        public int Priority => 70;

        public bool SupportsDevice(INetworkDevice device)
        {
            return device?.Vendor?.Equals("Broadcom", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new BroadcomVendorContext(device);
        }

        public void RegisterHandlers(ICliHandlerManager manager)
        {
            var handlers = GetHandlers();
            foreach (var handler in handlers)
            {
                manager.RegisterHandler(handler);
            }
        }

        public bool CanHandle(string vendorName)
        {
            return VendorName.Equals(vendorName, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "switch", "router" };
        }

        public void Initialize() { }
        public void Cleanup() { }

        public List<ICliHandler> GetHandlers()
        {
            return new List<ICliHandler>
            {
                // Basic handlers
                new Basic.EnableCommandHandler(),
                new Basic.PingCommandHandler(),
                new Basic.WriteCommandHandler(),
                new Basic.ReloadCommandHandler(),
                new Basic.HistoryCommandHandler(),
                new Basic.CopyCommandHandler(),
                new Basic.TracerouteCommandHandler(),

                // Show handlers
                new Show.ShowCommandHandler()
            };
        }
    }
}
