using NetSim.Simulation.Common;
using NetSim.Simulation.Core;
using NetSim.Simulation.Protocols.Implementations;

namespace NetSim.Simulation.Devices
{
    /// <summary>
    /// Basic Broadcom-based switch implementation
    /// </summary>
    public class BroadcomDevice : NetworkDevice
    {
        public BroadcomDevice(string name) : base(name)
        {
            Vendor = "Broadcom";
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Register common protocols
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new StpProtocol());
            RegisterProtocol(new LldpProtocol());
            RegisterProtocol(new ArpProtocol());
        }

        protected override void InitializeDefaultInterfaces()
        {
            Interfaces["ethernet1/1"] = new Configuration.InterfaceConfig("ethernet1/1", this);
            Interfaces["ethernet1/2"] = new Configuration.InterfaceConfig("ethernet1/2", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Broadcom handlers to ensure they are available for tests
            var registry = new NetSim.Simulation.CliHandlers.Broadcom.BroadcomHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
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

            var result = CommandManager.ProcessCommand(command);
            if (result != null)
            {
                var output = result.Output;
                if (!output.EndsWith("\n"))
                    output += "\n";
                return output + GetPrompt();
            }

            return $"Invalid command: {command}\n" + GetPrompt();
        }
    }
}

