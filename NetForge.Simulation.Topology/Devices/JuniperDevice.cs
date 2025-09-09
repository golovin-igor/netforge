using System.Text;
using NetForge.Simulation.CliHandlers.Juniper;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Juniper Junos device implementation
    /// </summary>
    public sealed class JuniperDevice : NetworkDevice
    {
        public override string DeviceType => "Router";
        private List<string> _candidateConfig = new List<string>();
        private bool _inConfigMode = false;

        public JuniperDevice(string name) : base(name, "Juniper")
        {
            SetModeEnum(DeviceModeExtensions.FromModeString("operational")); // Start in operational mode
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for a Juniper router
            AddInterface("ge-0/0/0", new InterfaceConfig("ge-0/0/0", this));
            AddInterface("ge-0/0/1", new InterfaceConfig("ge-0/0/1", this));
            AddInterface("ge-0/0/2", new InterfaceConfig("ge-0/0/2", this));
            AddInterface("ge-0/0/3", new InterfaceConfig("ge-0/0/3", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Juniper handlers to ensure they are available for tests
            var registry = new JuniperHandlerRegistry();
            // TODO: Register handlers with new command processor architecture
            // registry.RegisterHandlers(CommandManager);
        }

        /// <summary>
        /// Get the current device mode
        /// </summary>
        public string GetMode() => GetCurrentModeEnum().ToModeString();

        /// <summary>
        /// Set the device mode
        /// </summary>
        public override void SetMode(string mode)
        {
            SetModeEnum(DeviceModeExtensions.FromModeString(mode));
            _inConfigMode = (mode == "configuration");
        }

        /// <summary>
        /// Add a line to the candidate configuration
        /// </summary>
        public void AddToCandidateConfig(string configLine)
        {
            _candidateConfig.Add(configLine);
        }

        /// <summary>
        /// Get the candidate configuration
        /// </summary>
        public List<string> GetCandidateConfig()
        {
            return new List<string>(_candidateConfig);
        }

        /// <summary>
        /// Clear the candidate configuration
        /// </summary>
        public void ClearCandidateConfig()
        {
            _candidateConfig.Clear();
        }

        /// <summary>
        /// Delete a configuration line from candidate config
        /// </summary>
        public void DeleteFromCandidateConfig(string configPath)
        {
            _candidateConfig.RemoveAll(c => c.Contains(configPath));
        }

        /// <summary>
        /// Commit the candidate configuration
        /// </summary>
        public void CommitCandidateConfig()
        {
            foreach (var configLine in _candidateConfig)
            {
                ApplyConfigurationLine(configLine);
            }
            _candidateConfig.Clear();
        }

        /// <summary>
        /// Commit the candidate configuration to running config
        /// </summary>
        public string CommitConfiguration()
        {
            try
            {
                // Apply candidate configuration
                foreach (var configLine in _candidateConfig)
                {
                    ApplyConfigurationLine(configLine);
                }

                _candidateConfig.Clear();
                return "commit complete\n";
            }
            catch (Exception ex)
            {
                return $"error: {ex.Message}\n";
            }
        }

        /// <summary>
        /// Apply a single configuration line
        /// </summary>
        private void ApplyConfigurationLine(string configLine)
        {
            var parts = configLine.Split(' ');
            if (parts.Length < 2) return;

            var command = parts[0].ToLower();

            switch (command)
            {
                case "set":
                    ProcessSetConfigLine(parts);
                    break;
                case "delete":
                    ProcessDeleteConfigLine(parts);
                    break;
            }
        }

        /// <summary>
        /// Process a set configuration line during commit
        /// </summary>
        private void ProcessSetConfigLine(string[] parts)
        {
            if (parts.Length < 3) return;

            switch (parts[1].ToLower())
            {
                case "system":
                    if (parts.Length > 3 && parts[2].ToLower() == "host-name")
                    {
                        SetHostname(parts[3]);
                    }
                    break;

                case "interfaces":
                    ProcessInterfaceConfig(parts);
                    break;

                case "vlans":
                    ProcessVlanConfig(parts);
                    break;

                case "protocols":
                    ProcessProtocolConfig(parts);
                    break;

                case "routing-options":
                    ProcessRoutingOptionsConfig(parts);
                    break;
            }
        }

        /// <summary>
        /// Process interface configuration during commit
        /// </summary>
        private void ProcessInterfaceConfig(string[] parts)
        {
            if (parts.Length < 3) return;

            var interfaceName = parts[2];

            // Ensure interface exists
            var iface = GetInterface(interfaceName);
            if (iface == null)
            {
                iface = new InterfaceConfig(interfaceName, this);
                AddInterface(interfaceName, iface);
            }

            if (parts.Length > 3 && parts[3].ToLower() == "description")
            {
                iface.Description = string.Join(" ", parts.Skip(4));
            }
            else if (parts.Length > 3 && parts[3].ToLower() == "disable")
            {
                iface.IsShutdown = true;
                iface.IsUp = false;
            }
            else if (parts.Length > 8 && parts[3].ToLower() == "unit" &&
                     parts[5].ToLower() == "family" && parts[6].ToLower() == "inet" &&
                     parts[7].ToLower() == "address")
            {
                var addressParts = parts[8].Split('/');
                if (addressParts.Length == 2)
                {
                    iface.IpAddress = addressParts[0];
                    iface.SubnetMask = CidrToMask(int.Parse(addressParts[1]));
                    ForceUpdateConnectedRoutes();
                }
            }
        }

        /// <summary>
        /// Process VLAN configuration during commit
        /// </summary>
        private void ProcessVlanConfig(string[] parts)
        {
            if (parts.Length < 5) return;

            var vlanName = parts[2];

            if (parts[3].ToLower() == "vlan-id" && int.TryParse(parts[4], out int vlanId))
            {
                var vlans = GetAllVlans().Values;
                var existingVlan = vlans.FirstOrDefault(v => v.Id == vlanId);
                if (existingVlan == null)
                {
                    AddVlan(vlanId, new VlanConfig(vlanId, vlanName));
                }
                else
                {
                    existingVlan.Name = vlanName;
                }
            }
        }

        /// <summary>
        /// Process protocol configuration during commit
        /// </summary>
        private void ProcessProtocolConfig(string[] parts)
        {
            if (parts.Length < 3) return;

            switch (parts[2].ToLower())
            {
                case "ospf":
                    ProcessOspfConfig(parts);
                    break;
                case "bgp":
                    ProcessBgpConfig(parts);
                    break;
                case "rip":
                    ProcessRipConfig(parts);
                    break;
            }
        }

        /// <summary>
        /// Process OSPF configuration during commit
        /// </summary>
        private void ProcessOspfConfig(string[] parts)
        {
            if (parts.Length < 7) return;

            if (GetOspfConfiguration() == null)
            {
                SetOspfConfiguration(new OspfConfig(1));
            }

            if (parts[3].ToLower() == "area" && parts[5].ToLower() == "interface")
            {
                var areaStr = parts[4].Replace("0.0.0.", "");
                if (int.TryParse(areaStr, out int area))
                {
                    var interfaceName = parts[6];
                    var iface = GetInterface(interfaceName);
                    if (iface != null)
                    {
                        var ospfConfig = GetOspfConfiguration();
                        ospfConfig.Interfaces[interfaceName] = new OspfInterface(interfaceName, area);

                        if (!string.IsNullOrEmpty(iface.IpAddress))
                        {
                            var network = GetNetwork(iface.IpAddress, iface.SubnetMask);
                            ospfConfig.NetworkAreas[network] = area;
                            if (!ospfConfig.Networks.Contains(network))
                            {
                                ospfConfig.Networks.Add(network);
                            }
                        }

                        GetParentNetwork()?.UpdateProtocols();
                    }
                }
            }
        }

        /// <summary>
        /// Process BGP configuration during commit
        /// </summary>
        private void ProcessBgpConfig(string[] parts)
        {
            if (parts.Length < 5) return;

            if (GetBgpConfiguration() == null)
            {
                SetBgpConfiguration(new BgpConfig(65000)); // Default AS
            }

            if (parts[3].ToLower() == "group" && parts.Length > 8)
            {
                if (parts[5].ToLower() == "neighbor" && parts[7].ToLower() == "peer-as")
                {
                    var neighborIp = parts[6];
                    if (int.TryParse(parts[8], out int peerAs))
                    {
                        var bgpConfig = GetBgpConfiguration();
                        if (!bgpConfig.Neighbors.ContainsKey(neighborIp))
                        {
                            var neighbor = new BgpNeighbor(neighborIp, peerAs);
                            bgpConfig.Neighbors[neighbor.IpAddress] = neighbor;
                        }
                        bgpConfig.Neighbors[neighborIp].RemoteAs = peerAs;
                        GetParentNetwork()?.UpdateProtocols();
                    }
                }
            }
            else if (parts[3].ToLower() == "local-as" && parts.Length > 4)
            {
                if (int.TryParse(parts[4], out int localAs))
                {
                    var bgpConfig = GetBgpConfiguration();
                    bgpConfig.LocalAs = localAs;
                }
            }
        }

        /// <summary>
        /// Process RIP configuration during commit
        /// </summary>
        private void ProcessRipConfig(string[] parts)
        {
            if (parts.Length < 6) return;

            if (GetRipConfiguration() == null)
            {
                SetRipConfiguration(new RipConfig());
            }

            if (parts[3].ToLower() == "group" && parts[5].ToLower() == "neighbor")
            {
                var interfaceName = parts[6];
                var iface = GetInterface(interfaceName);
                if (iface != null)
                {
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        var network = GetNetwork(iface.IpAddress, iface.SubnetMask);
                        var ripConfig = GetRipConfiguration();
                        ripConfig.Networks.Add(network);
                        GetParentNetwork()?.UpdateProtocols();
                    }
                }
            }
        }

        /// <summary>
        /// Process routing options configuration during commit
        /// </summary>
        private void ProcessRoutingOptionsConfig(string[] parts)
        {
            if (parts.Length < 7) return;

            if (parts[2].ToLower() == "static" && parts[3].ToLower() == "route" && parts[5].ToLower() == "next-hop")
            {
                var routeParts = parts[4].Split('/');
                if (routeParts.Length == 2)
                {
                    var network = routeParts[0];
                    var mask = CidrToMask(int.Parse(routeParts[1]));
                    var nextHop = parts[6];

                    var route = new Route(network, mask, nextHop, "", "Static");
                    route.Metric = 1;
                    AddStaticRoute(network, mask, nextHop, 1);
                }
            }
            else if (parts[2].ToLower() == "router-id" && parts.Length > 3)
            {
                var ospfConfig = GetOspfConfiguration();
                if (ospfConfig != null)
                {
                    ospfConfig.RouterId = parts[3];
                }
                var bgpConfig = GetBgpConfiguration();
                if (bgpConfig != null)
                {
                    bgpConfig.RouterId = parts[3];
                }
            }
        }

        /// <summary>
        /// Process delete configuration line during commit
        /// </summary>
        private void ProcessDeleteConfigLine(string[] parts)
        {
            // Implementation for delete commands during commit
            // For now, just placeholder
        }

        /// <summary>
        /// Convert CIDR to subnet mask
        /// </summary>
        private new string CidrToMask(int cidr)
        {
            uint mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        public override string GetPrompt()
        {
            var mode = GetCurrentModeEnum();
            var hostname = GetHostname();

            return mode switch
            {
                DeviceMode.Operational => $"{hostname}> ",
                DeviceMode.Configuration => $"{hostname}# ",
                DeviceMode.Interface => $"[edit interfaces {GetCurrentInterfaceName()}]\n{hostname}# ",
                _ => $"{hostname}> "
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

        // Helper methods for command handlers
        public new void SetCurrentMode(string mode)
        {
            SetModeEnum(DeviceModeExtensions.FromModeString(mode));
            _inConfigMode = (mode == "configuration");
        }

        public new string GetCurrentInterface() => GetCurrentInterfaceName();
        public new void SetCurrentInterface(string iface) => SetCurrentInterfaceName(iface);

        // Juniper-specific helper methods
        public bool IsInConfigMode() => _inConfigMode;
        public void EnterConfigMode()
        {
            _inConfigMode = true;
            _candidateConfig.Clear();
        }

        public void ExitConfigMode()
        {
            _inConfigMode = false;
            _candidateConfig.Clear();
        }

        public void UpdateProtocols()
        {
            GetParentNetwork()?.UpdateProtocols();
        }

        public void ClearInterfaceCounters(string? interfaceName = null)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                foreach (var iface in GetAllInterfaces().Values)
                {
                    iface.RxPackets = 0;
                    iface.TxPackets = 0;
                    iface.RxBytes = 0;
                    iface.TxBytes = 0;
                }
            }
            else 
            {
                var iface = GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.RxPackets = 0;
                    iface.TxPackets = 0;
                    iface.RxBytes = 0;
                    iface.TxBytes = 0;
                }
            }
        }

        // ProcessConfigModeCommand removed - now handled by command handlers

        // ProcessSetCommand removed - now handled by SetCommandHandler

        // ProcessSetInterface removed - now handled by SetCommandHandler

        // ProcessSetVlan removed - now handled by SetCommandHandler
        // All manual ProcessSetXXX methods removed - now handled by command handlers

        private string ShowCandidateConfig()
        {
            var output = new StringBuilder();
            foreach (var line in _candidateConfig.OrderBy(c => c))
            {
                output.AppendLine(line);
            }
            return output.ToString();
        }

        // ProcessShowCommand removed - now handled by ShowCommandHandler

        // Additional methods needed by tests
        public Dictionary<string, RoutingPolicy> GetRoutingPolicies()
        {
            return (Dictionary<string, RoutingPolicy>?)GetRoutingPolicyConfiguration() ?? new Dictionary<string, RoutingPolicy>();
        }

        public Dictionary<string, PrefixList> GetPrefixLists()
        {
            return (Dictionary<string, PrefixList>?)GetPrefixListConfiguration() ?? new Dictionary<string, PrefixList>();
        }

        public Dictionary<string, BgpCommunity> GetCommunities()
        {
            return (Dictionary<string, BgpCommunity>?)GetBgpCommunityConfiguration() ?? new Dictionary<string, BgpCommunity>();
        }

        public Dictionary<string, AsPathGroup> GetAsPathGroups()
        {
            return (Dictionary<string, AsPathGroup>?)GetAsPathGroupConfiguration() ?? new Dictionary<string, AsPathGroup>();
        }

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
            var interfaces = GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var directMatch))
                return directMatch;

            // Try basic alias expansion - interface alias handling is now managed by the vendor registry system
            // For now, just do a case-insensitive search
            foreach (var kvp in interfaces)
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

            if (GetInterface(canonicalName) == null)
            {
                AddInterface(canonicalName, interfaceConfig ?? new InterfaceConfig(canonicalName, this));
            }
        }

        private object GetRoutingPolicyConfiguration()
        {
            // Stub implementation for routing policy configuration
            return new { };
        }

        private object GetPrefixListConfiguration()
        {
            // Stub implementation for prefix list configuration  
            return new { };
        }

        private object GetBgpCommunityConfiguration()
        {
            // Stub implementation for BGP community configuration
            return new { };
        }

        private object GetAsPathGroupConfiguration()
        {
            // Stub implementation for AS path group configuration
            return new { };
        }
    }
}
