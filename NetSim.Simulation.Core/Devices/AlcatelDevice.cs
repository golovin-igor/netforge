using NetSim.Simulation.Common;
using NetSim.Simulation.Core;
using NetSim.Simulation.Protocols.Implementations;

namespace NetSim.Simulation.Devices
{
    /// <summary>
    /// Alcatel-Lucent network device with OmniSwitch operating system
    /// </summary>
    public class AlcatelDevice : NetworkDevice
    {
        public AlcatelDevice(string name) : base(name)
        {
            Vendor = "Alcatel";
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Register common protocols
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new StpProtocol());
            RegisterProtocol(new LldpProtocol());
            RegisterProtocol(new RipProtocol());
            RegisterProtocol(new ArpProtocol());
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Alcatel devices use port numbering like 1/1/1, 1/1/2, etc.
            var interfaces = GetAllInterfaces();
            interfaces["1/1/1"] = new Configuration.InterfaceConfig("1/1/1", this);
            interfaces["1/1/2"] = new Configuration.InterfaceConfig("1/1/2", this);
            interfaces["1/1/3"] = new Configuration.InterfaceConfig("1/1/3", this);
            interfaces["1/1/4"] = new Configuration.InterfaceConfig("1/1/4", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Alcatel handlers to ensure they are available for tests
            var registry = new NetSim.Simulation.CliHandlers.Alcatel.AlcatelHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            var mode = GetCurrentModeEnum();
            
            return mode switch
            {
                DeviceMode.User => $"{Hostname}->",
                DeviceMode.Privileged => $"A:{Hostname}#",
                DeviceMode.Config => $"A:{Hostname}(config)#",
                DeviceMode.Interface => $"A:{Hostname}(config-if)#",
                _ => $"{Hostname}>"
            };
        }

        public override string ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            // Use the base class implementation for actual command processing
            // This will use the vendor discovery system to find appropriate handlers
            return base.ProcessCommand(command);
        }

        // Expose interface creation for command handlers
        public void AddInterface(string name)
        {
            if (!Interfaces.ContainsKey(name))
            {
                Interfaces[name] = new Configuration.InterfaceConfig(name, this);
            }
        }
    }
}

