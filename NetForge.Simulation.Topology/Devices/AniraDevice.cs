using NetForge.Simulation.CliHandlers.Anira;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Simple Anira device implementation using vendor registry system
    /// </summary>
    public sealed class AniraDevice : NetworkDevice
    {
        public override string DeviceType => "Switch";
        public AniraDevice(string name) : base(name, "Anira")
        {
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            AddInterface("ge-0/0/0", new InterfaceConfig("ge-0/0/0", this));
            AddInterface("ge-0/0/1", new InterfaceConfig("ge-0/0/1", this));
            AddInterface("ge-0/0/2", new InterfaceConfig("ge-0/0/2", this));
            AddInterface("ge-0/0/3", new InterfaceConfig("ge-0/0/3", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Anira handlers to ensure they are available for tests
            var registry = new AniraHandlerRegistry();
            // TODO: Implement handler registration with new architecture
            // registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            var mode = GetCurrentModeEnum();
            var hostname = GetHostname();

            return mode switch
            {
                DeviceMode.User => $"{hostname}>",
                DeviceMode.Privileged => $"{hostname}#",
                DeviceMode.Config => $"{hostname}(config)#",
                DeviceMode.Interface => $"{hostname}(config-if)#",
                _ => $"{hostname}>"
            };
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
        public void AddNewInterface(string name)
        {
            if (GetInterface(name) == null)
            {
                AddInterface(name, new InterfaceConfig(name, this));
            }
        }
    }
}
