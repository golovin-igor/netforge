using System.Text;
using NetForge.Simulation.CliHandlers.Extreme;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Topology.Devices;
using PortChannelConfig = NetForge.Simulation.Common.Configuration.PortChannel;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Extreme Networks EXOS device implementation
    /// </summary>
    public sealed class ExtremeDevice : NetworkDevice
    {
        public override string DeviceType => "Switch";
        private Dictionary<int, string> _vlanNameMap = new Dictionary<int, string>();

        public ExtremeDevice(string name) : base(name, "Extreme")
        {
            // Default VLAN
            AddVlan(1, new VlanConfig(1, "Default"));
            
            // Protocol registration is now handled by the vendor registry system
            _vlanNameMap[1] = "Default";
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for an Extreme switch
            // Example: 48 1Gbps ports + 4 10Gbps uplink ports
            for (int i = 1; i <= 48; i++)
            {
                AddInterface($"1:{i}", new InterfaceConfig($"1:{i}", this));
            }
            for (int i = 49; i <= 52; i++)
            {
                AddInterface($"1:{i}", new InterfaceConfig($"1:{i}", this) { Speed = "10G" }); // Assuming a way to denote speed
            }
            AddInterface("mgmt", new InterfaceConfig("mgmt", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Extreme handlers to ensure they are available for tests
            var registry = new ExtremeHandlerRegistry();
            // TODO: Register handlers with new command processor architecture
            // registry.RegisterHandlers(CommandManager);
        }

        private void RegisterCommonHandlers()
        {
            // Don't register CommonPingCommandHandler - ExtremeGeneralCommandHandler handles ping with Extreme-specific format
            // Other common handlers can be added here if needed in the future
        }

        public override string GetPrompt()
        {
            var mode = GetCurrentMode();
            switch (mode)
            {
                case "config":
                    return $"{GetHostname()}(config)#";
                case "interface":
                    var interfaceName = GetCurrentInterface();
                    return $"{GetHostname()}(config-if-{interfaceName})#";
                case "operational":
                default:
                    return $"{GetHostname()}#";
            }
        }

        private string GetPromptId()
        {
            // EXOS shows a prompt ID number
            return "1";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            // Use the command handler manager for all command processing
            // TODO: Update command processing with new architecture
            /*if (CommandManager != null)
            {
                var result = await CommandManager.ProcessCommandAsync(command);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }*/

            // Handle special case for incomplete "configure" command
            if (command.Trim().ToLower() == "configure")
            {
                return "% Incomplete command.\n" + $"* {GetHostname()}.1 # ";
            }

            // If no handler found, return EXOS error format
            return $"Invalid command\n" + $"* {GetHostname()}.1 # ";
        }

        // Helper methods for command handlers
        public string GetMode() => GetCurrentMode();
        // SetCurrentMode is inherited from base class
        // GetCurrentInterface is inherited from base class
        // SetCurrentInterface is inherited from base class

        // EXOS-specific helper methods
        public void AppendToRunningConfig(string line)
        {
            // TODO: Implement configuration building with new architecture
            // RunningConfig.AppendLine(line);
        }

        public void UpdateProtocols()
        {
            ParentNetwork?.UpdateProtocols();
        }

        public void UpdateConnectedRoutesPublic()
        {
            ForceUpdateConnectedRoutes();
        }

        public Dictionary<int, string> GetVlanNameMap() => _vlanNameMap;
        public void SetVlanName(int vlanId, string name) => _vlanNameMap[vlanId] = name;

        public void InitializeOspf(int processId)
        {
            if (GetOspfConfiguration() == null)
                SetOspfConfiguration(new OspfConfig(processId));
        }

        public void InitializeBgp(int asNumber)
        {
            if (GetBgpConfiguration() == null)
                SetBgpConfiguration(new BgpConfig(asNumber));
        }

        public void ClearPortCounters()
        {
            foreach (var iface in GetAllInterfaces().Values)
            {
                iface.RxPackets = 0;
                iface.TxPackets = 0;
                iface.RxBytes = 0;
                iface.TxBytes = 0;
            }
        }

        // ProcessShowCommand removed - now handled by ShowCommandHandler

        // ProcessConfigureCommand removed - now handled by ConfigureCommandHandler

        // ProcessCreateCommand removed - now handled by CreateCommandHandler
        // ProcessEnableCommand removed - now handled by command handlers
        // ProcessDisableCommand removed - now handled by command handlers

        // Utility methods for command handlers
        public string CidrToMask(int cidr)
        {
            uint mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        public int MaskToCidr(string mask)
        {
            if (string.IsNullOrEmpty(mask))
                return 0;

            var parts = mask.Split('.').Select(int.Parse).ToArray();
            int cidr = 0;

            foreach (var part in parts)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if ((part & (1 << i)) != 0)
                        cidr++;
                }
            }

            return cidr;
        }

        private string ShowConfiguration()
        {
            var output = new StringBuilder();

            output.AppendLine("#");
            output.AppendLine("# Module devmgr configuration.");
            output.AppendLine("#");
            output.AppendLine($"configure snmp sysName \"{GetHostname()}\"");
            output.AppendLine("");

            // Show created VLANs
            foreach (var vlan in GetAllVlans().Values.Where(v => v.Id > 1))
            {
                output.AppendLine($"create vlan \"{vlan.Name}\"");
                output.AppendLine($"configure vlan \"{vlan.Name}\" tag {vlan.Id}");
            }

            // Add running config
            // TODO: Implement configuration building with new architecture
            // if (RunningConfig.Length > 0)
            // {
            //     output.Append(RunningConfig.ToString());
            // }

            return output.ToString();
        }

        private string ShowIpConfig(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 2 && parts[2].ToLower() == "ipv4")
            {
                foreach (var vlan in GetAllVlans().Values)
                {
                    var vlanName = _vlanNameMap.GetValueOrDefault(vlan.Id, vlan.Name);
                    var vlanIface = GetAllInterfaces().Values.FirstOrDefault(i => i.VlanId == vlan.Id && !string.IsNullOrEmpty(i.IpAddress));

                    if (vlanIface != null)
                    {
                        output.AppendLine($"  VLAN      : {vlanName}");
                        output.AppendLine($"  Admin State       : ENABLED");
                        output.AppendLine($"  IP Address        : {vlanIface.IpAddress}/{MaskToCidr(vlanIface.SubnetMask)}");
                        output.AppendLine($"  Subnet Mask       : {vlanIface.SubnetMask}");
                        output.AppendLine($"  Broadcast Address : {GetBroadcastAddress(vlanIface.IpAddress, vlanIface.SubnetMask)}");
                        output.AppendLine("");
                    }
                }
            }

            return output.ToString();
        }

        private string ShowIpRoute()
        {
            var output = new StringBuilder();

            output.AppendLine("IPv4 Route Table");
            output.AppendLine("================");
            output.AppendLine("Destination      Gateway         Mtr  Flags    Use  VLAN         Duration");
            output.AppendLine("-------------------------------------------------------------------------------");

            foreach (var route in GetRoutingTable().OrderBy(r => r.Network))
            {
                var cidr = MaskToCidr(route.Mask);
                var flags = route.Protocol == "Connected" ? "UC" : "UG";
                var vlan = route.Interface.StartsWith("vlan") ? route.Interface.Replace("vlan ", "") : "----";

                output.AppendLine($"{route.Network}/{cidr,-15} {route.NextHop,-15} {route.Metric,-4} {flags,-8} 0    {vlan,-12} 0d:0h:0m:0s");
            }

            output.AppendLine("");
            output.AppendLine($"Total number of routes = {GetRoutingTable().Count}");

            return output.ToString();
        }

        private string ShowVlan(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 2 && parts[2].ToLower() == "detail")
            {
                foreach (var vlan in GetAllVlans().Values.OrderBy(v => v.Id))
                {
                    var vlanName = _vlanNameMap.GetValueOrDefault(vlan.Id, vlan.Name);
                    output.AppendLine($"VLAN Interface[{vlan.Id}] with name \"{vlanName}\" created by user");
                    output.AppendLine($"    Tagging:          802.1Q Tag {vlan.Id}");
                    output.AppendLine($"    Priority:         802.1P Priority 0");
                    output.AppendLine($"    STPD:             s0(Enabled,Auto-bind)");
                    output.AppendLine($"    Protocol:         Match all protocols");
                    output.AppendLine($"    Loopback:         Disabled");
                    output.AppendLine($"    RateShape:        Disabled");
                    output.AppendLine($"    Jumbo:            Enabled");

                    if (vlan.Interfaces.Any())
                    {
                        output.AppendLine($"    Ports:            {vlan.Interfaces.Count}.");
                        output.AppendLine($"           Untag:     {string.Join(",", vlan.Interfaces)}");
                    }
                    else
                    {
                        output.AppendLine($"    Ports:            None.");
                    }

                    output.AppendLine("");
                }
            }
            else
            {
                output.AppendLine("Name            VID  Protocol Addr       Flags                         Proto  Ports  Virtual");
                output.AppendLine("                                                                             Active Router");
                output.AppendLine("---------------------------------------------------------------------------------------------");

                foreach (var vlan in GetAllVlans().Values.OrderBy(v => v.Id))
                {
                    var vlanName = _vlanNameMap.GetValueOrDefault(vlan.Id, vlan.Name);
                    var flags = "/32";
                    var ports = vlan.Interfaces.Count;

                    output.AppendLine($"{vlanName,-15} {vlan.Id,-4} ---------------- {flags,-29} ANY    {ports,-6} VR-Default");
                }
            }

            return output.ToString();
        }

        private string ShowPorts(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 2 && parts[2].ToLower() == "information")
            {
                output.AppendLine("Port     Display                         VLAN Name          Port  Link  Speed   Duplex");
                output.AppendLine("         String                          (or # VLANs)       State State Actual  Actual");
                output.AppendLine("=====================================================================================");

                foreach (var iface in GetAllInterfaces().Values)
                {
                    var displayString = string.IsNullOrEmpty(iface.Description) ? "" : iface.Description;
                    var vlan = iface.VlanId > 0 ? _vlanNameMap.GetValueOrDefault(iface.VlanId, $"VLAN{iface.VlanId}") : "0";
                    var portState = iface.IsShutdown ? "D" : "E";
                    var linkState = iface.IsUp ? "A" : "R";

                    output.AppendLine($"{iface.Name,-8} {displayString,-31} {vlan,-18} {portState,-5} {linkState,-5} 1000    FULL");
                }
            }
            else if (parts.Length > 3 && parts[2].ToLower() == "statistics")
            {
                var portName = parts[3];
                if (GetAllInterfaces().ContainsKey(portName))
                {
                    var iface = GetAllInterfaces()[portName];
                    output.AppendLine($"Port          : {portName}");
                    output.AppendLine($"Link State    : {(iface.IsUp ? "Active" : "Ready")}");
                    output.AppendLine($"Port State    : {(iface.IsShutdown ? "Disabled" : "Enabled")}");
                    output.AppendLine($"");
                    output.AppendLine($"             Receive Statistics               Transmit Statistics");
                    output.AppendLine($"             -------------------               --------------------");
                    output.AppendLine($"Packets      {iface.RxPackets,-33} {iface.TxPackets}");
                    output.AppendLine($"Bytes        {iface.RxBytes,-33} {iface.TxBytes}");
                }
            }

            return output.ToString();
        }

        private string ShowOspf(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 2)
            {
                if (parts[2].ToLower() == "neighbor")
                {
                    if (GetOspfConfiguration() != null && GetOspfConfiguration().Neighbors.Any())
                    {
                        output.AppendLine("Neighbor ID      Pri State      Up/Dead Time       Address         Interface");
                        output.AppendLine("                                BFD Session State");
                        output.AppendLine("==============================================================================");

                        foreach (var neighbor in GetOspfConfiguration()?.Neighbors ?? new List<OspfNeighbor>())
                        {
                            output.AppendLine($"{neighbor.NeighborId,-16} {neighbor.Priority,-3} {neighbor.State,-10} 0d:0h:1m:0s/       {neighbor.IpAddress,-15} {neighbor.Interface}");
                            output.AppendLine($"                               0d:0h:0m:40s");
                            output.AppendLine($"                                N/A");
                        }
                    }
                }
                else if (parts[2].ToLower() == "interfaces")
                {
                    if (GetOspfConfiguration() != null)
                    {
                        output.AppendLine("Interface     IP Address         OSPF Area    OSPF State  Designated Router");
                        output.AppendLine("==============================================================================");

                        foreach (var ospfIface in GetOspfConfiguration().Interfaces.Values)
                        {
                            var iface = GetAllInterfaces().Values.FirstOrDefault(i => i.Name == ospfIface.Name);
                            if (iface != null)
                            {
                                output.AppendLine($"{ospfIface.Name,-13} {iface.IpAddress,-18} 0.0.0.{ospfIface.Area,-10} DR          {GetOspfConfiguration().RouterId ?? "0.0.0.0"}");
                            }
                        }
                    }
                }
            }

            return output.ToString();
        }

        private string ShowBgp(string[] parts)
        {
            var output = new StringBuilder();

            if (GetBgpConfiguration() != null && parts.Length > 2 && parts[2].ToLower() == "neighbor")
            {
                output.AppendLine("BGP Peer Table");
                output.AppendLine("==============================================================================");
                output.AppendLine("Peer Address      AS              State         Up/Down       In/Out/Pend");
                output.AppendLine("------------------------------------------------------------------------------");

                                        foreach (var neighbor in GetBgpConfiguration()?.Neighbors.Values ?? new Dictionary<string, BgpNeighbor>().Values)
                {
                    var upDown = neighbor.State == "Established" ? "0d:0h:1m:0s" : "0d:0h:0m:0s";
                    var inOut = neighbor.State == "Established" ? $"{neighbor.ReceivedRoutes.Count}/0/0" : "0/0/0";

                    output.AppendLine($"{neighbor.IpAddress,-17} {neighbor.RemoteAs,-15} {neighbor.State,-13} {upDown,-13} {inOut}");
                }

                output.AppendLine("------------------------------------------------------------------------------");
                output.AppendLine($"Total Peers : {GetBgpConfiguration()?.Neighbors.Count ?? 0}");
            }

            return output.ToString();
        }

        private string ShowSwitch()
        {
            var output = new StringBuilder();

            output.AppendLine($"System Name:          {GetHostname()}");
            output.AppendLine($"System Type:          X440-24t");
            output.AppendLine($"System Model:         SummitX");
            output.AppendLine($"System Serial Number: 1234567890");
            output.AppendLine($"MAC:                  AA:BB:CC:00:01:00");
            output.AppendLine($"System Temperature:   38 degrees C");
            output.AppendLine($"Boot Time:            Fri Jan 15 10:23:30 2021");
            output.AppendLine($"Current Time:         Fri Jan 15 10:24:13 2021");
            output.AppendLine($"System UpTime:        0 days 0 hours 0 minutes 43 seconds");

            return output.ToString();
        }

        private string ShowVersion()
        {
            var output = new StringBuilder();

            output.AppendLine($"Switch      : 1234567890 Rev 1.0 BootROM: 1.0.1.4 IMG: 30.6.1.11");
            output.AppendLine($"SysName     : {GetHostname()}");
            output.AppendLine($"SwitchType  : 220-24t-10GE2");
            output.AppendLine($"Platform    : X440-24t");
            output.AppendLine($"BootromVer  : 1.0.1.4");
            output.AppendLine($"Software Version : 30.6.1.11");

            return output.ToString();
        }

        private string ShowLog()
        {
            var output = new StringBuilder();

            output.AppendLine("01/15/2021 10:24:13.12 <Info:System> Login passed for user admin through console");
            output.AppendLine("01/15/2021 10:23:45.67 <Info:System> Port 1 link UP at speed 1 Gbps and full-duplex");
            output.AppendLine("01/15/2021 10:23:30.00 <Info:System> System started up");

            return output.ToString();
        }

        private string ShowFdb()
        {
            var output = new StringBuilder();

            output.AppendLine("MAC                     VLAN Name( Tag)  Age  Flags      Port / Virtual Port List");
            output.AppendLine("--------------------------------------------------------------------------------");
            output.AppendLine("aa:bb:cc:00:01:00     Default(0001)    0031  d m        1");
            output.AppendLine("aa:bb:cc:00:02:00     Default(0001)    0031  d m        2");
            output.AppendLine("");
            output.AppendLine("Flags : d - Dynamic, s - Static, b - Blackhole, m - MAC, i - IP,");
            output.AppendLine("        l - lockdown MAC, o - lockdown timeout MAC, M- Mirror, B - Egress Blackhole,");
            output.AppendLine("        L - Loopback port MAC");
            output.AppendLine("");
            output.AppendLine("Total: 2 Station(s) entries in the Forwarding Database.");

            return output.ToString();
        }

        private string ShowSharing()
        {
            var output = new StringBuilder();

            output.AppendLine("Load Sharing Monitor");
            output.AppendLine("Config    Current");
            output.AppendLine("Master    Master   Type   Description            Name");
            output.AppendLine("================================================================================");

            foreach (var pc in GetPortChannels())
            {
                var master = pc.Value.MemberInterfaces.FirstOrDefault() ?? $"{pc.Key}";
                var algorithm = pc.Value.Mode == "lacp" ? "L2" : "L3_L4";

                output.AppendLine($"{master,-9} {master,-8} {algorithm,-6} Load Sharing Group      {master}");
            }

            return output.ToString();
        }

        private string ShowIpArp()
        {
            var output = new StringBuilder();

            output.AppendLine("VR            Destination      Mac                Age  Static  VLAN");
            output.AppendLine("===================================================================");

            foreach (var iface in GetAllInterfaces().Values)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    var vlanName = iface.VlanId > 0 ? _vlanNameMap.GetValueOrDefault(iface.VlanId, "Default") : "Default";
                    output.AppendLine($"VR-Default    {iface.IpAddress,-16} aa:bb:cc:00:01:00  12   NO      {vlanName}");
                }
            }

            return output.ToString();
        }

        private string ShowStpd()
        {
            var output = new StringBuilder();

            output.AppendLine("Name       Mode   State       Priority   Ports");
            output.AppendLine("=================================================");
            output.AppendLine($"s0         802.1D ENABLED     {GetStpConfiguration().GetPriority(1),-10} 24");

            return output.ToString();
        }

        public string ConfigureVlan(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 2)
            {
                var vlanName = parts[2].Trim('"');
                var vlanId = GetAllVlans().FirstOrDefault(v => _vlanNameMap.GetValueOrDefault(v.Key, v.Value.Name) == vlanName).Key;

                if (vlanId > 0)
                {
                    if (parts.Length > 3)
                    {
                        switch (parts[3].ToLower())
                        {
                            case "add":
                                if (parts.Length > 5 && parts[4].ToLower() == "ports")
                                {
                                    var portList = ParsePortList(parts[5]);
                                    foreach (var port in portList)
                                    {
                                        if (GetAllInterfaces().ContainsKey(port))
                                        {
                                            GetAllInterfaces()[port].VlanId = vlanId;
                                            GetAllInterfaces()[port].SwitchportMode = "access";
                                            GetAllVlans()[vlanId].Interfaces.Add(port);
                                        }
                                    }
                                    // TODO: Implement configuration building with new architecture
                                    // RunningConfig.AppendLine($"configure vlan {vlanName} add ports {parts[5]}");
                                }
                                break;

                            case "tag":
                                if (parts.Length > 4 && int.TryParse(parts[4], out int newTag))
                                {
                                    // Change VLAN tag
                                    var vlan = GetAllVlans()[vlanId];
                                    GetAllVlans().Remove(vlanId);
                                    GetAllVlans()[newTag] = vlan;
                                    _vlanNameMap[newTag] = vlanName;
                                    _vlanNameMap.Remove(vlanId);
                                    // TODO: Implement configuration building with new architecture
                                    // RunningConfig.AppendLine($"configure vlan {vlanName} tag {newTag}");
                                }
                                break;

                            case "ipaddress":
                                if (parts.Length > 4)
                                {
                                    // Create VLAN interface
                                    var vlanIfaceName = $"vlan_{vlanName}";
                                    if (!GetAllInterfaces().ContainsKey(vlanIfaceName))
                                    {
                                        GetAllInterfaces()[vlanIfaceName] = new InterfaceConfig(vlanIfaceName, this);
                                    }
                                    var vlanIface = GetAllInterfaces()[vlanIfaceName];
                                    vlanIface.VlanId = vlanId;

                                    // Parse IP address
                                    var ipParts = parts[4].Split('/');
                                    if (ipParts.Length == 2)
                                    {
                                        vlanIface.IpAddress = ipParts[0];
                                        vlanIface.SubnetMask = CidrToMask(int.Parse(ipParts[1]));
                                        ForceUpdateConnectedRoutes();
                                        ParentNetwork?.UpdateProtocols();
                                        // TODO: Implement configuration building with new architecture
                                        // RunningConfig.AppendLine($"configure vlan {vlanName} ipaddress {parts[4]}");
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return output.ToString();
        }

        public string ConfigurePorts(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 3)
            {
                var portList = ParsePortList(parts[2]);

                switch (parts[3].ToLower())
                {
                    case "display-string":
                        if (parts.Length > 4)
                        {
                            var description = string.Join(" ", parts.Skip(4)).Trim('"');
                            foreach (var port in portList)
                            {
                                if (GetAllInterfaces().ContainsKey(port))
                                {
                                    GetAllInterfaces()[port].Description = description;
                                }
                            }
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure ports {parts[2]} display-string \"{description}\"");
                        }
                        break;

                    case "auto":
                        if (parts.Length > 4 && parts[4].ToLower() == "off")
                        {
                            if (parts.Length > 6 && parts[5].ToLower() == "speed")
                            {
                                // TODO: Implement configuration building with new architecture
                                // RunningConfig.AppendLine($"configure ports {parts[2]} auto off speed {parts[6]}");
                            }
                        }
                        break;
                }
            }

            return output.ToString();
        }

        public string ConfigureIpAddress(string[] parts)
        {
            var output = new StringBuilder();

            // configure ipaddress <vlan-name> <ip/mask>
            if (parts.Length > 3)
            {
                var vlanName = parts[2].Trim('"');
                var vlanId = GetAllVlans().FirstOrDefault(v => _vlanNameMap.GetValueOrDefault(v.Key, v.Value.Name) == vlanName).Key;

                if (vlanId > 0)
                {
                    // Create or update VLAN interface
                    var vlanIfaceName = $"vlan {vlanId}";
                    if (!GetAllInterfaces().TryGetValue(vlanIfaceName, out var vlanIface))
                    {
                        vlanIface = new InterfaceConfig(vlanIfaceName, this);
                        GetAllInterfaces()[vlanIfaceName] = vlanIface;
                    }

                    vlanIface.VlanId = vlanId;

                    // Parse IP address
                    var ipParts = parts[3].Split('/');
                    if (ipParts.Length != 2)
                    {
                        return output.ToString();
                    }

                    vlanIface.IpAddress = ipParts[0];
                    vlanIface.SubnetMask = CidrToMask(int.Parse(ipParts[1]));
                    ForceUpdateConnectedRoutes();
                    ParentNetwork?.UpdateProtocols();
                    // TODO: Implement configuration building with new architecture
                    // RunningConfig.AppendLine($"configure ipaddress {vlanName} {parts[3]}");
                }
            }

            return output.ToString();
        }

        public string ConfigureOspf(string[] parts)
        {
            var output = new StringBuilder();

            if (GetOspfConfiguration() == null)
                SetOspfConfiguration(new OspfConfig(1));

            if (parts.Length > 2)
            {
                switch (parts[2].ToLower())
                {
                    case "routerid":
                        if (parts.Length > 3)
                        {
                            GetOspfConfiguration().RouterId = parts[3];
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure ospf routerid {parts[3]}");
                        }
                        break;

                    case "add":
                        if (parts.Length > 4 && parts[3].ToLower() == "vlan")
                        {
                            var vlanName = parts[4];
                            var vlanId = GetAllVlans().FirstOrDefault(v => _vlanNameMap.GetValueOrDefault(v.Key, v.Value.Name) == vlanName).Key;

                            if (vlanId > 0)
                            {
                                var area = parts.Length > 6 && parts[5].ToLower() == "area" ? parts[6] : "0.0.0.0";
                                var areaNum = int.Parse(area.Replace("0.0.0.", ""));

                                // Add interface to OSPF
                                var vlanIfaceName = $"vlan {vlanId}";
                                if (GetAllInterfaces().ContainsKey(vlanIfaceName))
                                {
                                    GetOspfConfiguration().Interfaces[vlanIfaceName] = new OspfInterface(vlanIfaceName, areaNum);

                                    var iface = GetAllInterfaces()[vlanIfaceName];
                                    if (!string.IsNullOrEmpty(iface.IpAddress))
                                    {
                                        var network = GetNetwork(iface.IpAddress, iface.SubnetMask);
                                        GetOspfConfiguration().NetworkAreas[network] = areaNum;
                                        if (!GetOspfConfiguration().Networks.Contains(network))
                                        {
                                            GetOspfConfiguration().Networks.Add(network);
                                        }
                                    }

                                    // TODO: Implement configuration building with new architecture
                                    // RunningConfig.AppendLine($"configure ospf add vlan {vlanName} area {area}");
                                }
                            }
                        }
                        break;
                }
            }

            return output.ToString();
        }

        public string ConfigureBgp(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 2)
            {
                switch (parts[2].ToLower())
                {
                    case "as-number":
                        if (parts.Length > 3 && int.TryParse(parts[3], out int asNumber))
                        {
                            if (GetBgpConfiguration() == null)
                                SetBgpConfiguration(new BgpConfig(asNumber));
                            else
                                GetBgpConfiguration().LocalAs = asNumber;
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure bgp as-number {asNumber}");
                        }
                        break;

                    case "routerid":
                        if (parts.Length > 3)
                        {
                            if (GetBgpConfiguration() == null)
                                SetBgpConfiguration(new BgpConfig(65001));
                            GetBgpConfiguration().RouterId = parts[3];
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure bgp routerid {parts[3]}");
                        }
                        break;

                    case "neighbor":
                        if (parts.Length > 5)
                        {
                            var neighborIp = parts[3];
                            var remoteAs = parts[5];
                            if (GetBgpConfiguration() == null)
                                SetBgpConfiguration(new BgpConfig(65001));
                            GetBgpConfiguration().Neighbors[neighborIp] = new BgpNeighbor(neighborIp, int.Parse(remoteAs));
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure bgp neighbor {neighborIp} remote-as {remoteAs}");
                        }
                        break;
                }
            }

            return output.ToString();
        }

        public string ConfigureStpd(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length > 3 && parts[2].ToLower() == "s0")
            {
                switch (parts[3].ToLower())
                {
                    case "mode":
                        if (parts.Length > 4)
                        {
                            var mode = parts[4];
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure stpd s0 mode {mode}");
                        }
                        break;

                    case "priority":
                        if (parts.Length > 4 && int.TryParse(parts[4], out int priority))
                        {
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure stpd s0 priority {priority}");
                        }
                        break;

                    case "add":
                        if (parts.Length > 5 && parts[4].ToLower() == "vlan")
                        {
                            var vlanName = parts[5];
                            // TODO: Implement configuration building with new architecture
                            // RunningConfig.AppendLine($"configure stpd s0 add vlan {vlanName}");
                        }
                        break;
                }
            }

            return output.ToString();
        }

        private string SimulatePingExos(string destIp)
        {
            var output = new StringBuilder();
            output.AppendLine($"Ping(ICMP) {destIp}: 4 packets, 8 data bytes, interval 1 second(s).");

            bool reachable = IsReachable(destIp);

            for (int i = 1; i <= 4; i++)
            {
                if (reachable)
                {
                    output.AppendLine($"Reply from {destIp}: bytes=8 icmp_seq={i} ttl=64 time=1 ms");
                }
                else
                {
                    output.AppendLine($"Request timeout for icmp_seq {i}");
                }
            }

            output.AppendLine("");
            output.AppendLine("---{destIp} ping statistics---");
            output.AppendLine($"4 packets transmitted, {(reachable ? 4 : 0)} packets received, {(reachable ? 0 : 100)}% loss");
            if (reachable)
            {
                output.AppendLine("round-trip min/avg/max/stddev = 1.000/1.000/1.000/0.000 ms");
            }

            return output.ToString();
        }

        private bool IsReachable(string destIp)
        {
            // Check if IP is in any connected network
            foreach (var route in GetRoutingTable().Where(r => r.Protocol == "Connected"))
            {
                if (IsIpInNetwork(destIp, route.Network, route.Mask))
                {
                    return true;
                }
            }

            // Check if there's a route to the destination
            foreach (var route in GetRoutingTable())
            {
                if (IsIpInNetwork(destIp, route.Network, route.Mask))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIpInNetwork(string ip, string network, string mask)
        {
            try
            {
                var ipBytes = ip.Split('.').Select(byte.Parse).ToArray();
                var networkBytes = network.Split('.').Select(byte.Parse).ToArray();
                var maskBytes = mask.Split('.').Select(byte.Parse).ToArray();

                for (int i = 0; i < 4; i++)
                {
                    if ((ipBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<string> ParsePortList(string portSpec)
        {
            var ports = new List<string>();

            // Handle port ranges like "1-3" or individual ports like "1,2,3"
            if (portSpec.Contains("-"))
            {
                var parts = portSpec.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        ports.Add(i.ToString());
                    }
                }
            }
            else if (portSpec.Contains(","))
            {
                ports.AddRange(portSpec.Split(',').Select(p => p.Trim()));
            }
            else
            {
                ports.Add(portSpec);
            }

            return ports;
        }

        private string GetBroadcastAddress(string ip, string mask)
        {
            try
            {
                var ipBytes = ip.Split('.').Select(byte.Parse).ToArray();
                var maskBytes = mask.Split('.').Select(byte.Parse).ToArray();
                var broadcastBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
                }

                return string.Join(".", broadcastBytes);
            }
            catch
            {
                return "255.255.255.255";
            }
        }

        public void CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            if (!GetPortChannels().ContainsKey(channelId))
            {
                GetPortChannels()[channelId] = new PortChannelConfig(channelId);
            }
            GetPortChannels()[channelId].MemberInterfaces.Add(interfaceName);
            GetPortChannels()[channelId].Mode = mode;
        }

        public void AddInterfaceToPortChannel(string interfaceName, int channelId)
        {
            if (!GetPortChannels().ContainsKey(channelId))
            {
                GetPortChannels()[channelId] = new PortChannelConfig(channelId);
            }
            GetPortChannels()[channelId].MemberInterfaces.Add(interfaceName);
        }
    }
}
