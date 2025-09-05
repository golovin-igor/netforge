using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Huawei
{
    public class HuaweiHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Huawei";
        public int Priority => 110;

        public bool SupportsDevice(INetworkDevice device)
        {
            return device?.Vendor?.Equals("Huawei", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new HuaweiVendorContext(device);
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
