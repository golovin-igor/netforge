using NetForge.Simulation.CliHandlers.Huawei;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Huawei VRP device implementation
    /// </summary>
    public sealed class HuaweiDevice : NetworkDevice
    {
        public override string DeviceType => "Router";
        private int _currentVlanId = 0;
        private int _currentAclNumber = 0;

        public HuaweiDevice(string name) : base(name, "Huawei")
        {
            // VRP uses default VLAN 1 as well
            AddVlan(1, new VlanConfig(1, "default"));
            // InitializeDefaultInterfaces(); // Called by base constructor
            // RegisterDeviceSpecificHandlers(); // Called by base constructor

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Basic interfaces for demonstration
            AddInterface("GigabitEthernet0/0/0", new InterfaceConfig("GigabitEthernet0/0/0", this));
            AddInterface("GigabitEthernet0/0/1", new InterfaceConfig("GigabitEthernet0/0/1", this));
            AddInterface("GigabitEthernet0/0/2", new InterfaceConfig("GigabitEthernet0/0/2", this));
            AddInterface("GigabitEthernet0/0/3", new InterfaceConfig("GigabitEthernet0/0/3", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Huawei handlers to ensure they are available for tests
            var registry = new HuaweiHandlerRegistry();
            // TODO: Register handlers with new command processor architecture
            // registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return GetCurrentModeEnum() switch
            {
                DeviceMode.User => $"<{GetHostname()}>",
                DeviceMode.Privileged => $"<{GetHostname()}>",
                DeviceMode.Config => $"[{GetHostname()}]",
                DeviceMode.Interface => $"[{GetHostname()}-{GetCurrentInterface()}]",
                DeviceMode.Vlan => $"[{GetHostname()}-vlan{_currentVlanId}]",
                DeviceMode.RouterOspf => $"[{GetHostname()}-ospf-{GetOspfConfiguration()?.ProcessId ?? 1}]",
                DeviceMode.RouterBgp => $"[{GetHostname()}-bgp]",
                DeviceMode.RouterRip => $"[{GetHostname()}-rip]",
                DeviceMode.Acl => $"[{GetHostname()}-acl-basic-{_currentAclNumber}]",
                _ => $"<{GetHostname()}>"
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

        // Additional methods needed by tests
        public Dictionary<string, RoutingPolicy> GetRoutePolicies()
        {
            return new Dictionary<string, RoutingPolicy>(); // Simplified implementation
        }

        public Dictionary<string, PrefixList> GetIpPrefixLists()
        {
            return new Dictionary<string, PrefixList>(); // Simplified implementation
        }

        // Helper methods for command handlers (similar to AristaDevice)
        public int GetCurrentVlanId() => _currentVlanId;
        public void SetCurrentVlanId(int vlanId) => _currentVlanId = vlanId;

        public int GetCurrentAclNumber() => _currentAclNumber;
        public void SetCurrentAclNumber(int aclNumber) => _currentAclNumber = aclNumber;

        public void AppendToRunningConfig(string line)
        {
            // TODO: Implement configuration building with new architecture
            // RunningConfig.AppendLine(line);
        }

        public void CreateOrSelectVlan(int vlanId)
        {
            if (!GetAllVlans().ContainsKey(vlanId))
            {
                AddVlan(vlanId, new VlanConfig(vlanId));
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

        // Public getters for compatibility
        public new List<VlanConfig> GetVlans() => GetAllVlans().Values.ToList();
        public OspfConfig GetOspfConfig() => GetOspfConfiguration() ?? new OspfConfig(1);
        public BgpConfig GetBgpConfig() => GetBgpConfiguration() ?? new BgpConfig(65000);
        public RipConfig GetRipConfig() => GetRipConfiguration() ?? new RipConfig();
        // GetRoutingTable is inherited from base class
        public List<IInterfaceConfig> GetInterfaces() => GetAllInterfaces().Values.ToList();

        public new string GetCurrentInterface() => base.GetCurrentInterface();
        public new void SetCurrentInterface(string iface) => base.SetCurrentInterface(iface);
        public string GetMode() => GetCurrentModeEnum().ToModeString();

        /// <summary>
        /// Get interface with alias expansion support
        /// </summary>
        /// <param name="interfaceName">Interface name or alias</param>
        /// <returns>Interface configuration or null if not found</returns>
        public override IInterfaceConfig? GetInterface(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return null;

            // Try direct lookup first
            if (GetAllInterfaces().TryGetValue(interfaceName, out var directMatch))
                return directMatch;

            // Try basic alias expansion - interface alias handling is now managed by the vendor registry system
            // For now, just do a case-insensitive search
            foreach (var kvp in GetAllInterfaces())
            {
                if (string.Equals(kvp.Key, interfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Add interface to the device
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="interfaceConfig">Interface configuration (optional)</param>
        public void AddInterface(string interfaceName, InterfaceConfig? interfaceConfig = null)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return;

            // Use canonical interface name for storage - simplified for now
            var canonicalName = interfaceName.ToLower();

            if (!GetAllInterfaces().ContainsKey(canonicalName))
            {
                AddInterface(canonicalName, interfaceConfig ?? new InterfaceConfig(canonicalName, this));
            }
        }
    }
}
