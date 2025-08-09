using NetSim.Simulation.Common;
using NetSim.Simulation.Core;
using NetSim.Simulation.Protocols.Implementations;

namespace NetSim.Simulation.Devices
{
    /// <summary>
    /// Simple Anira device implementation using vendor registry system
    /// </summary>
    public class AniraDevice : NetworkDevice
    {
        public AniraDevice(string name) : base(name)
        {
            Vendor = "Anira";
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Register a basic set of common protocols
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new StpProtocol());
            RegisterProtocol(new LldpProtocol());
            RegisterProtocol(new ArpProtocol());
        }

        protected override void InitializeDefaultInterfaces()
        {
            Interfaces["ge-0/0/0"] = new Configuration.InterfaceConfig("ge-0/0/0", this);
            Interfaces["ge-0/0/1"] = new Configuration.InterfaceConfig("ge-0/0/1", this);
            Interfaces["ge-0/0/2"] = new Configuration.InterfaceConfig("ge-0/0/2", this);
            Interfaces["ge-0/0/3"] = new Configuration.InterfaceConfig("ge-0/0/3", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Anira handlers to ensure they are available for tests
            var registry = new NetSim.Simulation.CliHandlers.Anira.AniraHandlerRegistry();
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return CurrentMode == DeviceMode.Privileged ? $"{Hostname}#" : $"{Hostname}>";
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
