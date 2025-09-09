using NetForge.Simulation.CliHandlers.Linux;
using NetForge.Simulation.Common.CLI.Extensions;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Simple Linux host implementation with vendor-agnostic CLI handlers
    /// </summary>
    public sealed class LinuxDevice : NetworkDevice
    {
        public override string DeviceType => "Host";
        private VendorAwareCliHandlerManager? _vendorHandlerManager;

        public LinuxDevice(string name) : base(name, "Linux")
        {
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();
        }

        protected override void InitializeDefaultInterfaces()
        {
            AddInterface("eth0", new InterfaceConfig("eth0", this));
            AddInterface("eth1", new InterfaceConfig("eth1", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Linux handlers to ensure they are available for tests
            var registry = new LinuxHandlerRegistry();
            // registry.RegisterHandlers(CommandManager); // Commented out until command manager is properly implemented

            // Initialize vendor-aware handler manager
            _vendorHandlerManager = VendorHandlerFactory.CreateWithDiscovery(this);

            // Protocol registration is now handled by the vendor registry system
        }

        public override string GetPrompt()
        {
            // Use vendor-specific prompt format
            var mode = GetCurrentModeEnum() == DeviceMode.Privileged ? "root" : "user";
            return mode == "root" ? $"{GetHostname()}# " : $"{GetHostname()}$";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            // Process command using vendor-aware handler manager
            if (_vendorHandlerManager != null)
            {
                var result = await _vendorHandlerManager.ProcessCommandAsync(command);
                if (result != null)
                {
                    var output = result.Output;
                    if (!output.EndsWith("\n"))
                        output += "\n";
                    return output + GetPrompt();
                }
            }

            return $"bash: {command}: command not found\n" + GetPrompt();
        }
    }
}

