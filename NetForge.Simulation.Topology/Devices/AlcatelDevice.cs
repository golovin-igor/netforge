using NetForge.Simulation.CliHandlers.Alcatel;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Alcatel-Lucent network device with OmniSwitch operating system
    /// </summary>
    public sealed class AlcatelDevice : NetworkDevice
    {
        public AlcatelDevice(string name) : base(name)
        {
            Vendor = "Alcatel";
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Alcatel" vendor
            AutoRegisterProtocols();
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Alcatel devices use port numbering like 1/1/1, 1/1/2, etc.
            var interfaces = GetAllInterfaces();
            interfaces["1/1/1"] = new InterfaceConfig("1/1/1", this);
            interfaces["1/1/2"] = new InterfaceConfig("1/1/2", this);
            interfaces["1/1/3"] = new InterfaceConfig("1/1/3", this);
            interfaces["1/1/4"] = new InterfaceConfig("1/1/4", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Alcatel handlers to ensure they are available for tests
            var registry = new AlcatelHandlerRegistry();
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

