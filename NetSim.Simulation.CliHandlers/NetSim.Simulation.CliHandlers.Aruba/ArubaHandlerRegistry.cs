using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Common;
using static NetSim.Simulation.CliHandlers.Aruba.Basic.BasicHandlers;
using static NetSim.Simulation.CliHandlers.Aruba.Configuration.ConfigurationHandlers;
using static NetSim.Simulation.CliHandlers.Aruba.Show.ShowHandlers;

namespace NetSim.Simulation.CliHandlers.Aruba
{
    /// <summary>
    /// Registry for Aruba-specific CLI handlers
    /// </summary>
    public class ArubaHandlerRegistry : IVendorHandlerRegistry
    {
        public string VendorName => "Aruba";
        public int Priority => 150; // Aruba priority

        public bool SupportsDevice(NetworkDevice device)
        {
            return device?.Vendor?.Equals("Aruba", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IVendorContext CreateVendorContext(INetworkDevice device)
        {
            return new ArubaVendorContext((NetworkDevice)device);
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
                // Basic Commands
                new Basic.BasicHandlers.ArubaPingHandler(),
                new Basic.BasicHandlers.ArubaHostnameHandler(),
                new Basic.BasicHandlers.ArubaWriteHandler(),
                new Basic.BasicHandlers.ArubaReloadHandler(),
                new Basic.BasicHandlers.ArubaEnableHandler(),
                
                // Configuration Commands  
                new Configuration.ConfigurationHandlers.ArubaConfigureHandler(),
                new Configuration.ConfigurationHandlers.ArubaExitHandler(),
                new Configuration.ConfigurationHandlers.ArubaInterfaceHandler(),
                new Configuration.ConfigurationHandlers.ArubaVlanHandler(),
                new Configuration.ConfigurationHandlers.ArubaIpHandler(),
                new Configuration.ConfigurationHandlers.ArubaSwitchportHandler(),
                new Configuration.ConfigurationHandlers.ArubaShutdownHandler(),
                new Configuration.ConfigurationHandlers.ArubaNoHandler(),
                
                // VLAN Management Commands (migrated)
                new Configuration.ConfigurationHandlers.ArubaVlanNameHandler(),
                new Configuration.ConfigurationHandlers.ArubaVlanTaggedHandler(),
                new Configuration.ConfigurationHandlers.ArubaVlanUntaggedHandler(),
                
                // Routing Commands (migrated)
                new Configuration.ConfigurationHandlers.ArubaIpRouteHandler(),
                
                // Clear Commands (migrated)
                new Configuration.ConfigurationHandlers.ArubaClearHandler(),
                
                // Interface Commands (additional)
                new Configuration.ConfigurationHandlers.ArubaDisableHandler(),
                new Configuration.ConfigurationHandlers.ArubaInterfaceNameHandler(),
                new Configuration.ConfigurationHandlers.ArubaSpeedHandler(),
                new Configuration.ConfigurationHandlers.ArubaDuplexHandler(),
                
                // Show Commands
                new Show.ShowHandlers.ArubaShowHandler()
            };
        }

        public Dictionary<string, List<string>> GetCommandValidationRules()
        {
            return new Dictionary<string, List<string>>
            {
                ["user"] = new List<string> { "ping", "show", "configure", "exit" },
                ["privileged"] = new List<string> { "ping", "show", "configure", "write", "reload", "exit" },
                ["config"] = new List<string> { "hostname", "interface", "vlan", "ip", "exit", "end" },
                ["interface"] = new List<string> { "description", "ip", "switchport", "shutdown", "no", "exit" },
                ["vlan"] = new List<string> { "name", "tagged", "untagged", "exit" }
            };
        }

        public Dictionary<string, string> GetModeTransitions()
        {
            return new Dictionary<string, string>
            {
                ["configure"] = "config",
                ["interface"] = "interface", 
                ["vlan"] = "vlan",
                ["exit"] = "previous",
                ["end"] = "privileged"
            };
        }

        public List<string> GetSupportedModes()
        {
            return new List<string> { "user", "privileged", "config", "interface", "vlan" };
        }
    }
} 
