using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Nokia
{
    public class NokiaHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Nokia";
        public int Priority => 90;

        public bool SupportsDevice(NetworkDevice device)
        {
            return device?.Vendor?.Equals("Nokia", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new NokiaVendorContext((NetworkDevice)device);
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
                new Basic.TracerouteCommandHandler(),
                new Basic.WriteCommandHandler(),
                new Basic.ReloadCommandHandler(),
                new Basic.HistoryCommandHandler(),
                new Basic.CopyCommandHandler(),
                
                // Show handlers
                new Show.ShowCommandHandler()
            };
        }
    }
} 
