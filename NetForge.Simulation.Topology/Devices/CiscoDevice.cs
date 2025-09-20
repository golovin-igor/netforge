using System.Text;
using NetForge.Simulation.CliHandlers.Cisco;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Security;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Cisco IOS device implementation
    /// </summary>
    public sealed class CiscoDevice : NetworkDevice
    {
        public override string DeviceType => "Router";
        
        // Removed mode shadowing - using base class strongly typed currentMode
        private string _currentRouterProtocol = "";
        private int _currentVlanId = 0;
        private int _currentAclNumber = 0;
        private bool _cdpEnabled = true;
        private int _cdpTimer = 60;
        private int _cdpHoldtime = 180;

        public CiscoDevice(string name) : base(name, "Cisco")
        {
            // Add default VLAN 1
            AddVlan(1, new VlanConfig(1, "default"));
            InitializeDefaultInterfaces();

            // Register device-specific handlers (now handled by vendor registry)
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for a Cisco router
            AddInterface("GigabitEthernet0/0", new InterfaceConfig("GigabitEthernet0/0", this));
            AddInterface("GigabitEthernet0/1", new InterfaceConfig("GigabitEthernet0/1", this));
            AddInterface("GigabitEthernet0/2", new InterfaceConfig("GigabitEthernet0/2", this));
            AddInterface("GigabitEthernet0/3", new InterfaceConfig("GigabitEthernet0/3", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Cisco handlers to ensure they are available for tests
            var registry = new CiscoHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
            // Handlers are now registered through VendorSystemStartup
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
                DeviceMode.Router => $"{hostname}(config-router)#",
                DeviceMode.Vlan => $"{hostname}(config-vlan)#",
                DeviceMode.Acl => $"{hostname}(config-std-nacl)#",
                _ => $"{hostname}>"
            };
        }

        /// <summary>
        /// Override GetInterface to support interface aliases
        /// </summary>
        public override IInterfaceConfig? GetInterface(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Try direct lookup first
            var interfaces = GetAllInterfaces();
            if (interfaces.ContainsKey(name))
                return interfaces[name];

            // Try with expanded alias
            var canonicalName = CiscoInterfaceAliasHandler.ExpandInterfaceAlias(name);
            if (!string.Equals(name, canonicalName, StringComparison.OrdinalIgnoreCase) && interfaces.ContainsKey(canonicalName))
                return interfaces[canonicalName];

            return null;
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            var originalCommand = command;

            // Check for history recall commands first
            if (command.StartsWith("!"))
            {
                var history = GetCommandHistory();
                var recalledCommand = history.ProcessRecallCommand(command);
                if (!string.IsNullOrEmpty(recalledCommand))
                {
                    command = recalledCommand;
                }
                else
                {
                    history.AddCommand(originalCommand, GetCurrentModeEnum().ToModeString(), false);
                    return $"% No command found for '{command}'\n" + GetPrompt();
                }
            }

            // Process shortcuts and abbreviations
            var processedCommand = HistoryCommandProcessor.ProcessShortcuts(command, this);
            if (!string.IsNullOrEmpty(processedCommand))
            {
                command = processedCommand;
            }

            // Use the base class implementation for actual command processing
            return await base.ProcessCommandAsync(command);
        }

        private string CidrToMask(int cidr)
        {
            uint mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        private string WildcardToMask(string wildcard)
        {
            if (string.IsNullOrEmpty(wildcard)) return "255.255.255.255";
            var parts = wildcard.Split('.').Select(int.Parse).ToArray();
            var maskParts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                maskParts[i] = 255 - parts[i];
            }

            return string.Join(".", maskParts);
        }

        private int MaskToCidr(string mask)
        {
            if (string.IsNullOrEmpty(mask)) return 0;
            var parts = mask.Split('.').Select(int.Parse).ToArray();
            int cidr = 0;
            foreach (var part in parts)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if ((part & (1 << i)) != 0) cidr++;
                }
            }

            return cidr;
        }

        // Add helper methods for command handlers to access device state
        public string GetMode() => GetCurrentModeEnum().ToModeString();

        public void SetMode(string mode)
        {
            SetModeEnum(DeviceModeExtensions.FromModeString(mode));
            // Additional Cisco-specific mode logic here
        }

        /// <summary>
        /// Get current mode as strongly typed enum
        /// </summary>
        public DeviceMode GetModeEnum() => GetCurrentModeEnum();

        /// <summary>
        /// Set mode using strongly typed enum
        /// </summary>
        public new void SetModeEnum(DeviceMode mode)
        {
            base.SetModeEnum(mode);
            // Additional Cisco-specific mode logic here
        }

        public List<IInterfaceConfig> GetInterfaces() => GetAllInterfaces().Values.ToList();
        public OspfConfig GetOspfConfig() => GetOspfConfiguration();
        public BgpConfig GetBgpConfig() => GetBgpConfiguration();
        public RipConfig GetRipConfig() => GetRipConfiguration();

        public string ShowRunningConfig()
        {
            var output = new StringBuilder();

            // Use the new configuration builder
            var configBuilder = new Cisco.CiscoConfigurationBuilder(this, _configurationManager);
            var config = configBuilder.BuildRunningConfiguration();

            output.AppendLine("Building configuration...\n");
            output.AppendLine($"Current configuration : {config.Length} bytes");
            output.Append(config);

            return output.ToString();
        }

        public new List<VlanConfig> GetVlans() => GetAllVlans().Values.ToList();
        public new List<Route> GetRoutingTable() => base.GetRoutingTable();
        public int GetCurrentVlanId() => _currentVlanId;

        // Add interface management method
        public void AddNewInterface(string name)
        {
            if (GetInterface(name) == null)
            {
                AddInterface(name, new InterfaceConfig(name, this));
            }
        }

        public new void SetHostname(string name)
        {
            base.SetHostname(name);
            // Additional Cisco-specific hostname logic here
        }

        public void CreateOrSelectVlan(int vlanId)
        {
            var vlans = GetAllVlans().Values;
            if (!vlans.Any(v => v.Id == vlanId))
            {
                AddVlan(vlanId, new VlanConfig(vlanId));
            }

            _currentVlanId = vlanId; // Set the current VLAN ID for name commands
            // Configuration is now managed by DeviceConfigurationManager
        }

        public void SetCurrentVlanName(string name)
        {
            if (_currentVlanId > 0)
            {
                var vlan = GetAllVlans().Values.FirstOrDefault(v => v.Id == _currentVlanId);
                if (vlan != null)
                {
                    vlan.Name = name;
                    // Configuration is now managed by DeviceConfigurationManager
                    // GetRunningConfigBuilder().AppendLine($" name {name}");
                }
            }
        }

        public void InitializeOspf(int processId)
        {
            if (GetOspfConfiguration() == null)
            {
                SetOspfConfiguration(new OspfConfig(processId));
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($"router ospf {processId}");
        }

        public void InitializeBgp(int asNumber)
        {
            if (GetBgpConfiguration() == null)
            {
                SetBgpConfiguration(new BgpConfig(asNumber));
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($"router bgp {asNumber}");
        }

        public void InitializeRip()
        {
            if (GetRipConfiguration() == null)
            {
                SetRipConfiguration(new RipConfig());
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine("router rip");
        }

        public void InitializeEigrp(int asNumber)
        {
            if (GetEigrpConfiguration() == null)
            {
                SetEigrpConfiguration(new EigrpConfig(asNumber));
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($"router eigrp {asNumber}");
        }

        public void SetCurrentRouterProtocol(string protocol)
        {
            _currentRouterProtocol = protocol;
        }

        public void AppendToRunningConfig(string line)
        {
            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine(line);
        }

        public bool VlanExists(int vlanId)
        {
            return GetAllVlans().Values.Any(v => v.Id == vlanId);
        }

        public void AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            var vlan = GetAllVlans().Values.FirstOrDefault(v => v.Id == vlanId);
            if (vlan != null)
            {
                vlan.Interfaces.Add(interfaceName);
            }
        }


        // Router protocol helper methods
        public string GetCurrentRouterProtocol() => _currentRouterProtocol;

        public void AddOspfNetwork(string network, string wildcard, int area)
        {
            var ospfConfig = GetOspfConfiguration();
            if (ospfConfig == null) return;

            var mask = WildcardToMask(wildcard);
            ospfConfig.NetworkAreas[network] = area;

            // Store the full command format like the test expects
            var networkStr = $"{network} {wildcard} area {area}";
            if (!ospfConfig.Networks.Contains(networkStr))
            {
                ospfConfig.Networks.Add(networkStr);
            }

            // Find interfaces in this network
            foreach (var iface in GetAllInterfaces().Values)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    var ifaceNetwork = GetNetwork(iface.IpAddress, mask);
                    if (ifaceNetwork == network)
                    {
                        ospfConfig.Interfaces[iface.Name] = new OspfInterface(iface.Name, area);
                    }
                }
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($" network {network} {wildcard} area {area}");
            ParentNetwork?.UpdateProtocols();
        }

        public void SetOspfRouterId(string routerId)
        {
            var ospfConfig = GetOspfConfiguration();
            if (ospfConfig != null)
            {
                ospfConfig.RouterId = routerId;
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine($" router-id {routerId}");
            }
        }

        public void AddRipNetwork(string network)
        {
            var ripConfig = GetRipConfiguration();
            if (ripConfig != null)
            {
                ripConfig.Networks.Add(network);
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine($" network {network}");
                ParentNetwork?.UpdateProtocols();
            }
        }

        public void AddEigrpNetwork(string network, string wildcard)
        {
            var eigrpConfig = GetEigrpConfiguration();
            if (eigrpConfig != null)
            {
                var mask = WildcardToMask(wildcard);
                var networkStr = $"{network} {wildcard}";
                eigrpConfig.Networks.Add(networkStr);
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine($" network {network} {wildcard}");
                ParentNetwork?.UpdateProtocols();
            }
        }

        public void SetRipVersion(int version)
        {
            var ripConfig = GetRipConfiguration();
            if (ripConfig != null)
            {
                ripConfig.Version = version;
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine($" version {version}");
            }
        }

        public void SetRipAutoSummary(bool enabled)
        {
            var ripConfig = GetRipConfiguration();
            if (ripConfig != null)
            {
                ripConfig.AutoSummary = enabled;
                if (!enabled)
                {
                    // Configuration is now managed by DeviceConfigurationManager
                    // GetRunningConfigBuilder().AppendLine(" no auto-summary");
                }
            }
        }

        public void SetEigrpAutoSummary(bool enabled)
        {
            var eigrpConfig = GetEigrpConfiguration();
            if (eigrpConfig != null)
            {
                eigrpConfig.AutoSummary = enabled;
                if (!enabled)
                {
                    // Configuration is now managed by DeviceConfigurationManager
                    // GetRunningConfigBuilder().AppendLine(" no auto-summary");
                }
            }
        }

        public void AddBgpNetwork(string network, string mask)
        {
            var bgpConfig = GetBgpConfiguration();
            if (bgpConfig != null)
            {
                var cidr = mask != null ? MaskToCidr(mask) : 0;
                var networkStr = mask != null ? $"{network}/{cidr}" : network;
                bgpConfig.Networks.Add(networkStr);

                if (mask != null)
                {
                    // Configuration is now managed by DeviceConfigurationManager
                    // GetRunningConfigBuilder().AppendLine($" network {network} mask {mask}");
                }
                else
                {
                    // Configuration is now managed by DeviceConfigurationManager
                    // GetRunningConfigBuilder().AppendLine($" network {network}");
                }

                ParentNetwork?.UpdateProtocols();
            }
        }

        public void AddBgpNeighbor(string neighborIp, int remoteAs)
        {
            var bgpConfig = GetBgpConfiguration();
            if (bgpConfig == null) return;

            if (!bgpConfig.Neighbors.ContainsKey(neighborIp))
            {
                var neighbor = new BgpNeighbor(neighborIp, remoteAs);
                bgpConfig.Neighbors[neighborIp] = neighbor;
            }
            else
            {
                bgpConfig.Neighbors[neighborIp].RemoteAs = remoteAs;
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($" neighbor {neighborIp} remote-as {remoteAs}");
            ParentNetwork?.UpdateProtocols();
        }

        public void SetBgpRouterId(string routerId)
        {
            var bgpConfig = GetBgpConfiguration();
            if (bgpConfig != null)
            {
                bgpConfig.RouterId = routerId;
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine($" bgp router-id {routerId}");
            }
        }

        public void SetBgpNeighborDescription(string neighborIp, string description)
        {
            var bgpConfig = GetBgpConfiguration();
            if (bgpConfig == null) return;

            if (bgpConfig.Neighbors.ContainsKey(neighborIp))
            {
                bgpConfig.Neighbors[neighborIp].Description = description;
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine($" neighbor {neighborIp} description {description}");
            }
        }

        public void ShutdownBgpNeighbor(string neighborIp)
        {
            if (GetBgpConfiguration() == null) return;

            if (GetBgpConfiguration()?.Neighbors.ContainsKey(neighborIp) == true)
            {
                GetBgpConfiguration().Neighbors[neighborIp].State = "Idle (Admin)";
                // Configuration is now managed by DeviceConfigurationManager
                // RunningConfig.AppendLine($" neighbor {neighborIp} shutdown");
            }
        }

        public void ActivateBgpNeighbor(string neighborIp)
        {
            if (GetBgpConfiguration() == null) return;

            if (GetBgpConfiguration()?.Neighbors.ContainsKey(neighborIp) == true)
            {
                GetBgpConfiguration().Neighbors[neighborIp].State = "Established";
                // Configuration is now managed by DeviceConfigurationManager
                // RunningConfig.AppendLine($" neighbor {neighborIp} activate");
            }
        }

        // ACL helper methods
        public void SetCurrentAclNumber(int aclNumber)
        {
            _currentAclNumber = aclNumber;
            var acls = GetAccessLists();
            if (!acls.ContainsKey(aclNumber))
            {
                acls[aclNumber] = new AccessList(aclNumber);
            }
        }

        public int GetCurrentAclNumber() => _currentAclNumber;

        public void AddAclEntry(int aclNumber, AclEntry entry)
        {
            var acls = GetAccessLists();
            if (!acls.ContainsKey(aclNumber))
            {
                acls[aclNumber] = new AccessList(aclNumber);
            }

            acls[aclNumber].Entries.Add(entry);

            var cmd = new StringBuilder($"access-list {aclNumber} {entry.Action}");
            if (entry.Protocol != "ip" && aclNumber >= 100)
            {
                cmd.Append($" {entry.Protocol}");
            }

            if (entry.SourceAddress == "any")
            {
                cmd.Append(" any");
            }
            else if (entry.SourceWildcard == "0.0.0.0")
            {
                cmd.Append($" host {entry.SourceAddress}");
            }
            else
            {
                cmd.Append($" {entry.SourceAddress} {entry.SourceWildcard}");
            }

            if (aclNumber >= 100 && aclNumber <= 199)
            {
                if (entry.DestAddress == "any")
                {
                    cmd.Append(" any");
                }
                else if (entry.DestWildcard == "0.0.0.0")
                {
                    cmd.Append($" host {entry.DestAddress}");
                }
                else
                {
                    cmd.Append($" {entry.DestAddress} {entry.DestWildcard}");
                }
            }

            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine(cmd.ToString());
        }

        // STP helper methods
        public void SetStpMode(string mode)
        {
            var stpConfig = GetStpConfiguration();
            stpConfig.Mode = mode;
            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($"spanning-tree mode {mode}");
        }

        public void SetStpVlanPriority(int vlanId, int priority)
        {
            var stpConfig = GetStpConfiguration();
            stpConfig.VlanPriorities[vlanId] = priority;
            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($"spanning-tree vlan {vlanId} priority {priority}");
            ParentNetwork?.UpdateProtocols();
        }

        public void SetStpPriority(int priority)
        {
            var stpConfig = GetStpConfiguration();
            stpConfig.DefaultPriority = priority;
            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine($"spanning-tree priority {priority}");
            ParentNetwork?.UpdateProtocols();
        }

        public void EnablePortfast(string interfaceName)
        {
            if (GetInterface(interfaceName) != null)
            {
                // Store portfast setting in running config
                // Configuration is now managed by DeviceConfigurationManager
                // GetRunningConfigBuilder().AppendLine(" spanning-tree portfast");
            }
        }

        public void EnablePortfastDefault()
        {
            // Store portfast default setting in running config
            // Configuration is now managed by DeviceConfigurationManager
            // GetRunningConfigBuilder().AppendLine("spanning-tree portfast default");
        }

        public void EnableBpduGuard(string interfaceName)
        {
            if (GetAllInterfaces().ContainsKey(interfaceName))
            {
                // Store bpduguard setting in running config
                // Configuration is now managed by DeviceConfigurationManager
                // RunningConfig.AppendLine(" spanning-tree bpduguard enable");
            }
        }

        public void EnableBpduGuardDefault()
        {
            // Store bpduguard default setting in running config
            // Configuration is now managed by DeviceConfigurationManager
            // RunningConfig.AppendLine("spanning-tree portfast bpduguard default");
        }

        // CDP helper methods
        public void EnableCdpGlobal()
        {
            _cdpEnabled = true;
            // Configuration is now managed by DeviceConfigurationManager
            // RunningConfig.AppendLine("cdp run");
        }

        public void DisableCdpGlobal()
        {
            _cdpEnabled = false;
            // Configuration is now managed by DeviceConfigurationManager
            // RunningConfig.AppendLine("no cdp run");
        }

        public void EnableCdpInterface(string interfaceName)
        {
            if (GetAllInterfaces().ContainsKey(interfaceName))
            {
                // Configuration is now managed by DeviceConfigurationManager
                // RunningConfig.AppendLine(" cdp enable");
            }
        }

        public void DisableCdpInterface(string interfaceName)
        {
            if (GetAllInterfaces().ContainsKey(interfaceName))
            {
                // Configuration is now managed by DeviceConfigurationManager
                // RunningConfig.AppendLine(" no cdp enable");
            }
        }

        public void SetCdpTimer(int seconds)
        {
            _cdpTimer = seconds;
            // Configuration is now managed by DeviceConfigurationManager
            // RunningConfig.AppendLine($"cdp timer {seconds}");
        }

        public void SetCdpHoldtime(int seconds)
        {
            _cdpHoldtime = seconds;
            // Configuration is now managed by DeviceConfigurationManager
            // RunningConfig.AppendLine($"cdp holdtime {seconds}");
        }

        public string ShowCdpStatus()
        {
            var output = new StringBuilder();
            output.AppendLine($"Global CDP information:");
            output.AppendLine($"        Sending CDP packets every {_cdpTimer} seconds");
            output.AppendLine($"        Sending a holdtime value of {_cdpHoldtime} seconds");
            output.AppendLine($"        Sending CDPv2 advertisements is enabled");
            return output.ToString();
        }

        public string ShowCdpNeighbors()
        {
            var output = new StringBuilder();
            output.AppendLine("Capability Codes: R - Router, T - Trans Bridge, B - Source Route Bridge");
            output.AppendLine("                  S - Switch, H - Host, I - IGMP, r - Repeater, P - Phone,");
            output.AppendLine("                  D - Remote, C - CVTA, M - Two-port Mac Relay");
            output.AppendLine();
            output.AppendLine("Device ID        Local Intrfce     Holdtme    Capability  Platform  Port ID");

            // In a real implementation, this would show actual neighbors
            // For now, return empty neighbor list

            output.AppendLine();
            output.AppendLine($"Total cdp entries displayed : 0");
            return output.ToString();
        }

        public string ShowCdpNeighborsDetail()
        {
            // In a real implementation, this would show detailed neighbor info
            return "Total cdp entries displayed : 0\n";
        }

        public string ShowCdpInterface()
        {
            var output = new StringBuilder();
            foreach (var iface in GetAllInterfaces().Values)
            {
                output.AppendLine($"{iface.Name} is {iface.GetStatus()}, line protocol is {(iface.IsUp ? "up" : "down")}");
                output.AppendLine($"  Encapsulation ARPA");
                output.AppendLine($"  Sending CDP packets every {_cdpTimer} seconds");
                output.AppendLine($"  Holdtime is {_cdpHoldtime} seconds");
                output.AppendLine();
            }

            return output.ToString();
        }

        public string ShowCdpTraffic()
        {
            var output = new StringBuilder();
            output.AppendLine("CDP counters :");
            output.AppendLine("        Total packets output: 0, Input: 0");
            output.AppendLine("        Hdr syntax: 0, Chksum error: 0, Encaps failed: 0");
            output.AppendLine("        No memory: 0, Invalid packet: 0,");
            output.AppendLine("        CDP version 1 advertisements output: 0, Input: 0");
            output.AppendLine("        CDP version 2 advertisements output: 0, Input: 0");
            return output.ToString();
        }

        // Clear command helper methods
        public void ClearRoutingTable()
        {
            ClearAllRoutes();
        }

        public void ClearSpecificRoute(string route)
        {
            GetRoutingTable().RemoveAll(r => r.Network == route && r.Protocol == "Static");
        }

        public void ClearAllRoutes()
        {
            // Clear only non-connected routes
            GetRoutingTable().RemoveAll(r => r.Protocol != "Connected");
            ForceUpdateConnectedRoutes();
        }

        public void ClearOspfProcess()
        {
            if (GetOspfConfiguration() != null)
            {
                GetOspfConfiguration()?.Neighbors.Clear();
                // OSPF will reconverge
            }
        }

        public void ClearBgpPeer(string peerIp)
        {
            if (GetBgpConfiguration() != null)
            {
                if (GetBgpConfiguration().Neighbors.ContainsKey(peerIp))
                {
                    GetBgpConfiguration().Neighbors[peerIp].State = "Idle";
                    // BGP will attempt to reconnect
                }
            }
        }

        public void ClearAllBgpPeers()
        {
            if (GetBgpConfiguration() != null)
            {
                foreach (var peer in GetBgpConfiguration()?.Neighbors.Values ?? new Dictionary<string, BgpNeighbor>().Values)
                {
                    peer.State = "Idle";
                }
            }
        }

        public void ClearInterfaceCounters(string interfaceName)
        {
            if (GetAllInterfaces().ContainsKey(interfaceName))
            {
                var iface = GetAllInterfaces()[interfaceName];
                iface.RxPackets = 0;
                iface.TxPackets = 0;
                iface.RxBytes = 0;
                iface.TxBytes = 0;
            }
        }

        public void ClearAllCounters()
        {
            foreach (var iface in GetAllInterfaces().Values)
            {
                iface.RxPackets = 0;
                iface.TxPackets = 0;
                iface.RxBytes = 0;
                iface.TxBytes = 0;
            }
        }

        public void ClearLogging()
        {
            // Clear log buffer
        }

        // Additional methods needed by tests
        // GetEigrpConfiguration is inherited from base class

        // GetOspfConfiguration is inherited from base class

        // GetBgpConfiguration is inherited from base class

        // GetRipConfiguration is inherited from base class

        public Dictionary<string, RouteMap> GetRouteMaps()
        {
            return new Dictionary<string, RouteMap>();
        }

        public Dictionary<string, PrefixList> GetPrefixLists()
        {
            return new Dictionary<string, PrefixList>();
        }

        /// <summary>
        /// Provide Cisco-style ping output
        /// </summary>
        private string SimulatePing(string destination)
        {
            if (ParentNetwork == null)
                return "% Network not initialized";

            var destDevice = ParentNetwork.FindDeviceByIp(destination);
            if (destDevice == null)
                return "% Destination host unreachable";

            // Find outgoing interface for the destination
            string outgoingInterface = FindOutgoingInterface(destination);
            if (string.IsNullOrEmpty(outgoingInterface))
                return "% No route to destination";

            // Check if the outgoing interface is shutdown
            var outgoingInterfaceConfig = GetInterface(outgoingInterface);
            if (outgoingInterfaceConfig == null || outgoingInterfaceConfig.IsShutdown)
                return BuildCiscoPingFailureOutput(destination, "Request timeout");

            // Check if the outgoing interface has physical connectivity
            if (!IsInterfacePhysicallyConnected(outgoingInterface))
                return "% Interface physically disconnected";

            // Check for ACL blocking ICMP traffic on destination device
            var destInterface = destDevice.GetAllInterfaces().Values.FirstOrDefault(i => i.IpAddress == destination);
            if (destInterface != null && IsDestinationIcmpBlocked(destDevice, destInterface.Name, destination))
                return BuildCiscoPingAclBlockedOutput(destination);

            // Check destination interface status
            if (destInterface == null || destInterface.IsShutdown)
                return BuildCiscoPingFailureOutput(destination, "Request timeout");

            // Get physical connection metrics for realistic ping simulation
            var metrics = GetPhysicalConnectionMetrics(outgoingInterface);
            if (metrics == null)
                return "% Physical connection unavailable";

            var sb = new StringBuilder();
            sb.AppendLine("Type escape sequence to abort.");
            sb.AppendLine($"Sending 5, 100-byte ICMP Echos to {destination}, timeout is 2 seconds:");

            // Simulate pings with realistic results
            int successCount = 0;
            var resultPattern = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                var result = TestPhysicalConnectivity(outgoingInterface, 64); // Standard ping packet size
                if (result.Success)
                {
                    successCount++;
                    resultPattern.Append("!");

                    // Increment interface counters for successful pings
                    if (outgoingInterfaceConfig != null)
                    {
                        outgoingInterfaceConfig.TxPackets++;
                        outgoingInterfaceConfig.TxBytes += 64; // Standard ping packet size
                    }

                    // Increment RX counters on destination device
                    if (destInterface != null)
                    {
                        destInterface.RxPackets++;
                        destInterface.RxBytes += 64;
                    }
                }
                else
                {
                    resultPattern.Append(".");
                }
            }

            sb.AppendLine(resultPattern.ToString());

            if (successCount == 5)
            {
                sb.AppendLine($"Success rate is 100 percent (5/5), round-trip min/avg/max = 1/1/4 ms");
            }
            else if (successCount == 0)
            {
                sb.AppendLine($"Success rate is 0 percent (0/5)");
            }
            else
            {
                double successRate = (successCount / 5.0) * 100;
                sb.AppendLine($"Success rate is {successRate:F0} percent ({successCount}/5), round-trip min/avg/max = 1/1/4 ms");
            }

            return sb.ToString();
        }

        private string BuildCiscoPingFailureOutput(string destination, string reason)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Type escape sequence to abort.");
            sb.AppendLine($"Sending 5, 100-byte ICMP Echos to {destination}, timeout is 2 seconds:");
            sb.AppendLine(".....");
            sb.AppendLine("Success rate is 0 percent (0/5)");
            return sb.ToString();
        }

        private string BuildCiscoPingAclBlockedOutput(string destination)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Type escape sequence to abort.");
            sb.AppendLine($"Sending 5, 100-byte ICMP Echos to {destination}, timeout is 2 seconds:");
            sb.AppendLine("U.U.U");
            sb.AppendLine("Success rate is 0 percent (0/5)");
            return sb.ToString();
        }

        private string FindOutgoingInterface(string destination)
        {
            // Simple stub implementation - returns first available interface
            var interfaces = GetAllInterfaces();
            return interfaces.Values.FirstOrDefault()?.Name ?? "GigabitEthernet0/0";
        }

        private bool IsDestinationIcmpBlocked(object destDevice, string interfaceName, string destination)
        {
            // Simple stub implementation - assume ICMP is allowed
            return false;
        }
    }
}
