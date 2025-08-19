using System.Text;
using NetForge.Simulation.CliHandlers.Cisco;
using NetForge.Simulation.CliHandlers.Services;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Configuration;
using NetForge.Simulation.Core;
using DiscoveryCdpNeighbor = NetForge.Simulation.Protocols.Discovery.CdpNeighbor;
using NetForge.Simulation.Protocols.Discovery;
using NetForge.Simulation.Protocols.Implementations;
using NetForge.Simulation.Protocols.Routing;
using NetForge.Simulation.Protocols.Security;
using PortChannelConfig = NetForge.Simulation.Configuration.PortChannel;

namespace NetForge.Simulation.Devices
{
    /// <summary>
    /// Cisco IOS device implementation
    /// </summary>
    public class CiscoDevice : NetworkDevice
    {
        // Removed mode shadowing - using base class strongly typed currentMode
        private string currentRouterProtocol = "";
        private int currentVlanId = 0;
        private int currentAclNumber = 0;
        private bool cdpEnabled = true;
        private Dictionary<string, DiscoveryCdpNeighbor> cdpNeighbors = new();
        private int cdpTimer = 60;
        private int cdpHoldtime = 180;
        
        public CiscoDevice(string name) : base(name)
        {
            Vendor = "Cisco";
            
            // Add default VLAN 1
            Vlans[1] = new VlanConfig(1, "default");
            InitializeDefaultInterfaces();
            
            // Register device-specific handlers (now handled by vendor registry)
            RegisterDeviceSpecificHandlers();

            // Register common protocols for Cisco devices
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new RipProtocol());
            RegisterProtocol(new EigrpProtocol());
            RegisterProtocol(new IgrpProtocol());
            RegisterProtocol(new StpProtocol());
            RegisterProtocol(new CdpProtocol());
            RegisterProtocol(new IsisProtocol());
            RegisterProtocol(new LldpProtocol());
            RegisterProtocol(new HsrpProtocol());
            RegisterProtocol(new ArpProtocol());
            // Note: Actual Cisco devices might not run all these by default.
            // This is for comprehensive registration based on available protocol types.
        }
        
        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for a Cisco router
            Interfaces["GigabitEthernet0/0"] = new InterfaceConfig("GigabitEthernet0/0", this);
            Interfaces["GigabitEthernet0/1"] = new InterfaceConfig("GigabitEthernet0/1", this);
            Interfaces["GigabitEthernet0/2"] = new InterfaceConfig("GigabitEthernet0/2", this);
            Interfaces["GigabitEthernet0/3"] = new InterfaceConfig("GigabitEthernet0/3", this);
        }
        
        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Cisco handlers to ensure they are available for tests
            var registry = new NetForge.Simulation.CliHandlers.Cisco.CiscoHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
            registry.RegisterHandlers(CommandManager);
        }
        
        public override string GetPrompt()
        {
            return base.CurrentMode switch
            {
                DeviceMode.User => $"{Hostname}>",
                DeviceMode.Privileged => $"{Hostname}#",
                DeviceMode.Config => $"{Hostname}(config)#",
                DeviceMode.Interface => $"{Hostname}(config-if)#",
                DeviceMode.Router => $"{Hostname}(config-router)#",
                DeviceMode.Vlan => $"{Hostname}(config-vlan)#",
                DeviceMode.Acl => $"{Hostname}(config-std-nacl)#",
                _ => $"{Hostname}>"
            };
        }
        
        /// <summary>
        /// Override GetInterface to support interface aliases
        /// </summary>
        public override InterfaceConfig? GetInterface(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Try direct lookup first
            if (Interfaces.ContainsKey(name))
                return Interfaces[name];

            // Try with expanded alias
            var canonicalName = CiscoInterfaceAliasHandler.ExpandInterfaceAlias(name);
            if (!string.Equals(name, canonicalName, StringComparison.OrdinalIgnoreCase) && Interfaces.ContainsKey(canonicalName))
                return Interfaces[canonicalName];

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
                var recalledCommand = CommandHistory.ProcessRecallCommand(command);
                if (!string.IsNullOrEmpty(recalledCommand))
                {
                    command = recalledCommand;
                }
                else
                {
                    CommandHistory.AddCommand(originalCommand, base.CurrentMode.ToModeString(), false);
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
        
        protected string CidrToMask(int cidr)
        {
            uint mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }
        
        protected string WildcardToMask(string wildcard)
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
        
        protected int MaskToCidr(string mask)
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
        
        // override UpdateCdp changed to new, then new removed as it's not hiding
        public void UpdateCdp()
        {
            if (!cdpEnabled || ParentNetwork == null) return;
            // Clear neighbors each cycle, rebuild
            cdpNeighbors.Clear();
            foreach (var iface in Interfaces.Values)
            {
                if (iface.IsShutdown) continue;
                var connected = ParentNetwork.GetConnectedDevices(Name, iface.Name);
                foreach (var (device, intf) in connected)
                {
                    if (device is CiscoDevice remote && remote.cdpEnabled)
                    {
                        var remoteIface = remote.Interfaces[intf];
                        var neighbor = new DiscoveryCdpNeighbor(device.Name, iface.Name, intf, device.Vendor, remoteIface.IpAddress);
                        cdpNeighbors[iface.Name] = neighbor;
                    }
                }
            }
        }

        private string BuildCdpNeighborsOutput()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Capability Codes: R - Router, S - Switch, H - Host");
            sb.AppendLine();
            sb.AppendLine("Device ID        Local Intrfce     Holdtme    Capability  Platform  Port ID");
            foreach (var n in cdpNeighbors.Values)
            {
                sb.AppendLine($"{n.DeviceId,-17} {n.LocalInterface,-15} {n.HoldTime,4}       S            {n.Platform,-8} {n.PortId}");
            }
            return sb.ToString();
        }

        // Add helper methods for command handlers to access device state
        public string GetMode() => base.CurrentMode.ToModeString();
        public void SetMode(string mode)
        {
            base.SetMode(mode);
            // Additional Cisco-specific mode logic here
        }
        
        /// <summary>
        /// Get current mode as strongly typed enum
        /// </summary>
        public DeviceMode GetModeEnum() => base.CurrentMode;
        
        /// <summary>
        /// Set mode using strongly typed enum
        /// </summary>
        public void SetModeEnum(DeviceMode mode)
        {
            base.SetModeEnum(mode);
            // Additional Cisco-specific mode logic here
        }

        public List<InterfaceConfig> GetInterfaces() => Interfaces.Values.ToList();
        public OspfConfig GetOspfConfig() => OspfConfig;
        public BgpConfig GetBgpConfig() => BgpConfig;
        public RipConfig GetRipConfig() => RipConfig;
        
        public string ShowRunningConfig()
        {
            var output = new StringBuilder();
            var config = RunningConfig.Build();
            output.AppendLine("Building configuration...\n");
            output.AppendLine("Current configuration : " + config + " bytes");
            output.AppendLine("!");
            output.AppendLine($"hostname {Hostname}");
            output.AppendLine("!");
            output.Append(config);
            output.AppendLine("!");
            output.AppendLine("end");
            return output.ToString();
        }
        public List<VlanConfig> GetVlans() => Vlans.Values.ToList();
        public new List<Route> GetRoutingTable() => RoutingTable;
        public int GetCurrentVlanId() => currentVlanId;
        
        // Add interface management method
        public void AddInterface(string name)
        {
            if (!Interfaces.ContainsKey(name))
            {
                Interfaces[name] = new InterfaceConfig(name, this);
            }
        }
        
        public void SetHostname(string name)
        {
            base.SetHostname(name);
            // Additional Cisco-specific hostname logic here
        }
        
        public void CreateOrSelectVlan(int vlanId)
        {
            if (!Vlans.ContainsKey(vlanId))
            {
                Vlans[vlanId] = new VlanConfig(vlanId);
            }
            currentVlanId = vlanId; // Set the current VLAN ID for name commands
            RunningConfig.AppendLine($"vlan {vlanId}");
        }
        
        public void SetCurrentVlanName(string name)
        {
            if (currentVlanId > 0 && Vlans.ContainsKey(currentVlanId))
            {
                Vlans[currentVlanId].Name = name;
                RunningConfig.AppendLine($" name {name}");
            }
        }
        
        public void InitializeOspf(int processId)
        {
            if (OspfConfig == null)
            {
                OspfConfig = new OspfConfig(processId);
            }
            RunningConfig.AppendLine($"router ospf {processId}");
        }
        
        public void InitializeBgp(int asNumber)
        {
            if (BgpConfig == null)
            {
                BgpConfig = new BgpConfig(asNumber);
            }
            RunningConfig.AppendLine($"router bgp {asNumber}");
        }
        
        public void InitializeRip()
        {
            if (RipConfig == null)
            {
                RipConfig = new RipConfig();
            }
            RunningConfig.AppendLine("router rip");
        }
        
        public void InitializeEigrp(int asNumber)
        {
            if (EigrpConfig == null)
            {
                EigrpConfig = new EigrpConfig(asNumber);
            }
            RunningConfig.AppendLine($"router eigrp {asNumber}");
        }
        
        public void SetCurrentRouterProtocol(string protocol)
        {
            currentRouterProtocol = protocol;
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
            // Additional Cisco-specific route logic here
        }
        
        // Router protocol helper methods
        public string GetCurrentRouterProtocol() => currentRouterProtocol;
        
        public void AddOspfNetwork(string network, string wildcard, int area)
        {
            if (OspfConfig == null) return;
            
            var mask = WildcardToMask(wildcard);
            OspfConfig.NetworkAreas[network] = area;
            
            // Store the full command format like the test expects
            var networkStr = $"{network} {wildcard} area {area}";
            if (!OspfConfig.Networks.Contains(networkStr))
            {
                OspfConfig.Networks.Add(networkStr);
            }
            
            // Find interfaces in this network
            foreach (var iface in Interfaces.Values)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    var ifaceNetwork = GetNetwork(iface.IpAddress, mask);
                    if (ifaceNetwork == network)
                    {
                        OspfConfig.Interfaces[iface.Name] = new OspfInterface(iface.Name, area);
                    }
                }
            }
            
            RunningConfig.AppendLine($" network {network} {wildcard} area {area}");
            ParentNetwork?.UpdateProtocols();
        }
        
        public void SetOspfRouterId(string routerId)
        {
            if (OspfConfig != null)
            {
                OspfConfig.RouterId = routerId;
                RunningConfig.AppendLine($" router-id {routerId}");
            }
        }
        
        public void AddRipNetwork(string network)
        {
            if (RipConfig != null)
            {
                RipConfig.Networks.Add(network);
                RunningConfig.AppendLine($" network {network}");
                ParentNetwork?.UpdateProtocols();
            }
        }
        
        public void AddEigrpNetwork(string network, string wildcard)
        {
            if (EigrpConfig != null)
            {
                var mask = WildcardToMask(wildcard);
                var networkStr = $"{network} {wildcard}";
                EigrpConfig.Networks.Add(networkStr);
                RunningConfig.AppendLine($" network {network} {wildcard}");
                ParentNetwork?.UpdateProtocols();
            }
        }
        
        public void SetRipVersion(int version)
        {
            if (RipConfig != null)
            {
                RipConfig.Version = version;
                RunningConfig.AppendLine($" version {version}");
            }
        }
        
        public void SetRipAutoSummary(bool enabled)
        {
            if (RipConfig != null)
            {
                RipConfig.AutoSummary = enabled;
                if (!enabled)
                {
                    RunningConfig.AppendLine(" no auto-summary");
                }
            }
        }
        
        public void SetEigrpAutoSummary(bool enabled)
        {
            if (EigrpConfig != null)
            {
                EigrpConfig.AutoSummary = enabled;
                if (!enabled)
                {
                    RunningConfig.AppendLine(" no auto-summary");
                }
            }
        }
        
        public void AddBgpNetwork(string network, string mask)
        {
            if (BgpConfig != null)
            {
                var cidr = mask != null ? MaskToCidr(mask) : 0;
                var networkStr = mask != null ? $"{network}/{cidr}" : network;
                BgpConfig.Networks.Add(networkStr);
                
                if (mask != null)
                {
                    RunningConfig.AppendLine($" network {network} mask {mask}");
                }
                else
                {
                    RunningConfig.AppendLine($" network {network}");
                }
                ParentNetwork?.UpdateProtocols();
            }
        }
        
        public void AddBgpNeighbor(string neighborIp, int remoteAs)
        {
            if (BgpConfig == null) return;
            
            if (!BgpConfig.Neighbors.ContainsKey(neighborIp))
            {
                var neighbor = new BgpNeighbor(neighborIp, remoteAs);
                BgpConfig.Neighbors[neighborIp] = neighbor;
            }
            else
            {
                BgpConfig.Neighbors[neighborIp].RemoteAs = remoteAs;
            }
            RunningConfig.AppendLine($" neighbor {neighborIp} remote-as {remoteAs}");
            ParentNetwork?.UpdateProtocols();
        }
        
        public void SetBgpRouterId(string routerId)
        {
            if (BgpConfig != null)
            {
                BgpConfig.RouterId = routerId;
                RunningConfig.AppendLine($" bgp router-id {routerId}");
            }
        }
        
        public void SetBgpNeighborDescription(string neighborIp, string description)
        {
            if (BgpConfig == null) return;
            
            if (BgpConfig.Neighbors.ContainsKey(neighborIp))
            {
                BgpConfig.Neighbors[neighborIp].Description = description;
                RunningConfig.AppendLine($" neighbor {neighborIp} description {description}");
            }
        }
        
        public void ShutdownBgpNeighbor(string neighborIp)
        {
            if (BgpConfig == null) return;
            
            if (BgpConfig.Neighbors.ContainsKey(neighborIp))
            {
                BgpConfig.Neighbors[neighborIp].State = "Idle (Admin)";
                RunningConfig.AppendLine($" neighbor {neighborIp} shutdown");
            }
        }
        
        public void ActivateBgpNeighbor(string neighborIp)
        {
            if (BgpConfig == null) return;
            
            if (BgpConfig.Neighbors.ContainsKey(neighborIp))
            {
                BgpConfig.Neighbors[neighborIp].State = "Established";
                RunningConfig.AppendLine($" neighbor {neighborIp} activate");
            }
        }
        
        // ACL helper methods
        public void SetCurrentAclNumber(int aclNumber)
        {
            currentAclNumber = aclNumber;
            if (!AccessLists.ContainsKey(aclNumber))
            {
                AccessLists[aclNumber] = new AccessList(aclNumber);
            }
        }
        
        public int GetCurrentAclNumber() => currentAclNumber;
        
        public void AddAclEntry(int aclNumber, AclEntry entry)
        {
            if (!AccessLists.ContainsKey(aclNumber))
            {
                AccessLists[aclNumber] = new AccessList(aclNumber);
            }
            
            AccessLists[aclNumber].Entries.Add(entry);
            
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
            
            RunningConfig.AppendLine(cmd.ToString());
        }
        
        // STP helper methods
        public void SetStpMode(string mode)
        {
            StpConfig.Mode = mode;
            RunningConfig.AppendLine($"spanning-tree mode {mode}");
        }
        
        public void SetStpVlanPriority(int vlanId, int priority)
        {
            StpConfig.VlanPriorities[vlanId] = priority;
            RunningConfig.AppendLine($"spanning-tree vlan {vlanId} priority {priority}");
            ParentNetwork?.UpdateProtocols();
        }
        
        public void SetStpPriority(int priority)
        {
            StpConfig.DefaultPriority = priority;
            RunningConfig.AppendLine($"spanning-tree priority {priority}");
            ParentNetwork?.UpdateProtocols();
        }
        
        public void EnablePortfast(string interfaceName)
        {
            if (Interfaces.ContainsKey(interfaceName))
            {
                // Store portfast setting in running config
                RunningConfig.AppendLine(" spanning-tree portfast");
            }
        }
        
        public void EnablePortfastDefault()
        {
            // Store portfast default setting in running config
            RunningConfig.AppendLine("spanning-tree portfast default");
        }
        
        public void EnableBpduGuard(string interfaceName)
        {
            if (Interfaces.ContainsKey(interfaceName))
            {
                // Store bpduguard setting in running config
                RunningConfig.AppendLine(" spanning-tree bpduguard enable");
            }
        }
        
        public void EnableBpduGuardDefault()
        {
            // Store bpduguard default setting in running config
            RunningConfig.AppendLine("spanning-tree portfast bpduguard default");
        }
        
        // CDP helper methods
        public void EnableCdpGlobal()
        {
            cdpEnabled = true;
            RunningConfig.AppendLine("cdp run");
        }
        
        public void DisableCdpGlobal()
        {
            cdpEnabled = false;
            RunningConfig.AppendLine("no cdp run");
        }
        
        public void EnableCdpInterface(string interfaceName)
        {
            if (Interfaces.ContainsKey(interfaceName))
            {
                RunningConfig.AppendLine(" cdp enable");
            }
        }
        
        public void DisableCdpInterface(string interfaceName)
        {
            if (Interfaces.ContainsKey(interfaceName))
            {
                RunningConfig.AppendLine(" no cdp enable");
            }
        }
        
        public void SetCdpTimer(int seconds)
        {
            cdpTimer = seconds;
            RunningConfig.AppendLine($"cdp timer {seconds}");
        }
        
        public void SetCdpHoldtime(int seconds)
        {
            cdpHoldtime = seconds;
            RunningConfig.AppendLine($"cdp holdtime {seconds}");
        }
        
        public string ShowCdpStatus()
        {
            var output = new StringBuilder();
            output.AppendLine($"Global CDP information:");
            output.AppendLine($"        Sending CDP packets every {cdpTimer} seconds");
            output.AppendLine($"        Sending a holdtime value of {cdpHoldtime} seconds");
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
            foreach (var iface in Interfaces.Values)
            {
                output.AppendLine($"{iface.Name} is {iface.GetStatus()}, line protocol is {(iface.IsUp ? "up" : "down")}");
                output.AppendLine($"  Encapsulation ARPA");
                output.AppendLine($"  Sending CDP packets every {cdpTimer} seconds");
                output.AppendLine($"  Holdtime is {cdpHoldtime} seconds");
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
            RoutingTable.RemoveAll(r => r.Network == route && r.Protocol == "Static");
        }
        
        public void ClearAllRoutes()
        {
            // Clear only non-connected routes
            RoutingTable.RemoveAll(r => r.Protocol != "Connected");
            UpdateConnectedRoutes();
        }
        
        public void ClearOspfProcess()
        {
            if (OspfConfig != null)
            {
                OspfConfig.Neighbors.Clear();
                // OSPF will reconverge
            }
        }
        
        public void ClearBgpPeer(string peerIp)
        {
            if (BgpConfig != null)
            {
                if (BgpConfig.Neighbors.ContainsKey(peerIp))
                {
                    BgpConfig.Neighbors[peerIp].State = "Idle";
                    // BGP will attempt to reconnect
                }
            }
        }
        
        public void ClearAllBgpPeers()
        {
            if (BgpConfig != null)
            {
                foreach (var peer in BgpConfig.Neighbors.Values)
                {
                    peer.State = "Idle";
                }
            }
        }
        
        public void ClearInterfaceCounters(string interfaceName)
        {
            if (Interfaces.ContainsKey(interfaceName))
            {
                var iface = Interfaces[interfaceName];
                iface.RxPackets = 0;
                iface.TxPackets = 0;
                iface.RxBytes = 0;
                iface.TxBytes = 0;
            }
        }
        
        public void ClearAllCounters()
        {
            foreach (var iface in Interfaces.Values)
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
        
        public void ClearCdpTable()
        {
            cdpNeighbors.Clear();
        }

        // Additional methods needed by tests
        public EigrpConfig? GetEigrpConfiguration()
        {
            return EigrpConfig;
        }

        public OspfConfig? GetOspfConfiguration()
        {
            return OspfConfig;
        }

        public BgpConfig? GetBgpConfiguration()
        {
            return BgpConfig;
        }

        public RipConfig? GetRipConfiguration()
        {
            return RipConfig;
        }

        public Dictionary<string, RouteMap> GetRouteMaps()
        {
            return RouteMaps ?? new Dictionary<string, RouteMap>();
        }

        public Dictionary<string, PrefixList> GetPrefixLists()
        {
            return PrefixLists ?? new Dictionary<string, PrefixList>();
        }

        /// <summary>
        /// Override ping simulation to provide Cisco-style output
        /// </summary>
        protected override string SimulatePing(string destination)
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
    }
} 
