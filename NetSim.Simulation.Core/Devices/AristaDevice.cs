using NetSim.Simulation.Common;
using NetSim.Simulation.Configuration;
using NetSim.Simulation.Core;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Protocols.Security;
using PortChannelConfig = NetSim.Simulation.Configuration.PortChannel;

namespace NetSim.Simulation.Devices
{
    public class AristaDevice : NetworkDevice
    {
        private string currentRouterProtocol = "";
        private int currentVlanId = 0;
        private int currentAclNumber = 0;

        public AristaDevice(string name) : base(name)
        {
            Vendor = "Arista";
            InitializeDefaultInterfaces();
            RegisterCommonHandlers();
            RegisterDeviceSpecificHandlers();

            // Register common protocols for Arista devices
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new StpProtocol());
            RegisterProtocol(new LldpProtocol()); // Added LLDP (standard for Arista)
            RegisterProtocol(new ArpProtocol());
            // RegisterProtocol(new RipProtocol()); // Less common by default
            // RegisterProtocol(new IsisProtocol()); // Less common by default
            // Arista devices typically use LLDP. We could add CdpProtocol for compatibility testing if needed.
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
            var registry = new NetSim.Simulation.CliHandlers.Arista.AristaHandlerRegistry();
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
                DeviceMode.Router => $"{Hostname}(config-router-{currentRouterProtocol})#",
                DeviceMode.Vlan => $"{Hostname}(config-vlan-{Vlans.Keys.LastOrDefault()})#",
                DeviceMode.Acl => $"{Hostname}(config-acl-{currentAclNumber})#",
                _ => $"{Hostname}>"
            };
        }

        public override string ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            // Use the base class implementation for actual command processing
            // This will use the vendor discovery system to find appropriate handlers
            return base.ProcessCommand(command);
        }

        // Device-specific public methods for handlers
        public string GetCurrentRouterProtocol() => currentRouterProtocol;
        public void SetCurrentRouterProtocol(string protocol) => currentRouterProtocol = protocol;
        
        public int GetCurrentVlanId() => currentVlanId;
        public void SetCurrentVlanId(int vlanId) => currentVlanId = vlanId;
        
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
                PortChannels[channelId] = new PortChannelConfig(channelId);
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
                Interfaces[name] = new InterfaceConfig(name);
            }
        }

        public void SetCurrentAclNumber(int aclNumber)
        {
            currentAclNumber = aclNumber;
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
        public List<InterfaceConfig> GetInterfaces() => Interfaces.Values.ToList();
        
        public string GetMode() => base.CurrentMode.ToModeString();

        /// <summary>
        /// Get interface by name with alias support
        /// </summary>
        public override InterfaceConfig? GetInterface(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Try direct lookup first
            if (Interfaces.ContainsKey(name))
                return Interfaces[name];

            // Try with alias expansion - now handled by the new vendor-agnostic system
            // Interface alias handling is now managed by the vendor registry system
            // For now, just do a case-insensitive search for flexibility
            foreach (var (interfaceName, config) in Interfaces)
            {
                if (string.Equals(name, interfaceName, StringComparison.OrdinalIgnoreCase))
                    return config;
            }

            return null;
        }
    }
}
