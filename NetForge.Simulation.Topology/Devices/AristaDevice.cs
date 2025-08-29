using NetForge.Simulation.CliHandlers.Arista;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Security;

namespace NetForge.Simulation.Topology.Devices
{
    public sealed class AristaDevice : NetworkDevice
    {
        private string _currentRouterProtocol = "";
        private int _currentVlanId = 0;
        private int _currentAclNumber = 0;

        public AristaDevice(string name) : base(name)
        {
            Vendor = "Arista";
            InitializeDefaultInterfaces();
            RegisterCommonHandlers();
            RegisterDeviceSpecificHandlers();

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Arista" vendor
            AutoRegisterProtocols();
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for an Arista switch
            Interfaces["Ethernet1"] = new InterfaceConfig("Ethernet1", this);
            Interfaces["Ethernet2"] = new InterfaceConfig("Ethernet2", this);
            Interfaces["Ethernet3"] = new InterfaceConfig("Ethernet3", this);
            Interfaces["Ethernet4"] = new InterfaceConfig("Ethernet4", this);
            Interfaces["Management1"] = new InterfaceConfig("Management1", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Arista handlers to ensure they are available for tests
            var registry = new AristaHandlerRegistry();
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return base.CurrentMode switch
            {
                DeviceMode.User => $"{Hostname}>",
                DeviceMode.Privileged => $"{Hostname}#",
                DeviceMode.Config => $"{Hostname}(config)#",
                DeviceMode.Interface => $"{Hostname}(config-if-{base.CurrentInterface})#",
                DeviceMode.Router => $"{Hostname}(config-router-{_currentRouterProtocol})#",
                DeviceMode.Vlan => $"{Hostname}(config-vlan-{Vlans.Keys.LastOrDefault()})#",
                DeviceMode.Acl => $"{Hostname}(config-acl-{_currentAclNumber})#",
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
            if (!Vlans.ContainsKey(vlanId))
            {
                Vlans[vlanId] = new VlanConfig(vlanId);
            }
        }

        public void SetCurrentVlanName(string name)
        {
            if (Vlans.Count > 0)
            {
                var lastVlan = Vlans.Values.Last();
                lastVlan.Name = name;
            }
        }

        public void InitializeOspf(int processId)
        {
            if (OspfConfig == null)
            {
                OspfConfig = new OspfConfig(processId);
            }
        }

        public void InitializeBgp(int asNumber)
        {
            if (BgpConfig == null)
            {
                BgpConfig = new BgpConfig(asNumber);
            }
        }

        public void InitializeRip()
        {
            if (RipConfig == null)
            {
                RipConfig = new RipConfig();
            }
        }

        public void AppendToRunningConfig(string line)
        {
            RunningConfig.AppendLine(line);
        }

        public bool VlanExists(int vlanId)
        {
            return Vlans.ContainsKey(vlanId);
        }

        public void AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            if (Vlans.ContainsKey(vlanId))
            {
                Vlans[vlanId].Interfaces.Add(interfaceName);
            }
        }

        public void CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            if (!PortChannels.ContainsKey(channelId))
            {
                PortChannels[channelId] = new PortChannel(channelId);
            }
            PortChannels[channelId].MemberInterfaces.Add(interfaceName);
            PortChannels[channelId].Mode = mode;
        }

        public override void AddStaticRoute(string network, string mask, string nextHop, int metric)
        {
            base.AddStaticRoute(network, mask, nextHop, metric);
            // Additional Arista-specific route logic here
        }

        public void AddInterface(string name)
        {
            if (!Interfaces.ContainsKey(name))
            {
                Interfaces[name] = new InterfaceConfig(name, this);
            }
        }

        public void SetCurrentAclNumber(int aclNumber)
        {
            _currentAclNumber = aclNumber;
            if (!AccessLists.ContainsKey(aclNumber))
            {
                AccessLists[aclNumber] = new AccessList(aclNumber);
            }
        }

        // Public getters for compatibility
        public List<VlanConfig> GetVlans() => Vlans.Values.ToList();
        public OspfConfig GetOspfConfig() => OspfConfig;
        public BgpConfig GetBgpConfig() => BgpConfig;
        public RipConfig GetRipConfig() => RipConfig;
        public new List<Route> GetRoutingTable() => RoutingTable;

        // Add missing methods that command handlers expect
        public override void SetMode(string mode)
        {
            base.SetMode(mode);
            // Additional Arista-specific mode logic here
        }
        public new string GetCurrentInterface() => base.CurrentInterface;
        public new void SetCurrentInterface(string iface) => base.CurrentInterface = iface;
        public List<IInterfaceConfig> GetInterfaces() => [.. Interfaces.Values];

        public string GetMode() => base.CurrentMode.ToModeString();

        /// <summary>
        /// Get interface by name with alias support
        /// </summary>
        public override IInterfaceConfig? GetInterface(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Try direct lookup first
            if (Interfaces.TryGetValue(name, out IInterfaceConfig? @interface))
                return @interface;

            // Try with alias expansion - now handled by the new vendor-agnostic system
            // Interface alias handling is now managed by the vendor registry system
            // For now, just do a case-insensitive search for flexibility
            foreach ((string interfaceName, IInterfaceConfig config) in Interfaces)
            {
                if (string.Equals(name, interfaceName, StringComparison.OrdinalIgnoreCase))
                    return config;
            }

            return null;
        }
    }
}
