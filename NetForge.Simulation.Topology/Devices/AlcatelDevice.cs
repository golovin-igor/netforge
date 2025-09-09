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
        public override string DeviceType => "Switch";

        public AlcatelDevice(string name) : base(name, "Alcatel")
        {
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Alcatel devices use port numbering like 1/1/1, 1/1/2, etc.
            AddInterface("1/1/1", new InterfaceConfig("1/1/1", this));
            AddInterface("1/1/2", new InterfaceConfig("1/1/2", this));
            AddInterface("1/1/3", new InterfaceConfig("1/1/3", this));
            AddInterface("1/1/4", new InterfaceConfig("1/1/4", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Alcatel-specific handler registration would go here
            // For now, use default behavior
        }

        public override string GetPrompt()
        {
            var mode = GetCurrentModeEnum();
            var hostname = GetHostname();

            return mode switch
            {
                DeviceMode.User => $"{hostname}->",
                DeviceMode.Privileged => $"A:{hostname}#",
                DeviceMode.Config => $"A:{hostname}(config)#",
                DeviceMode.Interface => $"A:{hostname}(config-if)#",
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

