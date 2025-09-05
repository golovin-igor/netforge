using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.MikroTik
{
    public class MikroTikHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "MikroTik";
        public int Priority => 100;

        public bool SupportsDevice(INetworkDevice device)
        {
            return device?.Vendor?.Equals("MikroTik", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new MikroTikVendorContext(device);
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
            return new[] { "router", "switch" };
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

                // Show handlers
                new Show.ShowCommandHandler()
            };
        }
    }
}
