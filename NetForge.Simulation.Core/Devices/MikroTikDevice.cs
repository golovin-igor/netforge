using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Configuration;
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Devices
{
    /// <summary>
    /// MikroTik RouterOS device implementation
    /// </summary>
    public class MikroTikDevice : NetworkDevice
    {
        public MikroTikDevice(string name) : base(name)
        {
            Vendor = "MikroTik";
            SystemSettings["version"] = "6.48.6";
            SystemSettings["board-name"] = "RB750Gr3";
            SystemSettings["architecture-name"] = "mipsbe";

            // Initialize default log entries
            LogEntries.Add("jan/15 10:23:30 system,info system identity was changed by admin");
            LogEntries.Add("jan/15 10:23:45 interface,info ether1 link up (speed 1G, full duplex)");
            LogEntries.Add("jan/15 10:24:13 system,info,account user admin logged in from 192.168.88.2 via winbox");

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "MikroTik" vendor
            AutoRegisterProtocols();
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Default interfaces for MikroTik
            Interfaces["ether1"] = new InterfaceConfig("ether1", this) { Description = "WAN" };
            Interfaces["ether2"] = new InterfaceConfig("ether2", this) { Description = "LAN1" };
            Interfaces["ether3"] = new InterfaceConfig("ether3", this) { Description = "LAN2" };
            Interfaces["ether4"] = new InterfaceConfig("ether4", this) { Description = "LAN3" };
            Interfaces["wlan1"] = new InterfaceConfig("wlan1", this) { Description = "Wireless" };
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register MikroTik handlers to ensure they are available for tests
            var registry = new NetForge.Simulation.CliHandlers.MikroTik.MikroTikHandlerRegistry();
            registry.Initialize(); // Initialize vendor context factory
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return $"[{Hostname}] > ";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            var output = new StringBuilder();
            var trimmed = command.Trim();

            try
            {
                // Process commands based on their path structure
                var parts = trimmed.Split(' ');

                if (trimmed.StartsWith("/"))
                {
                    output.Append(ProcessMikroTikCommand(parts));
                }
                else
                {
                    output.AppendLine("bad command name (line 1 column 1)");
                }
            }
            catch (Exception)
            {
                output.AppendLine("syntax error (line 1 column 1)");
            }

            output.Append(GetPrompt());
            return output.ToString();
        }

        private string ProcessMikroTikCommand(string[] parts)
        {
            var output = new StringBuilder();
            var path = parts[0].ToLower();

            switch (path)
            {
                case "/export":
                    output.Append(GenerateExportOutput());
                    break;

                case "/system":
                    if (parts.Length > 1)
                    {
                        output.Append(ProcessSystemCommand(parts));
                    }
                    break;

                case "/interface":
                    if (parts.Length > 1)
                    {
                        output.Append(ProcessInterfaceCommand(parts));
                    }
                    break;

                case "/ip":
                    if (parts.Length > 1)
                    {
                        output.Append(ProcessIpCommand(parts));
                    }
                    break;

                case "/routing":
                    if (parts.Length > 1)
                    {
                        output.Append(ProcessRoutingCommand(parts));
                    }
                    break;

                case "/ping":
                    if (parts.Length > 1)
                    {
                        output.Append(SimulatePingMikroTik(parts[1]));
                    }
                    break;

                case "/file":
                    if (parts.Length > 1 && parts[1].ToLower() == "print")
                    {
                        output.AppendLine("# NAME                          TYPE                          SIZE CREATION-TIME");
                        output.AppendLine("0 flash                         disk                                jan/15/2021 10:23:25");
                        output.AppendLine("1 flash/skins                   directory                           jan/15/2021 10:23:25");
                        output.AppendLine("2 autosupout.rif                backup                       12.3KiB jan/15/2021 10:24:00");
                    }
                    break;

                case "/log":
                    if (parts.Length > 1 && parts[1].ToLower() == "print")
                    {
                        foreach (var log in LogEntries.TakeLast(10))
                        {
                            output.AppendLine(log);
                        }
                    }
                    break;

                default:
                    output.AppendLine("bad command name (line 1 column 1)");
                    break;
            }

            return output.ToString();
        }

        private string ProcessSystemCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length < 2)
                return "syntax error (line 1 column 7)\n";

            switch (parts[1].ToLower())
            {
                case "identity":
                    if (parts.Length > 2)
                    {
                        if (parts[2].ToLower() == "set" && parts.Length > 3)
                        {
                            // Extract name parameter
                            var nameParam = string.Join(" ", parts.Skip(3));
                            if (nameParam.StartsWith("name="))
                            {
                                Hostname = nameParam.Substring(5).Trim('"');
                                RunningConfig.AppendLine($"/system identity set name=\"{Hostname}\"");
                                LogEntries.Add($"{DateTime.Now:MMM/dd HH:mm:ss} system,info system identity was changed to {Hostname} by admin");
                            }
                        }
                        else if (parts[2].ToLower() == "print")
                        {
                            output.AppendLine($"  name: {Hostname}");
                        }
                    }
                    break;

                case "resource":
                    if (parts.Length > 2 && parts[2].ToLower() == "print")
                    {
                        output.AppendLine($"                   uptime: 1w2d3h24m13s");
                        output.AppendLine($"                  version: {SystemSettings["version"]} (stable)");
                        output.AppendLine($"               build-time: Dec/04/2020 14:19:51");
                        output.AppendLine($"         factory-software: 6.44.6");
                        output.AppendLine($"              free-memory: 233.4MiB");
                        output.AppendLine($"             total-memory: 256.0MiB");
                        output.AppendLine($"                      cpu: MIPS 74Kc V5.0");
                        output.AppendLine($"                cpu-count: 4");
                        output.AppendLine($"            cpu-frequency: 880MHz");
                        output.AppendLine($"                 cpu-load: 1%");
                        output.AppendLine($"           free-hdd-space: 14.5MiB");
                        output.AppendLine($"          total-hdd-space: 16.0MiB");
                        output.AppendLine($"  write-sect-since-reboot: 2147");
                        output.AppendLine($"         write-sect-total: 52644");
                        output.AppendLine($"               bad-blocks: 0%");
                        output.AppendLine($"        architecture-name: {SystemSettings["architecture-name"]}");
                        output.AppendLine($"               board-name: {SystemSettings["board-name"]}");
                        output.AppendLine($"                platform: MikroTik");
                    }
                    break;

                case "reboot":
                    output.AppendLine("Reboot, yes? [y/N]:");
                    break;

                case "package":
                    if (parts.Length > 2 && parts[2].ToLower() == "print")
                    {
                        output.AppendLine("Flags: X - disabled");
                        output.AppendLine(" #   NAME                          VERSION");
                        output.AppendLine(" 0   routeros-mipsbe               6.48.6");
                        output.AppendLine(" 1   system                        6.48.6");
                        output.AppendLine(" 2   ipv6                          6.48.6");
                        output.AppendLine(" 3   wireless                      6.48.6");
                        output.AppendLine(" 4   hotspot                       6.48.6");
                        output.AppendLine(" 5   mpls                          6.48.6");
                        output.AppendLine(" 6   routing                       6.48.6");
                        output.AppendLine(" 7   ppp                           6.48.6");
                        output.AppendLine(" 8   dhcp                          6.48.6");
                        output.AppendLine(" 9   security                      6.48.6");
                        output.AppendLine("10   advanced-tools                6.48.6");
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessInterfaceCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length < 2)
                return "syntax error (line 1 column 10)\n";

            switch (parts[1].ToLower())
            {
                case "print":
                    output.AppendLine("Flags: D - dynamic, X - disabled, R - running, S - slave");
                    output.AppendLine(" #     NAME                                TYPE       ACTUAL-MTU L2MTU  MAX-L2MTU MAC-ADDRESS");

                    int idx = 0;
                    foreach (var iface in Interfaces.Values)
                    {
                        var flags = "";
                        if (!iface.IsUp) flags += "X";
                        if (iface.IsUp && !iface.IsShutdown) flags += "R";
                        if (string.IsNullOrEmpty(flags)) flags = " ";

                        var type = iface.Name.StartsWith("ether") ? "ether" :
                                   iface.Name == "bridge" ? "bridge" :
                                   iface.Name.StartsWith("vlan") ? "vlan" : "unknown";

                        output.AppendLine($" {idx,-2} {flags,-2} {iface.Name,-35} {type,-10} 1500       1598             {GetMacAddress(idx)}");
                        idx++;
                    }
                    break;

                case "ethernet":
                    if (parts.Length > 2)
                    {
                        output.Append(ProcessInterfaceEthernetCommand(parts));
                    }
                    break;

                case "vlan":
                    if (parts.Length > 2)
                    {
                        output.Append(ProcessInterfaceVlanCommand(parts));
                    }
                    break;

                case "bonding":
                    if (parts.Length > 2 && parts[2].ToLower() == "print")
                    {
                        output.AppendLine("Flags: X - disabled, R - running");
                        output.AppendLine(" #   NAME                         MTU   MAC-ADDRESS       ARP     SLAVES");
                        // Show bonding interfaces if any
                        foreach (var pc in PortChannels)
                        {
                            var members = string.Join(",", pc.Value.MemberInterfaces);
                            output.AppendLine($" 0 R bond{pc.Key}                       1500  {GetMacAddress(pc.Key)}  enabled {members}");
                        }
                    }
                    break;

                case "bridge":
                    if (parts.Length > 2)
                    {
                        if (parts[2].ToLower() == "port" && parts.Length > 3 && parts[3].ToLower() == "print")
                        {
                            output.AppendLine("Flags: X - disabled, I - inactive, D - dynamic, H - hw-offload");
                            output.AppendLine(" #     INTERFACE                                              BRIDGE                                              HW  PVID PRIORITY  PATH-COST INTERNAL-PATH-COST    HORIZON");

                            // Show bridge ports
                            int portIdx = 0;
                            foreach (var iface in Interfaces.Values)
                            {
                                if (iface.SwitchportMode == "access" && iface.Name != "bridge")
                                {
                                    output.AppendLine($" {portIdx,-5} {iface.Name,-54} bridge                                              yes    1     0x80         10                 10       none");
                                    portIdx++;
                                }
                            }
                        }
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessInterfaceEthernetCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length < 3)
                return "syntax error (line 1 column 20)\n";

            switch (parts[2].ToLower())
            {
                case "print":
                    output.AppendLine(" # NAME          MTU MAC-ADDRESS       ARP     SWITCH");

                    int idx = 0;
                    foreach (var iface in Interfaces.Values.Where(i => i.Name.StartsWith("ether")))
                    {
                        var arp = "enabled";
                        output.AppendLine($" {idx} {iface.Name,-13} 1500 {GetMacAddress(idx)} {arp,-7} switch1");
                        idx++;
                    }
                    break;

                case "enable":
                    if (parts.Length > 3)
                    {
                        var ifaceName = ExtractValue(string.Join(" ", parts), "numbers") ?? parts[3];
                        if (Interfaces.ContainsKey(ifaceName))
                        {
                            Interfaces[ifaceName].IsShutdown = false;
                            Interfaces[ifaceName].IsUp = true;
                            RunningConfig.AppendLine($"/interface ethernet enable {ifaceName}");
                            ParentNetwork?.UpdateProtocols();
                        }
                    }
                    break;

                case "disable":
                    if (parts.Length > 3)
                    {
                        var ifaceName = ExtractValue(string.Join(" ", parts), "numbers") ?? parts[3];
                        if (Interfaces.ContainsKey(ifaceName))
                        {
                            Interfaces[ifaceName].IsShutdown = true;
                            Interfaces[ifaceName].IsUp = false;
                            RunningConfig.AppendLine($"/interface ethernet disable {ifaceName}");
                            ParentNetwork?.UpdateProtocols();
                        }
                    }
                    break;

                case "reset-counters":
                    // Clear all interface counters
                    foreach (var iface in Interfaces.Values)
                    {
                        iface.RxPackets = 0;
                        iface.TxPackets = 0;
                        iface.RxBytes = 0;
                        iface.TxBytes = 0;
                    }
                    break;

                case "monitor":
                    if (parts.Length > 3)
                    {
                        var ifaceName = parts[3];
                        if (Interfaces.ContainsKey(ifaceName))
                        {
                            var iface = Interfaces[ifaceName];
                            output.AppendLine($"                      name: {ifaceName}");
                            output.AppendLine($"                    status: {(iface.IsUp ? "link-ok" : "no-link")}");
                            output.AppendLine($"          auto-negotiation: done");
                            output.AppendLine($"                      rate: 1Gbps");
                            output.AppendLine($"               full-duplex: yes");
                            output.AppendLine($"           tx-flow-control: no");
                            output.AppendLine($"           rx-flow-control: no");
                            output.AppendLine($"               advertising: 10M-half,10M-full,100M-half,100M-full,1000M-half,1000M-full");
                            output.AppendLine($"  link-partner-advertising: 10M-half,10M-full,100M-half,100M-full,1000M-full");
                        }
                    }
                    break;

                case "set":
                    if (parts.Length > 3)
                    {
                        var cmdLine = string.Join(" ", parts);
                        var ifaceName = ExtractNumberParam(cmdLine) ?? parts[3];

                        if (Interfaces.ContainsKey(ifaceName))
                        {
                            var iface = Interfaces[ifaceName];

                            // Check for various parameters
                            var comment = ExtractValue(cmdLine, "comment");
                            if (!string.IsNullOrEmpty(comment))
                            {
                                iface.Description = comment.Trim('"');
                                RunningConfig.AppendLine($"/interface ethernet set {ifaceName} comment=\"{iface.Description}\"");
                            }

                            var disabled = ExtractValue(cmdLine, "disabled");
                            if (disabled == "yes")
                            {
                                iface.IsShutdown = true;
                                iface.IsUp = false;
                                RunningConfig.AppendLine($"/interface ethernet set {ifaceName} disabled=yes");
                                ParentNetwork?.UpdateProtocols();
                            }
                            else if (disabled == "no")
                            {
                                iface.IsShutdown = false;
                                iface.IsUp = true;
                                RunningConfig.AppendLine($"/interface ethernet set {ifaceName} disabled=no");
                                ParentNetwork?.UpdateProtocols();
                            }
                        }
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessInterfaceVlanCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length < 3)
                return "syntax error (line 1 column 15)\n";

            switch (parts[2].ToLower())
            {
                case "add":
                    // Parse vlan-id, interface, name
                    var cmdLine = string.Join(" ", parts);
                    var vlanId = ExtractIntValue(cmdLine, "vlan-id");
                    var ifaceName = ExtractValue(cmdLine, "interface");
                    var vlanName = ExtractValue(cmdLine, "name");

                    if (vlanId > 0 && !string.IsNullOrEmpty(ifaceName))
                    {
                        if (!Vlans.ContainsKey(vlanId))
                        {
                            Vlans[vlanId] = new VlanConfig(vlanId, vlanName ?? $"vlan{vlanId}");
                        }
                        // Create VLAN interface
                        var vlanIfaceName = vlanName ?? $"vlan{vlanId}";
                        Interfaces[vlanIfaceName] = new InterfaceConfig(vlanIfaceName, this);
                        Interfaces[vlanIfaceName].VlanId = vlanId;

                        RunningConfig.AppendLine($"/interface vlan add vlan-id={vlanId} interface={ifaceName} name={vlanIfaceName}");
                    }
                    break;

                case "print":
                    output.AppendLine("Flags: X - disabled, R - running");
                    output.AppendLine(" #   NAME              MTU   ARP     VLAN-ID INTERFACE");

                    int idx = 0;
                    foreach (var iface in Interfaces.Values.Where(i => i.VlanId > 0))
                    {
                        var flags = iface.IsUp ? "R" : "X";
                        output.AppendLine($" {idx} {flags} {iface.Name,-17} 1500  enabled {iface.VlanId,-7} bridge");
                        idx++;
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessIpCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length < 2)
                return "syntax error (line 1 column 3)\n";

            switch (parts[1].ToLower())
            {
                case "address":
                    if (parts.Length > 2)
                    {
                        if (parts[2].ToLower() == "add")
                        {
                            var cmdLine = string.Join(" ", parts);
                            var address = ExtractValue(cmdLine, "address");
                            var ifaceName = ExtractValue(cmdLine, "interface");

                            if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(ifaceName))
                            {
                                var addrParts = address.Split('/');
                                if (addrParts.Length == 2 && Interfaces.ContainsKey(ifaceName))
                                {
                                    var iface = Interfaces[ifaceName];
                                    iface.IpAddress = addrParts[0];
                                    iface.SubnetMask = CidrToMask(int.Parse(addrParts[1]));
                                    UpdateConnectedRoutes();
                                    ParentNetwork?.UpdateProtocols();

                                    RunningConfig.AppendLine($"/ip address add address={address} interface={ifaceName}");
                                }
                            }
                        }
                        else if (parts[2].ToLower() == "print")
                        {
                            output.AppendLine("Flags: X - disabled, I - invalid, D - dynamic");
                            output.AppendLine(" #   ADDRESS            NETWORK         INTERFACE");

                            int idx = 0;
                            foreach (var iface in Interfaces.Values)
                            {
                                if (!string.IsNullOrEmpty(iface.IpAddress))
                                {
                                    var cidr = MaskToCidr(iface.SubnetMask);
                                    var network = GetNetwork(iface.IpAddress, iface.SubnetMask);
                                    output.AppendLine($" {idx,-3} {iface.IpAddress}/{cidr,-17} {network,-15} {iface.Name}");
                                    idx++;
                                }
                            }
                        }
                    }
                    break;

                case "route":
                    if (parts.Length > 2)
                    {
                        if (parts[2].ToLower() == "print")
                        {
                            output.AppendLine("Flags: X - disabled, A - active, D - dynamic, C - connect, S - static, r - rip, b - bgp, o - ospf, m - mme,");
                            output.AppendLine("B - blackhole, U - unreachable, P - prohibit");
                            output.AppendLine(" #      DST-ADDRESS        PREF-SRC        GATEWAY            DISTANCE");

                            int idx = 0;
                            foreach (var route in RoutingTable.OrderBy(r => r.Network))
                            {
                                var flags = "A";
                                flags += route.Protocol switch
                                {
                                    "Connected" => "DC",
                                    "Static" => "S",
                                    "OSPF" => "Do",
                                    "BGP" => "Db",
                                    "RIP" => "Dr",
                                    _ => ""
                                };

                                var cidr = MaskToCidr(route.Mask);
                                var gateway = route.Protocol == "Connected" ? route.Interface : route.NextHop;
                                output.AppendLine($" {idx,-2} {flags,-3} {route.Network}/{cidr,-18}                 {gateway,-18} {route.AdminDistance}");
                                idx++;
                            }
                        }
                        else if (parts[2].ToLower() == "add")
                        {
                            var cmdLine = string.Join(" ", parts);
                            var dstAddress = ExtractValue(cmdLine, "dst-address");
                            var gateway = ExtractValue(cmdLine, "gateway");

                            if (!string.IsNullOrEmpty(dstAddress) && !string.IsNullOrEmpty(gateway))
                            {
                                var addrParts = dstAddress.Split('/');
                                if (addrParts.Length == 2)
                                {
                                    var network = addrParts[0];
                                    var mask = CidrToMask(int.Parse(addrParts[1]));

                                    var route = new Route(network, mask, gateway, "", "Static");
                                    route.Metric = 1;
                                    RoutingTable.Add(route);

                                    RunningConfig.AppendLine($"/ip route add dst-address={dstAddress} gateway={gateway}");
                                }
                            }
                        }
                    }
                    break;

                case "arp":
                    if (parts.Length > 2 && parts[2].ToLower() == "print")
                    {
                        output.AppendLine("Flags: X - disabled, I - invalid, H - DHCP, D - dynamic, P - published, C - complete");
                        output.AppendLine(" #    ADDRESS                                 MAC-ADDRESS       INTERFACE");

                        int idx = 0;
                        foreach (var iface in Interfaces.Values)
                        {
                            if (!string.IsNullOrEmpty(iface.IpAddress))
                            {
                                output.AppendLine($" {idx,-3} DC {iface.IpAddress,-40} {GetMacAddress(idx)} {iface.Name}");
                                idx++;
                            }
                        }
                    }
                    break;

                case "firewall":
                    if (parts.Length > 2 && parts[2].ToLower() == "filter")
                    {
                        if (parts.Length > 3 && parts[3].ToLower() == "print")
                        {
                            output.AppendLine("Flags: X - disabled, I - invalid, D - dynamic");
                            output.AppendLine(" 0    ;;; defconf: accept established,related,untracked");
                            output.AppendLine("      chain=input action=accept connection-state=established,related,untracked");
                            output.AppendLine("");
                            output.AppendLine(" 1    ;;; defconf: drop invalid");
                            output.AppendLine("      chain=input action=drop connection-state=invalid");
                            output.AppendLine("");
                            output.AppendLine(" 2    ;;; defconf: accept ICMP");
                            output.AppendLine("      chain=input action=accept protocol=icmp");
                        }
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessRoutingCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (parts.Length < 2)
                return "syntax error (line 1 column 8)\n";

            switch (parts[1].ToLower())
            {
                case "ospf":
                    if (parts.Length > 2)
                    {
                        output.Append(ProcessRoutingOspfCommand(parts));
                    }
                    break;

                case "bgp":
                    if (parts.Length > 2)
                    {
                        output.Append(ProcessRoutingBgpCommand(parts));
                    }
                    break;

                case "rip":
                    if (parts.Length > 2)
                    {
                        output.Append(ProcessRoutingRipCommand(parts));
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessRoutingOspfCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (OspfConfig == null)
                OspfConfig = new OspfConfig(1);

            switch (parts[2].ToLower())
            {
                case "instance":
                    if (parts.Length > 3 && parts[3].ToLower() == "set")
                    {
                        var cmdLine = string.Join(" ", parts);
                        var routerId = ExtractValue(cmdLine, "router-id");
                        if (!string.IsNullOrEmpty(routerId))
                        {
                            OspfConfig.RouterId = routerId;
                            RunningConfig.AppendLine($"/routing ospf instance set 0 router-id={routerId}");
                        }
                    }
                    break;

                case "network":
                    if (parts.Length > 3 && parts[3].ToLower() == "add")
                    {
                        var cmdLine = string.Join(" ", parts);
                        var network = ExtractValue(cmdLine, "network");
                        var area = ExtractValue(cmdLine, "area") ?? "0.0.0.0";

                        if (!string.IsNullOrEmpty(network))
                        {
                            OspfConfig.NetworkAreas[network] = 0; // Simplified area handling
                            if (!OspfConfig.Networks.Contains(network))
                            {
                                OspfConfig.Networks.Add(network);
                            }
                            RunningConfig.AppendLine($"/routing ospf network add network={network} area={area}");
                            ParentNetwork?.UpdateProtocols();
                        }
                    }
                    break;

                case "neighbor":
                    if (parts.Length > 3 && parts[3].ToLower() == "print")
                    {
                        if (OspfConfig.Neighbors.Any())
                        {
                            output.AppendLine(" # INSTANCE ROUTER-ID      ADDRESS         INTERFACE              PRIORITY STATE         STATE-CHANGES");

                            int idx = 0;
                            foreach (var neighbor in OspfConfig.Neighbors)
                            {
                                output.AppendLine($" {idx} default  {neighbor.NeighborId,-14} {neighbor.IpAddress,-15} {neighbor.Interface,-22} {neighbor.Priority,-8} {neighbor.State,-13} 3");
                                idx++;
                            }
                        }
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessRoutingBgpCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (BgpConfig == null)
                BgpConfig = new BgpConfig(65000);

            switch (parts[2].ToLower())
            {
                case "instance":
                    if (parts.Length > 3 && parts[3].ToLower() == "set")
                    {
                        var cmdLine = string.Join(" ", parts);
                        var asn = ExtractIntValue(cmdLine, "as");
                        var routerId = ExtractValue(cmdLine, "router-id");

                        if (asn > 0)
                        {
                            BgpConfig.LocalAs = asn;
                            RunningConfig.AppendLine($"/routing bgp instance set default as={asn}");
                        }
                        if (!string.IsNullOrEmpty(routerId))
                        {
                            BgpConfig.RouterId = routerId;
                            RunningConfig.AppendLine($"/routing bgp instance set default router-id={routerId}");
                        }
                    }
                    break;

                case "peer":
                    if (parts.Length > 3)
                    {
                        if (parts[3].ToLower() == "add")
                        {
                            var cmdLine = string.Join(" ", parts);
                            var remoteAddr = ExtractValue(cmdLine, "remote-address");
                            var remoteAs = ExtractIntValue(cmdLine, "remote-as");

                            if (!string.IsNullOrEmpty(remoteAddr) && remoteAs > 0)
                            {
                                var neighbor = new BgpNeighbor(remoteAddr, remoteAs);
                                BgpConfig.Neighbors[neighbor.IpAddress] = neighbor;
                                RunningConfig.AppendLine($"/routing bgp peer add remote-address={remoteAddr} remote-as={remoteAs}");
                                ParentNetwork?.UpdateProtocols();
                            }
                        }
                        else if (parts[3].ToLower() == "print")
                        {
                            output.AppendLine("Flags: X - disabled, E - established");
                            output.AppendLine(" # INSTANCE REMOTE-ADDRESS                                  REMOTE-AS");

                            int idx = 0;
                            foreach (var neighbor in BgpConfig.Neighbors.Values)
                            {
                                var flags = neighbor.State == "Established" ? "E" : " ";
                                output.AppendLine($" {idx} {flags} default  {neighbor.IpAddress,-47} {neighbor.RemoteAs}");
                                idx++;
                            }
                        }
                    }
                    break;

                case "network":
                    if (parts.Length > 3 && parts[3].ToLower() == "add")
                    {
                        var cmdLine = string.Join(" ", parts);
                        var network = ExtractValue(cmdLine, "network");

                        if (!string.IsNullOrEmpty(network))
                        {
                            BgpConfig.Networks.Add(network);
                            RunningConfig.AppendLine($"/routing bgp network add network={network}");
                            ParentNetwork?.UpdateProtocols();
                        }
                    }
                    break;
            }

            return output.ToString();
        }

        private string ProcessRoutingRipCommand(string[] parts)
        {
            var output = new StringBuilder();

            if (RipConfig == null)
                RipConfig = new RipConfig();

            if (parts.Length > 2)
            {
                switch (parts[2].ToLower())
                {
                    case "network":
                        if (parts.Length > 3 && parts[3].ToLower() == "add")
                        {
                            var cmdLine = string.Join(" ", parts);
                            var network = ExtractValue(cmdLine, "network");

                            if (!string.IsNullOrEmpty(network))
                            {
                                RipConfig.Networks.Add(network);
                                RunningConfig.AppendLine($"/routing rip network add network={network}");
                                ParentNetwork?.UpdateProtocols();
                            }
                        }
                        break;

                    case "interface":
                        if (parts.Length > 3 && parts[3].ToLower() == "add")
                        {
                            var cmdLine = string.Join(" ", parts);
                            var ifaceName = ExtractValue(cmdLine, "interface");

                            if (!string.IsNullOrEmpty(ifaceName))
                            {
                                RunningConfig.AppendLine($"/routing rip interface add interface={ifaceName}");
                            }
                        }
                        break;
                }
            }

            return output.ToString();
        }

        private string GenerateExportOutput()
        {
            var output = new StringBuilder();

            output.AppendLine($"# jan/15/2021 10:24:13 by RouterOS {SystemSettings["version"]}");
            output.AppendLine("# software id = 1234-5678");
            output.AppendLine("#");
            output.AppendLine($"# model = {SystemSettings["board-name"]}");
            output.AppendLine("# serial number = 1234567890AB");

            // System identity
            output.AppendLine($"/system identity set name=\"{Hostname}\"");

            // Interface configurations
            foreach (var iface in Interfaces.Values)
            {
                if (iface.IsShutdown)
                {
                    output.AppendLine($"/interface ethernet set {iface.Name} disabled=yes");
                }
                if (!string.IsNullOrEmpty(iface.Description))
                {
                    output.AppendLine($"/interface ethernet set {iface.Name} comment=\"{iface.Description}\"");
                }
            }

            // VLAN configurations
            foreach (var vlan in Vlans.Values.Where(v => v.Id > 1))
            {
                output.AppendLine($"/interface vlan add vlan-id={vlan.Id} interface=bridge name={vlan.Name}");
            }

            // IP addresses
            foreach (var iface in Interfaces.Values)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    var cidr = MaskToCidr(iface.SubnetMask);
                    output.AppendLine($"/ip address add address={iface.IpAddress}/{cidr} interface={iface.Name}");
                }
            }

            // Running config additions
            if (RunningConfig.Length > 0)
            {
                output.Append(RunningConfig.ToString());
            }

            return output.ToString();
        }

        private string SimulatePingMikroTik(string destIp)
        {
            var output = new StringBuilder();
            output.AppendLine($"  SEQ HOST                                     SIZE TTL TIME  STATUS");

            // Check if destination is reachable
            bool reachable = IsReachable(destIp);

            // Simulate 5 ping attempts
            for (int i = 0; i < 5; i++)
            {
                if (reachable)
                {
                    output.AppendLine($"    {i} {destIp,-40} 56  64 0ms");
                }
                else
                {
                    output.AppendLine($"    {i} {destIp,-40}              timeout");
                }
            }

            output.AppendLine($"    sent=5 received={(reachable ? 5 : 0)} packet-loss={(reachable ? 0 : 100)}%");
            return output.ToString();
        }

        private bool IsReachable(string destIp)
        {
            // Check if IP is in any connected network
            foreach (var route in RoutingTable.Where(r => r.Protocol == "Connected"))
            {
                if (IsIpInNetwork(destIp, route.Network, route.Mask))
                {
                    return true;
                }
            }

            // Check if there's a route to the destination
            foreach (var route in RoutingTable)
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

        private int ExtractIntValue(string input, string key)
        {
            var value = ExtractValue(input, key);
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }

        private string ExtractValue(string input, string key)
        {
            var idx = input.IndexOf(key + "=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var substr = input.Substring(idx + key.Length + 1);
                var space = substr.IndexOf(' ');
                if (space < 0) space = substr.Length;
                return substr.Substring(0, space).Trim('"');
            }
            return "";
        }

        private string ExtractNumberParam(string input)
        {
            // Extract interface number (e.g., from "set 0" or "set ether1")
            var parts = input.Split(' ');
            for (int i = 2; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int num))
                {
                    // Convert number to interface name
                    var interfaces = this.Interfaces.Keys.ToList();
                    if (num < interfaces.Count)
                    {
                        return interfaces[num];
                    }
                }
                else if (this.Interfaces.ContainsKey(parts[i]))
                {
                    return parts[i];
                }
            }
            return null;
        }

        private string GetMacAddress(int index)
        {
            // Generate consistent MAC addresses
            return $"AA:BB:CC:00:0{index:X1}:00";
        }
    }
}
