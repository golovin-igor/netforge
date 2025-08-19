using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Interfaces;

namespace NetForge.Simulation.CliHandlers.Broadcom.Show
{
    /// <summary>
    /// Broadcom show command handler
    /// </summary>
    public class ShowCommandHandler : VendorAgnosticCliHandler
    {
        public ShowCommandHandler() : base("show", "Display device information")
        {
            AddAlias("sh");
            AddAlias("sho");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need show option");
            }
            
            var option = context.CommandParts[1];
            
            return option switch
            {
                "version" => HandleShowVersion(context),
                "interfaces" => HandleShowInterfaces(context),
                "interface" => HandleShowInterfaces(context), // Alias
                "arp" => HandleShowArp(context),
                "running-config" => HandleShowRunningConfig(context),
                "spanning-tree" => HandleShowSpanningTree(context),
                "evpn" => HandleShowEvpn(context),
                "mac" => HandleShowMac(context),
                "vlan" => HandleShowVlan(context),
                "ip" => HandleShowIp(context),
                "mlag" => HandleShowMlag(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid show option: {option}")
            };
        }
        
        private CliResult HandleShowVersion(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine($"Broadcom Network Device");
            output.AppendLine($"Device name: {device?.Name}");
            output.AppendLine($"Software version: 1.0");
            output.AppendLine($"Hardware: Generic");
            output.AppendLine($"Uptime: 1 day, 0 hours, 0 minutes");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowInterfaces(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var interfaces = device.GetAllInterfaces();
            
            output.AppendLine("Interface              IP-Address      Status    Protocol");
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";
                output.AppendLine($"{iface.Name,-22} {iface.IpAddress,-15} {status,-9} {protocol}");
            }
            
            return Success(output.ToString());
        }

        private CliResult HandleShowArp(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var table = device?.GetArpTableOutput();
            if (string.IsNullOrEmpty(table))
                return Success("ARP table is empty.\n");
            return Success(table);
        }
        
        private CliResult HandleShowRunningConfig(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var output = new StringBuilder();
            output.AppendLine("!");
            output.AppendLine("version 15.1");
            output.AppendLine("!");
            output.AppendLine($"hostname {device.Name}");
            output.AppendLine("!");
            
            // Add interface configurations
            var interfaces = device.GetAllInterfaces();
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                output.AppendLine($"interface {iface.Name}");
                if (!string.IsNullOrEmpty(iface.IpAddress) && iface.IpAddress != "0.0.0.0")
                {
                    output.AppendLine($" ip address {iface.IpAddress} {iface.SubnetMask}");
                }
                if (iface.IsUp)
                {
                    output.AppendLine(" no shutdown");
                }
                else
                {
                    output.AppendLine(" shutdown");
                }
            }
            
