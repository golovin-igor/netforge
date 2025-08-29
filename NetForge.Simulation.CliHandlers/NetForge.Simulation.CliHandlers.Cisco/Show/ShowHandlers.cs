using System.Text;
using System.Linq;
using System.Collections.Generic;
using NetForge.Interfaces.Cli;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.CliHandlers.Cisco.Show
{
    /// <summary>
    /// Main Cisco show command handler
    /// </summary>
    public class ShowCommandHandler : VendorAgnosticCliHandler
    {
        public ShowCommandHandler() : base("show", "Display device information")
        {
            AddAlias("sh");
            AddAlias("sho");

            // Add sub-handlers for various show commands
            AddSubHandler("running-config", new ShowRunningConfigHandler());
            AddSubHandler("run", new ShowRunningConfigHandler());
            AddSubHandler("version", new ShowVersionHandler());
            AddSubHandler("interfaces", new ShowInterfacesHandler());
            AddSubHandler("interface", new ShowInterfacesHandler());
            AddSubHandler("vlan", new ShowVlanHandler());
            AddSubHandler("arp", new ShowArpHandler());
            AddSubHandler("ip", new ShowIpHandler());
            AddSubHandler("cdp", new ShowCdpHandler());
            AddSubHandler("mac", new ShowMacHandler());
            AddSubHandler("spanning-tree", new ShowSpanningTreeHandler());
            AddSubHandler("flash", new ShowFlashHandler());
            AddSubHandler("memory", new ShowMemoryHandler());
            AddSubHandler("processes", new ShowProcessesHandler());
            AddSubHandler("clock", new ShowClockHandler());
            AddSubHandler("environment", new ShowEnvironmentHandler());
            AddSubHandler("inventory", new ShowInventoryHandler());
            AddSubHandler("ssh", new ShowSshHandler());
            AddSubHandler("telnet", new ShowTelnetHandler());
        }

        public override List<string> GetCompletions(ICliContext context)
        {
            // Use the enhanced base implementation with vendor-specific extensions
            var completions = base.GetCompletions(context);

            // Add Cisco-specific completions for show command
            if (context.CommandParts.Length > 1)
            {
                var subCommand = context.CommandParts[1].ToLower();

                // Provide context-aware completions for common patterns
                switch (subCommand)
                {
                    case "ip":
                        if (context.CommandParts.Length == 2)
                            completions.AddRange(new[] { "route", "arp", "interface", "ospf", "bgp", "eigrp", "rip", "protocols" });
                        break;
                    case "interfaces":
                        if (context.CommandParts.Length == 2)
                            completions.AddRange(GetInterfaceNames(context));
                        break;
                    case "vlan":
                        if (context.CommandParts.Length == 2)
                            completions.AddRange(new[] { "brief", "id" });
                        break;
                    case "spanning-tree":
                        if (context.CommandParts.Length == 2)
                            completions.AddRange(new[] { "brief", "root", "interface" });
                        break;
                    case "cdp":
                        if (context.CommandParts.Length == 2)
                            completions.AddRange(new[] { "neighbors", "entry", "interface", "traffic" });
                        break;
                    case "mac":
                        if (context.CommandParts.Length == 2)
                            completions.AddRange(new[] { "address-table" });
                        break;
                }
            }

            return completions.Distinct().OrderBy(c => c).ToList();
        }

        private List<string> GetInterfaceNames(ICliContext context)
        {
            var interfaces = new List<string>();

            // Common Cisco interface names and aliases
            interfaces.AddRange(new[] {
                "ethernet0/0", "ethernet0/1", "ethernet0/2", "ethernet0/3",
                "e0/0", "e0/1", "e0/2", "e0/3", // Ethernet aliases
                "fastethernet0/0", "fastethernet0/1", "fastethernet0/2", "fastethernet0/3",
                "fa0/0", "fa0/1", "fa0/2", "fa0/3", // FastEthernet aliases
                "gigabitethernet0/0", "gigabitethernet0/1", "gigabitethernet0/2", "gigabitethernet0/3",
                "gi0/0", "gi0/1", "gi0/2", "gi0/3", // GigabitEthernet aliases
                "serial0/0", "serial0/1", "serial0/2", "serial0/3",
                "s0/0", "s0/1", "s0/2", "s0/3", // Serial aliases
                "loopback0", "loopback1", "loopback2", "loopback3",
                "lo0", "lo1", "lo2", "lo3", // Loopback aliases
                "vlan1", "vlan10", "vlan20", "vlan30", "vlan100"
            });

            return interfaces;
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // If no sub-command specified, return help
            if (context.CommandParts.Length == 1)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            // This shouldn't be reached if sub-handlers are properly configured
            return Error(CliErrorType.InvalidCommand,
                GetVendorError(context, "invalid_command"));
        }
    }

    /// <summary>
    /// Show running configuration handler
    /// </summary>
    public class ShowRunningConfigHandler : VendorAgnosticCliHandler
    {
        public ShowRunningConfigHandler() : base("running-config", "Display current operating configuration")
        {
            AddAlias("run");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // Use vendor-agnostic method to get running config
            var runningConfig = GetRunningConfig(context);
            return Success(runningConfig);
        }
    }

    /// <summary>
    /// Show version handler
    /// </summary>
    public class ShowVersionHandler() : VendorAgnosticCliHandler("version", "Display system hardware and software status")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Cisco IOS Software, Router Software (C2900-UNIVERSALK9-M), Version 15.7(3)M5, RELEASE SOFTWARE (fc1)");
            output.AppendLine("Technical Support: http://www.cisco.com/techsupport");
            output.AppendLine("Copyright (c) 1986-2020 by Cisco Systems, Inc.");
            output.AppendLine("Compiled Thu 16-Jan-20 01:02 by prod_rel_team\n");
            output.AppendLine($"ROM: System Bootstrap, Version 15.0(1r)M16, RELEASE SOFTWARE (fc1)\n");
            output.AppendLine($"{context.Device.Name} uptime is 1 week, 2 days, 3 hours, 24 minutes");
            output.AppendLine("System returned to ROM by power-on");
            output.AppendLine("System image file is \"flash:c2900-universalk9-mz.SPA.157-3.M5.bin\"\n");
            output.AppendLine("Cisco CISCO2911/K9 (revision 1.0) with 491520K/32768K bytes of memory.");
            output.AppendLine("Processor board ID FTX1234ABCD");
            output.AppendLine("3 Gigabit Ethernet interfaces");
            output.AppendLine("256K bytes of non-volatile configuration memory.");
            output.AppendLine("255488K bytes of ATA System CompactFlash 0 (Read/Write)\n");
            output.AppendLine("Configuration register is 0x2102");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show interfaces handler
    /// </summary>
    public class ShowInterfacesHandler : VendorAgnosticCliHandler
    {
        public ShowInterfacesHandler() : base("interfaces", "Display interface status and configuration")
        {
            AddAlias("interface");
            AddAlias("int");
            AddSubHandler("status", new ShowInterfacesStatusHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();

            if (context.CommandParts.Length > 1)
            {
                // Show specific interface
                var interfaceName = string.Join(" ", context.CommandParts.Skip(1));
                var formattedName = FormatInterfaceName(context, interfaceName);

                // Use vendor-agnostic method to get interface info
                var interfaces = context.Device.GetAllInterfaces();
                if (interfaces.TryGetValue(formattedName, out var iface))
                {
                    output.AppendLine($"{iface.Name} is {(iface.IsUp ? "up" : "down")}, line protocol is {(iface.IsUp ? "up" : "down")}");
                    output.AppendLine($"  Hardware is FastEthernet, address is aabb.cc00.0100");
                    if (!string.IsNullOrEmpty(iface.Description))
                        output.AppendLine($"  Description: {iface.Description}");
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                        output.AppendLine($"  Internet address is {iface.IpAddress}/24");
                    output.AppendLine($"  MTU 1500 bytes, BW 100000 Kbit/sec, DLY 100 usec,");
                    output.AppendLine($"     reliability 255/255, txload 1/255, rxload 1/255");
                    output.AppendLine($"  Encapsulation ARPA, loopback not set");
                    output.AppendLine($"  5 minute input rate 0 bits/sec, 0 packets/sec");
                    output.AppendLine($"  5 minute output rate 0 bits/sec, 0 packets/sec");
                    output.AppendLine($"     {iface.RxPackets} packets input, {iface.RxBytes} bytes");
                    output.AppendLine($"     {iface.TxPackets} packets output, {iface.TxBytes} bytes");
                }
                else
                {
                    return Error(CliErrorType.InvalidParameter,
                        $"% Invalid interface name: {interfaceName}");
                }
            }
            else
            {
                // Show all interfaces
                var allInterfaces = context.Device.GetAllInterfaces();
                foreach (var iface in allInterfaces.Values)
                {
                    output.AppendLine($"{iface.Name} is {(iface.IsUp ? "up" : "down")}, line protocol is {(iface.IsUp ? "up" : "down")}");
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                        output.AppendLine($"  Internet address is {iface.IpAddress}/24");
                    output.AppendLine();
                }
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show interfaces status handler
    /// </summary>
    public class ShowInterfacesStatusHandler() : VendorAgnosticCliHandler("status", "Display interface status summary")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Port      Name               Status       Vlan       Duplex  Speed Type");
            output.AppendLine("Gi0/0                        connected    1          a-full  a-1000 10/100/1000BaseTX");
            output.AppendLine("Gi0/1                        notconnect   1            auto   auto 10/100/1000BaseTX");
            output.AppendLine("Gi0/2                        notconnect   1            auto   auto 10/100/1000BaseTX");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show VLAN handler
    /// </summary>
    public class ShowVlanHandler : VendorAgnosticCliHandler
    {
        public ShowVlanHandler() : base("vlan", "Display VLAN information")
        {
            AddSubHandler("brief", new ShowVlanBriefHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // Default to brief output if no sub-command
            if (context.CommandParts.Length == 1)
            {
                var briefHandler = new ShowVlanBriefHandler();
                var briefContext = new CliContext(context.Device, new[] { "brief" }, context.FullCommand);
                return briefHandler.Handle(briefContext);
            }

            return Error(CliErrorType.InvalidCommand,
                GetVendorError(context, "invalid_command"));
        }
    }

    /// <summary>
    /// Show VLAN brief handler
    /// </summary>
    public class ShowVlanBriefHandler() : VendorAgnosticCliHandler("brief", "Display VLAN brief information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("VLAN Name                             Status    Ports");
            output.AppendLine("---- -------------------------------- --------- -------------------------------");

            // Get VLANs from device using reflection to call device-specific methods
            try
            {
                var deviceType = context.Device.GetType();
                var getVlansMethod = deviceType.GetMethod("GetVlans");
                if (getVlansMethod != null)
                {
                    var vlans = getVlansMethod.Invoke(context.Device, null);
                    if (vlans is IEnumerable<dynamic> vlanList)
                    {
                        foreach (var vlan in vlanList)
                        {
                            var vlanId = vlan.Id;
                            var vlanName = vlan.Name ?? "default";
                            var status = vlan.Active ? "active" : "suspend";

                            // Get interfaces assigned to this VLAN
                            var ports = "";
                            var allInterfaces = context.Device.GetAllInterfaces();
                            var vlanInterfaces = allInterfaces.Values
                                .Where(i => i.VlanId == vlanId)
                                .Select(i => FormatInterfaceName(context, i.Name.Replace("GigabitEthernet", "Gi")))
                                .ToList();

                            if (vlanInterfaces.Any())
                            {
                                ports = string.Join(", ", vlanInterfaces);
                            }

                            output.AppendLine($"{vlanId,-4} {vlanName,-32} {status,-9} {ports}");
                        }
                    }
                }
                else
                {
                    // Fallback to default VLANs if method not found
                    output.AppendLine("1    default                          active    ");
                }
            }
            catch
            {
                // Fallback to default VLANs on error
                output.AppendLine("1    default                          active    ");
            }

            // Always show default system VLANs
            output.AppendLine("1002 fddi-default                     act/unsup ");
            output.AppendLine("1003 token-ring-default               act/unsup ");
            output.AppendLine("1004 fddinet-default                  act/unsup ");
            output.AppendLine("1005 trnet-default                    act/unsup ");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show IP handler
    /// </summary>
    public class ShowIpHandler : VendorAgnosticCliHandler
    {
        public ShowIpHandler() : base("ip", "Display IP information")
        {
            AddSubHandler("route", new ShowIpRouteHandler());
            AddSubHandler("interface", new ShowIpInterfaceHandler());
            AddSubHandler("ospf", new ShowIpOspfHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand,
                GetVendorError(context, "incomplete_command"));
        }
    }

    /// <summary>
    /// Show IP route handler
    /// </summary>
    public class ShowIpRouteHandler() : VendorAgnosticCliHandler("route", "Display IP routing table")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Codes: L - local, C - connected, S - static, R - RIP, M - mobile, B - BGP");
            output.AppendLine("       D - EIGRP, EX - EIGRP external, O - OSPF, IA - OSPF inter area");
            output.AppendLine("       N1 - OSPF NSSA external type 1, N2 - OSPF NSSA external type 2");
            output.AppendLine("       E1 - OSPF external type 1, E2 - OSPF external type 2");
            output.AppendLine("       i - IS-IS, su - IS-IS summary, L1 - IS-IS level-1, L2 - IS-IS level-2");
            output.AppendLine("       ia - IS-IS inter area, * - candidate default, U - per-user static route");
            output.AppendLine("       o - ODR, P - periodic downloaded static route, H - NHRP, l - LISP");
            output.AppendLine("       a - application route");
            output.AppendLine("       + - replicated route, % - next hop override\n");

            output.AppendLine("Gateway of last resort is not set\n");

            // Use vendor-agnostic method to get routes
            var routes = context.Device.GetRoutingTable();
            foreach (var route in routes.OrderBy(r => r.Network))
            {
                var code = "C"; // Default to connected

                // Simulate route display - some properties may not exist
                var network = route.Network ?? "0.0.0.0";
                var nextHop = route.NextHop ?? "0.0.0.0";
                var interfaceName = route.Interface ?? "Unknown";

                output.AppendLine($"{code}        {network}/24 is directly connected, {interfaceName}");
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show IP interface handler
    /// </summary>
    public class ShowIpInterfaceHandler : VendorAgnosticCliHandler
    {
        public ShowIpInterfaceHandler() : base("interface", "Display IP interface status")
        {
            AddSubHandler("brief", new ShowIpInterfaceBriefHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // Default to brief if no additional params
            if (context.CommandParts.Length == 2)
            {
                var briefHandler = new ShowIpInterfaceBriefHandler();
                return briefHandler.Handle(context);
            }

            return Error(CliErrorType.InvalidCommand,
                GetVendorError(context, "invalid_command"));
        }
    }

    /// <summary>
    /// Show IP interface brief handler
    /// </summary>
    public class ShowIpInterfaceBriefHandler() : VendorAgnosticCliHandler("brief", "Display brief IP interface status")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Interface                  IP-Address      OK? Method Status                Protocol");

            // Use vendor-agnostic method to get interfaces
            var allInterfaces = context.Device.GetAllInterfaces();
            foreach (var iface in allInterfaces.Values.OrderBy(i => i.Name))
            {
                var ipAddress = string.IsNullOrEmpty(iface.IpAddress) ? "unassigned" : iface.IpAddress;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";

                output.AppendLine($"{iface.Name,-26} {ipAddress,-15} YES manual {status,-21} {protocol}");
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show IP OSPF handler
    /// </summary>
    public class ShowIpOspfHandler : VendorAgnosticCliHandler
    {
        public ShowIpOspfHandler() : base("ospf", "Display OSPF information")
        {
            AddSubHandler("neighbor", new ShowIpOspfNeighborHandler());
            AddSubHandler("database", new ShowIpOspfDatabaseHandler());
            AddSubHandler("interface", new ShowIpOspfInterfaceHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand,
                GetVendorError(context, "incomplete_command"));
        }
    }

    /// <summary>
    /// Show IP OSPF neighbor handler
    /// </summary>
    public class ShowIpOspfNeighborHandler() : VendorAgnosticCliHandler("neighbor", "Display OSPF neighbor information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Neighbor ID     Pri   State           Dead Time   Address         Interface");

            // Simulate OSPF neighbors - in a real implementation this would come from device state
            // For test purposes, we'll simulate a neighbor
            output.AppendLine("2.2.2.2           1   FULL/  -        00:00:35    10.0.0.2        GigabitEthernet0/0");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show IP OSPF database handler
    /// </summary>
    public class ShowIpOspfDatabaseHandler() : VendorAgnosticCliHandler("database", "Display OSPF database information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("            OSPF Router with ID (1.1.1.1) (Process ID 1)");
            output.AppendLine("");
            output.AppendLine("                Router Link States (Area 0)");
            output.AppendLine("");
            output.AppendLine("Link ID         ADV Router      Age         Seq#       Checksum Link count");
            output.AppendLine("1.1.1.1         1.1.1.1         123         0x80000001 0x1234   1");
            output.AppendLine("2.2.2.2         2.2.2.2         124         0x80000001 0x5678   1");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show IP OSPF interface handler
    /// </summary>
    public class ShowIpOspfInterfaceHandler() : VendorAgnosticCliHandler("interface", "Display OSPF interface information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("GigabitEthernet0/0 is up, line protocol is up");
            output.AppendLine("  Internet Address 10.0.0.1/30, Area 0, Attached via Network Statement");
            output.AppendLine("  Process ID 1, Router ID 1.1.1.1, Network Type BROADCAST, Cost: 1");
            output.AppendLine("  Topology-MTID    Cost    Disabled    Shutdown      Topology Name");
            output.AppendLine("        0           1         no          no            Base");
            output.AppendLine("  Enabled by interface config, including secondary ip addresses");
            output.AppendLine("  Transmit Delay is 1 sec, State DR, Priority 1");
            output.AppendLine("  Designated Router (ID) 1.1.1.1, Interface address 10.0.0.1");
            output.AppendLine("  Backup Designated router (ID) 2.2.2.2, Interface address 10.0.0.2");
            output.AppendLine("  Timer intervals configured, Hello 10, Dead 40, Wait 40, Retransmit 5");
            output.AppendLine("    oob-resync timeout 40");
            output.AppendLine("    Hello due in 00:00:08");
            output.AppendLine("  Supports Link-local Signaling (LLS)");
            output.AppendLine("  Cisco NSF helper support enabled");
            output.AppendLine("  IETF NSF helper support enabled");
            output.AppendLine("  Index 1/1, flood queue length 0");
            output.AppendLine("  Next 0x0(0)/0x0(0)");
            output.AppendLine("  Last flood scan length is 1, maximum is 1");
            output.AppendLine("  Last flood scan time is 0 msec, maximum is 0 msec");
            output.AppendLine("  Neighbor Count is 1, Adjacent neighbor count is 1");
            output.AppendLine("    Adjacent with neighbor 2.2.2.2  (Backup Designated Router)");
            output.AppendLine("  Suppress hello for 0 neighbor(s)");

            return Success(output.ToString());
        }
    }
    /// <summary>
    /// Show ARP table handler
    /// </summary>
    public class ShowArpHandler() : VendorAgnosticCliHandler("arp", "Display ARP table")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device;
            var output = device?.GetArpTableOutput();
            if (string.IsNullOrEmpty(output))
            {
                return Success("ARP table is empty.\n");
            }

            return Success(output);
        }
    }

    /// <summary>
    /// Show CDP handler
    /// </summary>
    public class ShowCdpHandler : VendorAgnosticCliHandler
    {
        public ShowCdpHandler() : base("cdp", "Display CDP information")
        {
            AddSubHandler("neighbors", new ShowCdpNeighborsHandler());
            AddSubHandler("neighbor", new ShowCdpNeighborsHandler());
            AddSubHandler("interface", new ShowCdpInterfaceHandler());
            AddSubHandler("entry", new ShowCdpEntryHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand,
                GetVendorError(context, "incomplete_command"));
        }
    }

    /// <summary>
    /// Show CDP neighbors handler
    /// </summary>
    public class ShowCdpNeighborsHandler : VendorAgnosticCliHandler
    {
        public ShowCdpNeighborsHandler() : base("neighbors", "Display CDP neighbors")
        {
            AddAlias("neighbor");
            AddSubHandler("detail", new ShowCdpNeighborsDetailHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Capability Codes: R - Router, T - Trans Bridge, B - Source Route Bridge");
            output.AppendLine("                  S - Switch, H - Host, I - IGMP, r - Repeater");
            output.AppendLine();
            output.AppendLine("Device ID        Local Intrfce     Holdtme    Capability  Platform  Port ID");

            // In test environment, simulate CDP neighbors with sample data
            var device = context.Device;
            if (device?.ParentNetwork != null)
            {
                // Get all devices in the network except this one
                var allDevices = device.ParentNetwork.GetAllDevices();
                foreach (var otherDevice in allDevices)
                {
                    if (otherDevice.Name != device.Name)
                    {
                        output.AppendLine($"{otherDevice.Name,-16} Gig 0/0           120        R S I     C2900     Gig 0/0");
                    }
                }
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show CDP neighbors detail handler
    /// </summary>
    public class ShowCdpNeighborsDetailHandler() : VendorAgnosticCliHandler("detail", "Display detailed CDP neighbor information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Device ID: R2");
            output.AppendLine("Entry address(es): ");
            output.AppendLine("  IP address: 10.0.0.2");
            output.AppendLine("Platform: Cisco 2900,  Capabilities: Router Switch IGMP");
            output.AppendLine("Interface: GigabitEthernet0/0,  Port ID (outgoing port): GigabitEthernet0/0");
            output.AppendLine("Holdtime : 120 sec");
            output.AppendLine();
            output.AppendLine("Version :");
            output.AppendLine("Cisco IOS Software, C2900 Software (C2900-UNIVERSALK9-M), Version 15.7(3)M5");
            output.AppendLine();
            output.AppendLine("advertisement version: 2");
            output.AppendLine("Protocol Hello:  OUI=0x00000C, Protocol ID=0x0112; payload len=27, value=00000000FFFFFFFF010221FF0000000000000000000000000000");
            output.AppendLine("VTP Management Domain: ''");
            output.AppendLine("Native VLAN: 1");
            output.AppendLine("Duplex: full");
            output.AppendLine("Power Available TLV:");
            output.AppendLine();

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show CDP interface handler
    /// </summary>
    public class ShowCdpInterfaceHandler() : VendorAgnosticCliHandler("interface", "Display CDP interface information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("GigabitEthernet0/0 is up, line protocol is up");
            output.AppendLine("  Encapsulation ARPA");
            output.AppendLine("  Sending CDP packets every 60 seconds");
            output.AppendLine("  Holdtime is 180 seconds");
            output.AppendLine();

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show CDP entry handler
    /// </summary>
    public class ShowCdpEntryHandler() : VendorAgnosticCliHandler("entry", "Display CDP entry information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: show cdp entry <device-name>");
            }

            var deviceName = context.CommandParts[1];
            var output = new StringBuilder();
            output.AppendLine($"Device ID: {deviceName}");
            output.AppendLine("Entry address(es): ");
            output.AppendLine("  IP address: 10.0.0.2");
            output.AppendLine("Platform: Cisco 2900,  Capabilities: Router Switch IGMP");
            output.AppendLine("Interface: GigabitEthernet0/0,  Port ID (outgoing port): GigabitEthernet0/0");
            output.AppendLine("Holdtime : 120 sec");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show MAC handler
    /// </summary>
    public class ShowMacHandler : VendorAgnosticCliHandler
    {
        public ShowMacHandler() : base("mac", "Display MAC address information")
        {
            AddSubHandler("address-table", new ShowMacAddressTableHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand,
                GetVendorError(context, "incomplete_command"));
        }
    }

    /// <summary>
    /// Show MAC address table handler
    /// </summary>
    public class ShowMacAddressTableHandler() : VendorAgnosticCliHandler("address-table", "Display MAC address table")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("          Mac Address Table");
            output.AppendLine("-------------------------------------------");
            output.AppendLine();
            output.AppendLine("Vlan    Mac Address       Type        Ports");
            output.AppendLine("----    -----------       --------    -----");
            output.AppendLine("   1    0001.0001.0001    DYNAMIC     Gi0/1");
            output.AppendLine("   1    0002.0002.0002    DYNAMIC     Gi0/2");
            output.AppendLine("Total Mac Addresses for this criterion: 2");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show spanning tree handler
    /// </summary>
    public class ShowSpanningTreeHandler() : VendorAgnosticCliHandler("spanning-tree", "Display spanning tree information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("VLAN0001");
            output.AppendLine("  Spanning tree enabled protocol ieee");
            output.AppendLine("  Root ID    Priority    32769");
            output.AppendLine("             Address     0001.0001.0001");
            output.AppendLine("             This bridge is the root");
            output.AppendLine("             Hello Time   2 sec  Max Age 20 sec  Forward Delay 15 sec");
            output.AppendLine();
            output.AppendLine("  Bridge ID  Priority    32769  (priority 32768 sys-id-ext 1)");
            output.AppendLine("             Address     0001.0001.0001");
            output.AppendLine("             Hello Time   2 sec  Max Age 20 sec  Forward Delay 15 sec");
            output.AppendLine("             Aging Time  300 sec");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show flash handler
    /// </summary>
    public class ShowFlashHandler() : VendorAgnosticCliHandler("flash", "Display flash file system")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Directory of flash:/");
            output.AppendLine();
            output.AppendLine("    1  -rw-   124000000  Dec 1 1993 00:01:00 +00:00  isr4300-universalk9.16.12.04.SPA.bin");
            output.AppendLine("    2  -rw-        2048  Dec 1 1993 00:01:00 +00:00  info");
            output.AppendLine();
            output.AppendLine("255926272 bytes total (131926272 bytes free)");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show memory handler
    /// </summary>
    public class ShowMemoryHandler() : VendorAgnosticCliHandler("memory", "Display memory information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Head    Total(b)     Used(b)     Free(b)   Lowest(b)  Largest(b)");
            output.AppendLine("Processor   4194304     2097152     2097152     1048576     2097152");
            output.AppendLine("      I/O   2097152      524288     1572864      524288     1572864");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show processes handler
    /// </summary>
    public class ShowProcessesHandler() : VendorAgnosticCliHandler("processes", "Display process information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("CPU utilization for five seconds: 1%/0%; one minute: 1%; five minutes: 1%");
            output.AppendLine("PID Runtime(ms)     Invoked      uSecs   5Sec   1Min   5Min TTY Process");
            output.AppendLine("  1        156          12      13000  0.00%  0.00%  0.00%   0 Chunk Manager");
            output.AppendLine("  2         64           8       8000  0.00%  0.00%  0.00%   0 Load Meter");
            output.AppendLine("  3        432          32      13500  0.00%  0.00%  0.00%   0 Exec");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show clock handler
    /// </summary>
    public class ShowClockHandler() : VendorAgnosticCliHandler("clock", "Display system clock")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var currentTime = DateTime.Now;
            var output = $"*{currentTime:HH:mm:ss.fff} UTC {currentTime:ddd MMM d yyyy}";

            return Success(output);
        }
    }

    /// <summary>
    /// Show environment handler
    /// </summary>
    public class ShowEnvironmentHandler() : VendorAgnosticCliHandler("environment", "Display environmental information")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("Number of Critical alarms:  0");
            output.AppendLine("Number of Major alarms:     0");
            output.AppendLine("Number of Minor alarms:     0");
            output.AppendLine();
            output.AppendLine("Slot        Sensor       Current State       Reading        Threshold(Minor,Major,Critical,Shutdown)");
            output.AppendLine("R0          Temp: Inlet  Normal              25 Celsius     (42,52,62,72)(Celsius)");
            output.AppendLine("R0          Temp: Outlet Normal              35 Celsius     (65,75,85,95)(Celsius)");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show inventory handler
    /// </summary>
    public class ShowInventoryHandler() : VendorAgnosticCliHandler("inventory", "Display hardware inventory")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var output = new StringBuilder();
            output.AppendLine("NAME: \"Chassis\", DESCR: \"Cisco ISR4331 Integrated Services Router\"");
            output.AppendLine("PID: ISR4331/K9        , VID: V03  , SN: FDO12345678");
            output.AppendLine();
            output.AppendLine("NAME: \"module R0\", DESCR: \"Cisco ISR4331 Route Processor\"");
            output.AppendLine("PID: ISR4331/K9        , VID:      , SN:");
            output.AppendLine();
            output.AppendLine("NAME: \"NIM subslot 0/0\", DESCR: \"Front Panel 3 ports Gigabitethernet Module\"");
            output.AppendLine("PID: NIM-ES2-4         , VID: V01  , SN: FOC12345678");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show SSH handler
    /// </summary>
    public class ShowSshHandler() : VendorAgnosticCliHandler("ssh", "Display SSH status and sessions")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            var device = context.Device;
            var protocolService = context.GetService<IProtocolService>();

            if (protocolService == null)
            {
                return Error(CliErrorType.ExecutionError, "Protocol service not available");
            }

            var output = new StringBuilder();
            output.AppendLine("SSH server status:");

            // Get SSH protocol and state
            IDeviceProtocol sshProtocol = protocolService.GetProtocol(NetworkProtocolType.SSH);
            if (sshProtocol == null)
            {
                output.AppendLine("SSH server: Not configured");
                return Success(output.ToString());
            }

            var sshState = sshProtocol.GetState();
            var sshConfig = sshProtocol.GetConfiguration();

            if (sshState == null)
            {
                output.AppendLine("SSH server: Configuration error");
                return Success(output.ToString());
            }

            // Display SSH status
            output.AppendLine($"SSH server: {(sshState.IsActive ? "Enabled" : "Disabled")}");

            if (sshState.IsActive)
            {
                var stateData = sshState.GetStateData();

                output.AppendLine($"SSH version: {stateData.GetValueOrDefault("ProtocolVersion", "2")}");
                output.AppendLine($"Listening port: {stateData.GetValueOrDefault("ListeningPort", "22")}");
                output.AppendLine($"Active sessions: {stateData.GetValueOrDefault("ActiveSessions", "0")}");
                output.AppendLine($"Total connections: {stateData.GetValueOrDefault("TotalConnections", "0")}");
                output.AppendLine($"Successful authentications: {stateData.GetValueOrDefault("SuccessfulAuthentications", "0")}");
                output.AppendLine($"Failed authentications: {stateData.GetValueOrDefault("FailedAuthentications", "0")}");

                var hostKeyFingerprint = stateData.GetValueOrDefault("HostKeyFingerprint", "");
                if (!string.IsNullOrEmpty(hostKeyFingerprint.ToString()))
                {
                    output.AppendLine($"Host key fingerprint: {hostKeyFingerprint}");
                }

                var lastActivity = stateData.GetValueOrDefault("LastActivity", DateTime.MinValue);
                if (lastActivity is DateTime lastActivityTime && lastActivityTime != DateTime.MinValue)
                {
                    output.AppendLine($"Last activity: {lastActivityTime:yyyy-MM-dd HH:mm:ss}");
                }
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Show Telnet handler
    /// </summary>
    public class ShowTelnetHandler() : VendorAgnosticCliHandler("telnet", "Display Telnet status and sessions")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            var device = context.Device;
            var protocolService = context.GetService<IProtocolService>();

            if (protocolService == null)
            {
                return Error(CliErrorType.ExecutionError, "Protocol service not available");
            }

            var output = new StringBuilder();
            output.AppendLine("Telnet server status:");

            // Get Telnet protocol and state
            IDeviceProtocol telnetProtocol = protocolService.GetProtocol(NetworkProtocolType.TELNET);
            if (telnetProtocol == null)
            {
                output.AppendLine("Telnet server: Not configured");
                return Success(output.ToString());
            }

            var telnetState = telnetProtocol.GetState();
            var telnetConfig = telnetProtocol.GetConfiguration();

            if (telnetState == null)
            {
                output.AppendLine("Telnet server: Configuration error");
                return Success(output.ToString());
            }

            // Display Telnet status
            output.AppendLine($"Telnet server: {(telnetState.IsActive ? "Enabled" : "Disabled")}");

            if (telnetState.IsActive)
            {
                var stateData = telnetState.GetStateData();

                output.AppendLine($"Listening port: {stateData.GetValueOrDefault("ListeningPort", "23")}");
                output.AppendLine($"Active sessions: {stateData.GetValueOrDefault("ActiveSessions", "0")}");
                output.AppendLine($"Total connections: {stateData.GetValueOrDefault("TotalConnections", "0")}");

                var lastActivity = stateData.GetValueOrDefault("LastActivity", DateTime.MinValue);
                if (lastActivity is DateTime lastActivityTime && lastActivityTime != DateTime.MinValue)
                {
                    output.AppendLine($"Last activity: {lastActivityTime:yyyy-MM-dd HH:mm:ss}");
                }

                var sessionStats = stateData.GetValueOrDefault("SessionStatistics", null);
                if (sessionStats != null)
                {
                    output.AppendLine("Session details available via 'show telnet sessions'");
                }
            }

            return Success(output.ToString());
        }
    }
}
