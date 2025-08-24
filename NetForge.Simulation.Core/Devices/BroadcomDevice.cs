using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Core;
using NetForge.Simulation.Protocols.Implementations;

namespace NetForge.Simulation.Devices
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

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Broadcom" vendor
            AutoRegisterProtocols();
        }

        protected override void InitializeDefaultInterfaces()
        {
            Interfaces["ethernet1/1"] = new InterfaceConfig("ethernet1/1", this);
            Interfaces["ethernet1/2"] = new InterfaceConfig("ethernet1/2", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Broadcom handlers to ensure they are available for tests
            var registry = new NetForge.Simulation.CliHandlers.Broadcom.BroadcomHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return CurrentMode == DeviceMode.Privileged ? $"{Hostname}#" : $"{Hostname}>";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            var result = await CommandManager.ProcessCommandAsync(command);
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