            output.AppendLine("!");
            output.AppendLine("end");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowSpanningTree(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("VLAN0001");
            output.AppendLine("  Spanning tree enabled protocol ieee");
            output.AppendLine("  Root ID    Priority    32769");
            output.AppendLine("             Address     aabb.cc00.0100");
            output.AppendLine("             This bridge is the root");
            output.AppendLine("             Hello Time   2 sec  Max Age 20 sec  Forward Delay 15 sec");
            output.AppendLine("");
            output.AppendLine("  Bridge ID  Priority    32769  (priority 32768 sys-id-ext 1)");
            output.AppendLine("             Address     aabb.cc00.0100");
            output.AppendLine("             Hello Time   2 sec  Max Age 20 sec  Forward Delay 15 sec");
            output.AppendLine("             Aging Time  300 sec");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowEvpn(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("EVPN Instance Information:");
            output.AppendLine("");
            output.AppendLine("Instance: 1");
            output.AppendLine("  VLAN: 100");
            output.AppendLine("  RD: 65001:100");
            output.AppendLine("  RT Import: 65001:100");
            output.AppendLine("  RT Export: 65001:100");
            output.AppendLine("  Status: Active");
            output.AppendLine("");
            output.AppendLine("Instance: 2");
            output.AppendLine("  VLAN: 200");
            output.AppendLine("  RD: 65001:200");
            output.AppendLine("  RT Import: 65001:200");
            output.AppendLine("  RT Export: 65001:200");
            output.AppendLine("  Status: Active");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowMac(CliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "address-table")
            {
                return HandleShowMacAddressTable(context);
            }
            
            return HandleShowMacAddressTable(context);
        }
        
        private CliResult HandleShowMacAddressTable(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Dynamic Address Count: 8");
            output.AppendLine("Secure Address Count: 0");
            output.AppendLine("Static Address (User-defined) Count: 0");
            output.AppendLine("System Self Address Count: 48");
            output.AppendLine("Total MAC addresses: 56");
            output.AppendLine("Maximum MAC addresses: 8192");
            output.AppendLine("");
            output.AppendLine("Vlan  Mac Address       Type    Port");
            output.AppendLine("----  -----------       ----    ----");
            output.AppendLine("100   aabb.cc00.0100    Dynamic Gi0/1");
            output.AppendLine("100   aabb.cc00.0200    Dynamic Gi0/2");
            output.AppendLine("200   aabb.cc00.0300    Dynamic Gi0/3");
            output.AppendLine("200   aabb.cc00.0400    Dynamic Gi0/4");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowVlan(CliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "brief")
            {
                return HandleShowVlanBrief(context);
            }
            
            return HandleShowVlanBrief(context);
        }
        
        private CliResult HandleShowVlanBrief(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("VLAN Name                             Status    Ports");
            output.AppendLine("---- -------------------------------- --------- -------------------------------");
            output.AppendLine("1    default                          active    Gi0/1, Gi0/2, Gi0/3, Gi0/4");
            output.AppendLine("100  VLAN0100                         active    Gi0/5, Gi0/6");
            output.AppendLine("200  VLAN0200                         active    Gi0/7, Gi0/8");
            output.AppendLine("1002 fddi-default                     act/unsup");
            output.AppendLine("1003 token-ring-default               act/unsup");
            output.AppendLine("1004 fddinet-default                  act/unsup");
            output.AppendLine("1005 trnet-default                    act/unsup");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIp(CliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need IP option");
            }
            
            var ipOption = context.CommandParts[2];
            
            return ipOption switch
            {
                "route" => HandleShowIpRoute(context),
                "interface" => HandleShowIpInterface(context),
                "arp" => HandleShowArp(context), // Reuse existing ARP handler
                "bgp" => HandleShowIpBgp(context),
                "ospf" => HandleShowIpOspf(context),
                _ => Error(CliErrorType.InvalidCommand, 
                    $"% Invalid IP show option: {ipOption}")
            };
        }
        
        private CliResult HandleShowIpRoute(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Codes: C - connected, S - static, R - RIP, M - mobile, B - BGP");
            output.AppendLine("       D - EIGRP, EX - EIGRP external, O - OSPF, IA - OSPF inter area");
            output.AppendLine("       N1 - OSPF NSSA external type 1, N2 - OSPF NSSA external type 2");
            output.AppendLine("       E1 - OSPF external type 1, E2 - OSPF external type 2");
            output.AppendLine("       i - IS-IS, su - IS-IS summary, L1 - IS-IS level-1, L2 - IS-IS level-2");
            output.AppendLine("       ia - IS-IS inter area, * - candidate default, U - per-user static route");
            output.AppendLine("       o - ODR, P - periodic downloaded static route");
            output.AppendLine("");
            output.AppendLine("Gateway of last resort is not set");
            output.AppendLine("");
            output.AppendLine("C    192.168.1.0/24 is directly connected, GigabitEthernet0/1");
            output.AppendLine("S    10.0.0.0/8 [1/0] via 192.168.1.1");
            output.AppendLine("O    172.16.0.0/16 [110/2] via 192.168.1.2, 00:05:23, GigabitEthernet0/1");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpInterface(CliContext context)
        {
            // Check for "brief" sub-command
            if (context.CommandParts.Length > 3 && context.CommandParts[3] == "brief")
            {
                return HandleShowIpInterfaceBrief(context);
            }
            
            return HandleShowIpInterfaceBrief(context);
        }
        
        private CliResult HandleShowIpInterfaceBrief(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var interfaces = device.GetAllInterfaces();
            
            output.AppendLine("Interface                  IP-Address      OK? Method Status                Protocol");
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";
                output.AppendLine($"{iface.Name,-26} {iface.IpAddress,-15} YES NVRAM  {status,-21} {protocol}");
            }
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpBgp(CliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 3)
            {
                var subCommand = context.CommandParts[3];
                return subCommand switch
                {
                    "summary" => HandleShowIpBgpSummary(context),
                    _ => Error(CliErrorType.InvalidCommand, 
                        $"% Invalid BGP show option: {subCommand}")
                };
            }
            
            var output = new StringBuilder();
            output.AppendLine("BGP table version is 1, local router ID is 192.168.1.1");
            output.AppendLine("Status codes: s suppressed, d damped, h history, * valid, > best, i - internal,");
            output.AppendLine("              r RIB-failure, S Stale, m multipath, b backup-path, f RT-Filter,");
            output.AppendLine("              x best-external, a additional-path, c RIB-compressed,");
            output.AppendLine("Origin codes: i - IGP, e - EGP, ? - incomplete");
            output.AppendLine("");
            output.AppendLine("   Network          Next Hop            Metric LocPrf Weight Path");
            output.AppendLine("*> 192.168.1.0/24   0.0.0.0                  0         32768 i");
            output.AppendLine("*> 10.0.0.0/8       192.168.1.2              0             0 65002 i");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpBgpSummary(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("BGP router identifier 192.168.1.1, local AS number 65001");
            output.AppendLine("BGP table version is 1, main routing table version 1");
            output.AppendLine("2 network entries using 496 bytes of memory");
            output.AppendLine("2 path entries using 160 bytes of memory");
            output.AppendLine("");
            output.AppendLine("Neighbor        V           AS MsgRcvd MsgSent   TblVer  InQ OutQ Up/Down  State/PfxRcd");
            output.AppendLine("192.168.1.2     4        65002      25      27        1    0    0 00:15:23        1");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpOspf(CliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 3)
            {
                var subCommand = context.CommandParts[3];
                return subCommand switch
                {
                    "neighbor" => HandleShowIpOspfNeighbor(context),
                    "database" => HandleShowIpOspfDatabase(context),
                    _ => Error(CliErrorType.InvalidCommand, 
                        $"% Invalid OSPF show option: {subCommand}")
                };
            }
            
            var output = new StringBuilder();
            output.AppendLine("Routing Process \"ospf 1\" with ID 192.168.1.1");
            output.AppendLine("Process has been running for 1 day, 5 hours, 23 minutes");
            output.AppendLine("Supports only single TOS(TOS0) routes");
            output.AppendLine("SPF schedule delay 5 secs, Hold time between two SPFs 10 secs");
            output.AppendLine("");
            output.AppendLine("Area BACKBONE(0):");
            output.AppendLine("    Number of interfaces in this area is 1");
            output.AppendLine("    Number of fully adjacent neighbors is 1");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpOspfNeighbor(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Neighbor ID     Pri   State           Dead Time   Address         Interface");
            output.AppendLine("192.168.1.2       1   Full/DR         00:00:39    192.168.1.2     GigabitEthernet0/1");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpOspfDatabase(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("            OSPF Router with ID (192.168.1.1) (Process ID 1)");
            output.AppendLine("");
            output.AppendLine("                Router Link States (Area 0)");
            output.AppendLine("");
            output.AppendLine("Link ID         ADV Router      Age         Seq#       Checksum Link count");
            output.AppendLine("192.168.1.1     192.168.1.1     1234        0x80000002 0x005A3B  1");
            output.AppendLine("192.168.1.2     192.168.1.2     987         0x80000001 0x004C2A  1");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowMlag(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("MLAG Configuration:");
            output.AppendLine("Domain ID          : 1");
            output.AppendLine("Local interface    : Vlan4094");
            output.AppendLine("Peer address       : 192.168.1.2");
            output.AppendLine("Peer-link          : Port-channel10");
            output.AppendLine("Peer-config        : consistent");
            output.AppendLine("Peer-link status   : up");
            output.AppendLine("");
            output.AppendLine("MLAG Status:");
            output.AppendLine("State              : Active");
            output.AppendLine("Negotiation status : Connected");
            output.AppendLine("Peer-link status   : Up");
            output.AppendLine("Local int status   : Up");
            output.AppendLine("");
            output.AppendLine("MLAG Ports:");
            output.AppendLine("Mlag-id  Local ports   Peer ports    Status");
            output.AppendLine("-------  ------------  ------------  ------");
            output.AppendLine("1        Po1           Po1           Active");
            output.AppendLine("2        Po2           Po2           Active");
            
            return Success(output.ToString());
        }
    }
}
