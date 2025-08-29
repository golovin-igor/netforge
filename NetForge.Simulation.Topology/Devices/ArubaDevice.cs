using System.Text;
using NetForge.Simulation.CliHandlers.Aruba;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Devices
{
    /// <summary>
    /// HPE/Aruba switch running ArubaOS-Switch (formerly ProCurve)
    /// </summary>
    public sealed class ArubaDevice : NetworkDevice
    {
        // Mode shadowing removed - using base class currentMode
        private string _currentRouterProtocol = "";
        private int _currentVlanId = 0;
        private int _currentAclNumber = 0;

        public ArubaDevice(string name) : base(name)
        {
            Vendor = "Aruba";
            // InitializeDefaultInterfaces(); // Called by base constructor
            // RegisterDeviceSpecificHandlers(); // Called by base constructor

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Aruba" vendor
            AutoRegisterProtocols();

            // Add default VLAN 1
            Vlans[1] = new VlanConfig(1, "DEFAULT_VLAN");
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for an Aruba switch
            // Example: 24 1Gbps ports + 4 SFP+ uplink ports
            for (int i = 1; i <= 24; i++)
            {
                Interfaces[$"1/1/{i}"] = new InterfaceConfig($"1/1/{i}", this);
            }
            for (int i = 25; i <= 28; i++)
            {
                Interfaces[$"1/1/{i}"] = new InterfaceConfig($"1/1/{i}", this) { Description = "SFP+ Uplink" };
            }
            Interfaces["mgmt"] = new InterfaceConfig("mgmt", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Aruba handlers to ensure they are available for tests
            var registry = new ArubaHandlerRegistry();
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return base.CurrentMode switch
            {
                DeviceMode.User => $"{Hostname}>",
                DeviceMode.Privileged => $"{Hostname}#",
                DeviceMode.Config => $"{Hostname}(config)#",
                DeviceMode.Interface => $"{Hostname}(eth-{GetSimplifiedInterfaceName(CurrentInterface)})#",
                DeviceMode.Vlan => $"{Hostname}(vlan-{Vlans.Keys.LastOrDefault()})#",
                DeviceMode.Router => $"{Hostname}(config-{_currentRouterProtocol})#",
                _ => $"{Hostname}>"
            };
        }

        private string GetSimplifiedInterfaceName(string interfaceName)
        {
            // Convert "1/1/1" back to "1" for prompt display
            if (interfaceName?.StartsWith("1/1/") == true)
            {
                var portNum = interfaceName.Substring(4);
                return portNum;
            }
            return interfaceName ?? "";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            // Use the command handler manager for all command processing
            if (CommandManager != null)
            {
                var result = await CommandManager.ProcessCommandAsync(command);

                // If command was handled, return the result
                if (result != null)
                {
                    // Check if result already ends with prompt
                    var prompt = GetPrompt();
                    if (result.Output.EndsWith(prompt))
                    {
                        return result.Output;
                    }
                    else
                    {
                        return result.Output + prompt;
                    }
                }
            }

            // If no handler found, return Aruba error format
            return "Invalid input\n" + GetPrompt();
        }

        // All ProcessXXXModeCommand methods removed - now handled by command handlers

        // All ProcessXXXCommand methods removed - now handled by command handlers

        // Helper methods for command handlers
        public string GetMode() => base.CurrentMode.ToModeString();
        public new void SetCurrentMode(string mode) => base.CurrentMode = DeviceModeExtensions.FromModeString(mode);
        public new string GetCurrentInterface() => CurrentInterface;
        public new void SetCurrentInterface(string iface) => CurrentInterface = iface;

        // Aruba-specific helper methods
        public string GetCurrentRouterProtocol() => _currentRouterProtocol;
        public void SetCurrentRouterProtocol(string protocol) => _currentRouterProtocol = protocol;

        public void AppendToRunningConfig(string line)
        {
            RunningConfig.AppendLine(line);
        }

        public void UpdateProtocols()
        {
            ParentNetwork?.UpdateProtocols();
        }

        public void UpdateConnectedRoutesPublic()
        {
            UpdateConnectedRoutes();
        }

        // ProcessShowCommand removed - now handled by ShowCommandHandler

        private string GetMacAddress()
        {
            return "aa-bb-cc-00-01-00";
        }

        public override void SetMode(string mode)
        {
            base.SetMode(mode);
            // Additional Aruba-specific mode logic here
        }

        public int GetCurrentVlanId()
        {
            return _currentVlanId;
        }

        public void SetCurrentVlanId(int vlanId)
        {
            _currentVlanId = vlanId;
        }

        public string GetRunningConfig()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Running configuration:");
            sb.AppendLine("hostname " + Name);

            // VLAN configurations
            foreach (var vlan in Vlans.Values.OrderBy(v => v.Id))
            {
                sb.AppendLine($"vlan {vlan.Id}");
                if (!string.IsNullOrEmpty(vlan.Name))
                    sb.AppendLine($"   name {vlan.Name}");
            }

            // Interface configurations
            foreach (var iface in Interfaces.Values.OrderBy(i => i.Name))
            {
                sb.AppendLine($"interface {iface.Name}");
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    sb.AppendLine($"   ip address {iface.IpAddress} {iface.SubnetMask}");
                if (iface.IsShutdown)
                    sb.AppendLine("   shutdown");
            }

            return sb.ToString();
        }

        public string GetInterfacesStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Interface Status:");
            sb.AppendLine("Port     Status       VLAN   Duplex  Speed   Type");
            sb.AppendLine("-------- ------------ ------ ------- ------- ------------");

            foreach (var iface in Interfaces.Values.OrderBy(i => i.Name))
            {
                var status = iface.IsShutdown ? "down" : "up";
                var vlan = "1"; // Default VLAN
                var duplex = "auto";
                var speed = "auto";
                var type = "1000BASE-T";

                sb.AppendLine($"{iface.Name,-8} {status,-12} {vlan,-6} {duplex,-7} {speed,-7} {type}");
            }

            return sb.ToString();
        }

        public string GetVlansStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("VLAN Status:");
            sb.AppendLine("VLAN ID  Name                              Status");
            sb.AppendLine("-------- --------------------------------- ----------");

            foreach (var vlan in Vlans.Values.OrderBy(v => v.Id))
            {
                var status = "Active";
                sb.AppendLine($"{vlan.Id,-8} {vlan.Name,-33} {status}");
            }

            return sb.ToString();
        }

        public string GetIpInterfaces()
        {
            var sb = new StringBuilder();
            sb.AppendLine("IP Interface Status:");
            sb.AppendLine("Interface  IP Address      Subnet Mask     Status");
            sb.AppendLine("---------- -------------- --------------- ----------");

            foreach (var iface in Interfaces.Values.OrderBy(i => i.Name))
            {
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    var status = iface.IsShutdown ? "down" : "up";
                    sb.AppendLine($"{iface.Name,-10} {iface.IpAddress,-14} {iface.SubnetMask,-15} {status}");
                }
            }

            return sb.ToString();
        }

        public void ClearCounters()
        {
            foreach (var iface in Interfaces.Values)
            {
                iface.RxPackets = 0;
                iface.TxPackets = 0;
                iface.RxBytes = 0;
                iface.TxBytes = 0;
            }
        }

        public void SaveConfig()
        {
            // In a real device, this would save to flash/nvram
            // For simulation, we just acknowledge the save
        }

        public void CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            if (!PortChannels.TryGetValue(channelId, out PortChannel? value))
            {
                value = new PortChannel(channelId);
                PortChannels[channelId] = value;
            }

            value.MemberInterfaces.Add(interfaceName);
            value.Mode = mode;
        }

        public void AddInterfaceToPortChannel(string interfaceName, int channelId)
        {
            if (!PortChannels.TryGetValue(channelId, out PortChannel? value))
            {
                value = new PortChannel(channelId);
                PortChannels[channelId] = value;
            }

            value.MemberInterfaces.Add(interfaceName);
        }
    }
}
