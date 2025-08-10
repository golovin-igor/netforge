using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.CliHandlers;
using NetSim.Simulation.CliHandlers.Services;
using NetSim.Simulation.CliHandlers.Extensions;
using NetSim.Simulation.Common.Configuration;
using NetSim.Simulation.Core;
using NetSim.Simulation.Protocols.Implementations;

namespace NetSim.Simulation.Devices
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
            var registry = new NetSim.Simulation.CliHandlers.Linux.LinuxHandlerRegistry();
            registry.RegisterHandlers(CommandManager);

            // Initialize vendor-aware handler manager
            _vendorHandlerManager = VendorHandlerFactory.CreateWithDiscovery(this);

            // Register routing protocols for state management
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new RipProtocol());
            RegisterProtocol(new ArpProtocol());
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

