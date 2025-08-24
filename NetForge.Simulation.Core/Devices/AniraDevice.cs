using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Core;

namespace NetForge.Simulation.Devices
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

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Anira" vendor
            AutoRegisterProtocols();
        }

        protected override void InitializeDefaultInterfaces()
        {
            Interfaces["ge-0/0/0"] = new InterfaceConfig("ge-0/0/0", this);
            Interfaces["ge-0/0/1"] = new InterfaceConfig("ge-0/0/1", this);
            Interfaces["ge-0/0/2"] = new InterfaceConfig("ge-0/0/2", this);
            Interfaces["ge-0/0/3"] = new InterfaceConfig("ge-0/0/3", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Anira handlers to ensure they are available for tests
            var registry = new NetForge.Simulation.CliHandlers.Anira.AniraHandlerRegistry();
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

            // Use the base class implementation for actual command processing
            // This will use the vendor discovery system to find appropriate handlers
            return await base.ProcessCommandAsync(command);
        }

        // Expose interface creation for command handlers
        public void AddInterface(string name)
        {
            if (!Interfaces.ContainsKey(name))
            {
                Interfaces[name] = new InterfaceConfig(name, this);
            }
        }
    }
}
