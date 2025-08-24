using NetForge.Simulation.Common;
using NetForge.Simulation.Interfaces;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.CliHandlers.Services;
using NetForge.Simulation.CliHandlers.Extensions;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Core;
using NetForge.Simulation.Protocols.Implementations;

namespace NetForge.Simulation.Devices
{
    /// <summary>
    /// Simple Linux host implementation with vendor-agnostic CLI handlers
    /// </summary>
    public class LinuxDevice : NetworkDevice
    {
        private VendorAwareCliHandlerManager? _vendorHandlerManager;

        public LinuxDevice(string name) : base(name)
        {
            Vendor = "Linux";
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();
        }

        protected override void InitializeDefaultInterfaces()
        {
            Interfaces["eth0"] = new InterfaceConfig("eth0", this);
            Interfaces["eth1"] = new InterfaceConfig("eth1", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Linux handlers to ensure they are available for tests
            var registry = new NetForge.Simulation.CliHandlers.Linux.LinuxHandlerRegistry();
            registry.RegisterHandlers(CommandManager);

            // Initialize vendor-aware handler manager
            _vendorHandlerManager = VendorHandlerFactory.CreateWithDiscovery(this);

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Linux" vendor
            AutoRegisterProtocols();
        }

        public override string GetPrompt()
        {
            // Use vendor-specific prompt format
            var mode = CurrentMode == DeviceMode.Privileged ? "root" : "user";
            return mode == "root" ? $"{Hostname}# " : $"{Hostname}$";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            // Process command using vendor-aware handler manager
            var result = _vendorHandlerManager != null ? 
                        await _vendorHandlerManager.ProcessCommandAsync(command) : 
                        await CommandManager.ProcessCommandAsync(command);
            
            if (result != null)
            {
                var output = result.Output;
                if (!output.EndsWith("\n"))
                    output += "\n";
                return output + GetPrompt();
            }

            return $"bash: {command}: command not found\n" + GetPrompt();
        }
    }
}

