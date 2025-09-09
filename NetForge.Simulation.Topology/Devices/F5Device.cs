using NetForge.Simulation.CliHandlers.F5;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// F5 BIG-IP device implementation using vendor registry system
    /// </summary>
    public sealed class F5Device : NetworkDevice
    {
        public override string DeviceType => "Firewall";
        public F5Device(string name) : base(name, "F5")
        {
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            // F5 BIG-IP typically uses interface names like 1.1, 1.2, etc.
            AddInterface("1.1", new InterfaceConfig("1.1", this));
            AddInterface("1.2", new InterfaceConfig("1.2", this));
            AddInterface("1.3", new InterfaceConfig("1.3", this));
            AddInterface("1.4", new InterfaceConfig("1.4", this));

            // Set default IP addresses for management interface
            var mgmtInterface = GetInterface("1.1");
            mgmtInterface.IpAddress = "192.168.1.100";
            mgmtInterface.SubnetMask = "255.255.255.0";
            mgmtInterface.IsUp = true;
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register F5 handlers to ensure they are available
            var registry = new F5HandlerRegistry();
            // TODO: Update command handler registration with new architecture
            // registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            var hostname = GetHostname() ?? "F5";

            return GetCurrentModeEnum() switch
            {
                DeviceMode.User => $"{hostname}>",
                DeviceMode.Privileged => $"{hostname}#",
                DeviceMode.Configuration => $"{hostname}(config)#",
                DeviceMode.Interface => $"{hostname}(config-if)#",
                _ => $"{hostname}#"
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
                AddInterface(name, new InterfaceConfig(name, this));
            }
        }

        /// <summary>
        /// F5 BIG-IP specific method to create LTM pool
        /// </summary>
        public void CreateLtmPool(string poolName, string[] members)
        {
            // Implementation for LTM pool creation
            // This would be called by F5-specific command handlers
        }

        /// <summary>
        /// F5 BIG-IP specific method to create LTM virtual server
        /// </summary>
        public void CreateLtmVirtual(string virtualName, string destination, string pool)
        {
            // Implementation for LTM virtual server creation
            // This would be called by F5-specific command handlers
        }

        /// <summary>
        /// F5 BIG-IP specific method to create LTM node
        /// </summary>
        public void CreateLtmNode(string nodeName, string address, int port)
        {
            // Implementation for LTM node creation
            // This would be called by F5-specific command handlers
        }
    }
}
