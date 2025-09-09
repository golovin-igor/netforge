using System.Text;
using NetForge.Simulation.CliHandlers.Aruba;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// HPE/Aruba switch running ArubaOS-Switch (formerly ProCurve)
    /// </summary>
    public sealed class ArubaDevice : NetworkDevice
    {
        public override string DeviceType => "Switch";
        
        // Mode shadowing removed - using base class currentMode
        private string _currentRouterProtocol = "";
        private int _currentVlanId = 0;
        private int _currentAclNumber = 0;

        public ArubaDevice(string name) : base(name, "Aruba")
        {
            // InitializeDefaultInterfaces(); // Called by base constructor
            // RegisterDeviceSpecificHandlers(); // Called by base constructor

            // Add default VLAN 1
            AddVlan(1, new VlanConfig(1, "DEFAULT_VLAN"));
            
            // Protocol registration is now handled by the vendor registry system
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for an Aruba switch
            // Example: 24 1Gbps ports + 4 SFP+ uplink ports
            for (int i = 1; i <= 24; i++)
            {
                AddInterface($"1/1/{i}", new InterfaceConfig($"1/1/{i}", this));
            }
            for (int i = 25; i <= 28; i++)
            {
                AddInterface($"1/1/{i}", new InterfaceConfig($"1/1/{i}", this) { Description = "SFP+ Uplink" });
            }
            AddInterface("mgmt", new InterfaceConfig("mgmt", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Aruba handlers to ensure they are available for tests
            var registry = new ArubaHandlerRegistry();
            // TODO: Register handlers with new command processor architecture
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
                DeviceMode.Interface => $"{hostname}(eth-{GetSimplifiedInterfaceName(GetCurrentInterface())})#",
                DeviceMode.Vlan => $"{hostname}(vlan-{GetAllVlans().Values.LastOrDefault()?.Id})#",
                DeviceMode.Router => $"{hostname}(config-{_currentRouterProtocol})#",
                _ => $"{hostname}>"
            };
        }

        private string GetSimplifiedInterfaceName(string? interfaceName)
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
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            // Use the base class implementation for actual command processing
            // This will use the vendor discovery system to find appropriate handlers
            return await base.ProcessCommandAsync(command);
        }

        // All ProcessXXXModeCommand methods removed - now handled by command handlers

        // All ProcessXXXCommand methods removed - now handled by command handlers

        // Helper methods for command handlers
        public string GetMode() => GetCurrentModeEnum().ToModeString();
        public new void SetCurrentMode(string mode) => SetModeEnum(DeviceModeExtensions.FromModeString(mode));
        // GetCurrentInterface and SetCurrentInterface are already available from base class

        // Aruba-specific helper methods
        public string GetCurrentRouterProtocol() => _currentRouterProtocol;
        public void SetCurrentRouterProtocol(string protocol) => _currentRouterProtocol = protocol;

        public void AppendToRunningConfig(string line)
        {
            // TODO: Implement configuration building with new architecture
            // GetRunningConfigBuilder().AppendLine(line);
        }

        public void UpdateProtocols()
        {
            ParentNetwork?.UpdateProtocols();
        }

        public void UpdateConnectedRoutesPublic()
        {
            ForceUpdateConnectedRoutes();
        }

        // ProcessShowCommand removed - now handled by ShowCommandHandler

        private string GetMacAddress()
        {
            return "aa-bb-cc-00-01-00";
        }

        public override void SetMode(string mode)
        {
            SetModeEnum(DeviceModeExtensions.FromModeString(mode));
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
            sb.AppendLine("hostname " + GetHostname());

            // VLAN configurations
            foreach (var vlan in GetAllVlans().Values.OrderBy(v => v.Id))
            {
                sb.AppendLine($"vlan {vlan.Id}");
                if (!string.IsNullOrEmpty(vlan.Name))
                    sb.AppendLine($"   name {vlan.Name}");
            }

            // Interface configurations
            foreach (var iface in GetAllInterfaces().Values.OrderBy(i => i.Name))
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

            foreach (var iface in GetAllInterfaces().Values.OrderBy(i => i.Name))
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

            foreach (var vlan in GetAllVlans().Values.OrderBy(v => v.Id))
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

            foreach (var iface in GetAllInterfaces().Values.OrderBy(i => i.Name))
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
            foreach (var iface in GetAllInterfaces().Values)
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
            var portChannels = GetPortChannels();
            if (!portChannels.TryGetValue(channelId, out PortChannel? value))
            {
                value = new PortChannel(channelId);
                portChannels[channelId] = value;
            }

            value.MemberInterfaces.Add(interfaceName);
            value.Mode = mode;
        }

        public void AddInterfaceToPortChannel(string interfaceName, int channelId)
        {
            var portChannels = GetPortChannels();
            if (!portChannels.TryGetValue(channelId, out PortChannel? value))
            {
                value = new PortChannel(channelId);
                portChannels[channelId] = value;
            }

            value.MemberInterfaces.Add(interfaceName);
        }
    }
}
