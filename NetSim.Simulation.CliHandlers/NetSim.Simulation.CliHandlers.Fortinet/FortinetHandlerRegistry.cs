using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Fortinet
{
    public class FortinetHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Fortinet";
        public int Priority => 120;

        public bool SupportsDevice(NetworkDevice device)
        {
            return device?.Vendor?.Equals("Fortinet", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new FortinetVendorContext((NetworkDevice)device);
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
            return new[] { "firewall", "switch", "router" };
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
                new Basic.ExecuteCommandHandler(),
                new Basic.ConfigCommandHandler(),
                new Basic.EditCommandHandler(),
                new Basic.SetCommandHandler(),
                new Basic.NextCommandHandler(),
                new Basic.EndCommandHandler(),
                
                // Show handlers
                new Show.ShowCommandHandler(),
                new Show.GetCommandHandler()
            };
        }
    }
} 
