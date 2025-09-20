using NetForge.Simulation.CliHandlers.Arista;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Security;

namespace NetForge.Simulation.Topology.Devices
{
    public sealed class AristaDevice : NetworkDevice
    {
        public override string DeviceType => "Switch";
        private string _currentRouterProtocol = "";
        private int _currentVlanId = 0;
        private int _currentAclNumber = 0;

        public AristaDevice(string name) : base(name, "Arista")
        {
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for an Arista switch
            AddInterface("Ethernet1", new InterfaceConfig("Ethernet1", this));
            AddInterface("Ethernet2", new InterfaceConfig("Ethernet2", this));
            AddInterface("Ethernet3", new InterfaceConfig("Ethernet3", this));
            AddInterface("Ethernet4", new InterfaceConfig("Ethernet4", this));
            AddInterface("Management1", new InterfaceConfig("Management1", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Arista handlers to ensure they are available for tests
            var registry = new AristaHandlerRegistry();
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
                DeviceMode.Router => $"{hostname}(config-router-{_currentRouterProtocol})#",
                DeviceMode.Vlan => $"{hostname}(config-vlan-{GetVlans().LastOrDefault()?.Id})#",
                DeviceMode.Acl => $"{hostname}(config-acl-{_currentAclNumber})#",
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

        // Device-specific public methods for handlers
        public string GetCurrentRouterProtocol() => _currentRouterProtocol;
        public void SetCurrentRouterProtocol(string protocol) => _currentRouterProtocol = protocol;

        public int GetCurrentVlanId() => _currentVlanId;
        public void SetCurrentVlanId(int vlanId) => _currentVlanId = vlanId;

        public new void SetHostname(string name)
        {
            base.SetHostname(name);
            // Additional Arista-specific hostname logic here
        }

        public void CreateOrSelectVlan(int vlanId)
        {
            var vlans = GetVlans();
            if (!vlans.Any(v => v.Id == vlanId))
            {
                AddVlan(vlanId, new VlanConfig(vlanId));
            }
        }

        public void SetCurrentVlanName(string name)
        {
            var vlans = GetVlans();
            if (vlans.Count > 0)
            {
                var lastVlan = vlans.Last();
                lastVlan.Name = name;
            }
        }

        public void InitializeOspf(int processId)
        {
            if (GetOspfConfiguration() == null)
            {
                SetOspfConfiguration(new OspfConfig(processId));
            }
        }

        public void InitializeBgp(int asNumber)
        {
            if (GetBgpConfiguration() == null)
            {
                SetBgpConfiguration(new BgpConfig(asNumber));
            }
        }

        public void InitializeRip()
        {
            if (GetRipConfiguration() == null)
            {
                SetRipConfiguration(new RipConfig());
            }
        }

        public void AppendToRunningConfig(string line)
        {
            // TODO: Implement with new architecture
        }

        public bool VlanExists(int vlanId)
        {
            return GetVlans().Any(v => v.Id == vlanId);
        }

        public void AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            var vlan = GetVlans().FirstOrDefault(v => v.Id == vlanId);
            if (vlan != null)
            {
                vlan.Interfaces.Add(interfaceName);
            }
        }

        public void CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            var portChannels = GetPortChannels();
            if (!portChannels.ContainsKey(channelId))
            {
                portChannels[channelId] = new PortChannel(channelId);
            }
            portChannels[channelId].MemberInterfaces.Add(interfaceName);
            portChannels[channelId].Mode = mode;
        }

        public override void AddStaticRoute(string network, string mask, string nextHop, int metric)
        {
            base.AddStaticRoute(network, mask, nextHop, metric);
            // Additional Arista-specific route logic here
        }

        public void AddNewInterface(string name)
        {
            if (GetInterface(name) == null)
            {
                AddInterface(name, new InterfaceConfig(name, this));
            }
        }

        public void SetCurrentAclNumber(int aclNumber)
        {
            _currentAclNumber = aclNumber;
            var acls = GetAccessLists();
            if (!acls.ContainsKey(aclNumber))
            {
                acls[aclNumber] = new AccessList(aclNumber);
            }
        }

        // Public getters for compatibility - use base class methods
        public new List<VlanConfig> GetVlans() => new List<VlanConfig>(); // TODO: Implement with new architecture
        public OspfConfig GetOspfConfig() => GetOspfConfiguration();
        public BgpConfig GetBgpConfig() => GetBgpConfiguration();
        public RipConfig GetRipConfig() => GetRipConfiguration();
        public new List<Route> GetRoutingTable() => base.GetRoutingTable();

        // Add missing methods that command handlers expect
        public override void SetMode(string mode)
        {
            SetModeEnum(DeviceModeExtensions.FromModeString(mode));
            // Additional Arista-specific mode logic here
        }
        public new string GetCurrentInterface() => string.Empty; // TODO: Implement with new architecture
        public new void SetCurrentInterface(string iface) { /* TODO: Implement with new architecture */ }
        public List<IInterfaceConfig> GetInterfaces() => [.. GetAllInterfaces().Values];

        public string GetMode() => GetCurrentModeEnum().ToModeString();

        /// <summary>
        /// Show the running configuration in Arista EOS format
        /// </summary>
        public string ShowRunningConfig()
        {
            // Use the new configuration builder
            var configBuilder = new Arista.AristaConfigurationBuilder(this, _configurationManager);
            return configBuilder.BuildRunningConfiguration();
        }

        /// <summary>
        /// Get interface by name with alias support
        /// </summary>
        public override IInterfaceConfig? GetInterface(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Try direct lookup first
            var interfaces = GetAllInterfaces();
            if (interfaces.TryGetValue(name, out IInterfaceConfig? @interface))
                return @interface;

            // Try with alias expansion - now handled by the new vendor-agnostic system
            // Interface alias handling is now managed by the vendor registry system
            // For now, just do a case-insensitive search for flexibility
            foreach ((string interfaceName, IInterfaceConfig config) in interfaces)
            {
                if (string.Equals(name, interfaceName, StringComparison.OrdinalIgnoreCase))
                    return config;
            }

            return null;
        }
    }
}
