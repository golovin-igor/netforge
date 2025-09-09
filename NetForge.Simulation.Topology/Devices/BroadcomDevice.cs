using NetForge.Simulation.CliHandlers.Broadcom;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Basic Broadcom-based switch implementation
    /// </summary>
    public sealed class BroadcomDevice : NetworkDevice
    {
        public override string DeviceType => "Switch";
        public BroadcomDevice(string name) : base(name, "Broadcom")
        {
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            AddInterface("ethernet1/1", new InterfaceConfig("ethernet1/1", this));
            AddInterface("ethernet1/2", new InterfaceConfig("ethernet1/2", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Broadcom handlers to ensure they are available for tests
            var registry = new BroadcomHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
            // TODO: Update command handler registration with new architecture
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
    }
}

