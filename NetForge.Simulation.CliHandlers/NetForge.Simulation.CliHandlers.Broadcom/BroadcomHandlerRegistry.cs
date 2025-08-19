using NetForge.Simulation.Common;
using NetForge.Simulation.Interfaces;

namespace NetForge.Simulation.CliHandlers.Broadcom
{
    public class BroadcomHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Broadcom";
        public int Priority => 70;

        public bool SupportsDevice(NetworkDevice device)
        {
            return device?.Vendor?.Equals("Broadcom", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new BroadcomVendorContext((NetworkDevice)device);
        }

        public void RegisterHandlers(CliHandlerManager manager)
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
