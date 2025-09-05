using System.Text;
using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Arista.Show
{
    /// <summary>
    /// Comprehensive Arista show command handler with full feature parity
    /// </summary>
    public class ShowCommandHandler : VendorAgnosticCliHandler
    {
        public ShowCommandHandler() : base("show", "Display device information")
        {
            AddAlias("sh");
            AddAlias("sho");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need show option");
            }

            var option = context.CommandParts[1];

            return option switch
            {
                "version" => HandleShowVersion(context),
                "running-config" => HandleShowRunningConfig(context),
                "startup-config" => HandleShowStartupConfig(context),
                "interfaces" => HandleShowInterfaces(context),
                "interface" => HandleShowInterfaces(context), // Alias
                "vlan" => HandleShowVlan(context),
                "ip" => HandleShowIp(context),
                "lldp" => HandleShowLldp(context),
                "spanning-tree" => HandleShowSpanningTree(context),
                "arp" => HandleShowArp(context),
                "environment" => HandleShowEnvironment(context),
                "inventory" => HandleShowInventory(context),
                "vxlan" => HandleShowVxlan(context),
                "clock" => HandleShowClock(context),
                "mlag" => HandleShowMlag(context),
                "system" => HandleShowSystem(context),
                "port-channel" => HandleShowPortChannel(context),
                "bgp" => HandleShowBgp(context),
                "mac" => HandleShowMac(context),
                _ => Error(CliErrorType.InvalidCommand,
                    $"% Invalid show option: {option}")
            };
        }

        private CliResult HandleShowVersion(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            output.AppendLine("Arista DCS-7050TX-64");
            output.AppendLine("Hardware version:    01.00");
            output.AppendLine("Serial number:       JPE12345678");
            output.AppendLine("System MAC address:  001c.7301.2345");
            output.AppendLine("");
            output.AppendLine("Software image version: 4.25.3F");
            output.AppendLine("Architecture:           i686");
            output.AppendLine("Internal build version: 4.25.3F-22541874.4253F");
            output.AppendLine("Internal build ID:      3ae07322-066e-4ab8-b740-24dd81f0e7e7");
            output.AppendLine("");
            output.AppendLine("Uptime:                 1 week, 2 days, 3 hours and 24 minutes");
            output.AppendLine("Total memory:           3891940 kB");
            output.AppendLine("Free memory:            2170728 kB");

            return Success(output.ToString());
        }

        private CliResult HandleShowRunningConfig(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            output.AppendLine("! Command: show running-config");
            output.AppendLine("! device: " + device?.Name);
            output.AppendLine("! boot system flash:/EOS.swi");
            output.AppendLine("!");
            output.AppendLine("transceiver qsfp default-mode 4x10G");
            output.AppendLine("!");
            output.AppendLine("hostname " + device?.Name);
            output.AppendLine("!");

            // Show interface configurations
            if (device != null)
            {
                var interfaces = device.GetAllInterfaces();
                foreach (var kvp in interfaces)
                {
                    var iface = kvp.Value;
                    output.AppendLine($"interface {iface.Name}");

                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        output.AppendLine($"   description {iface.Description}");
                    }

                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        var cidr = MaskToCidr(iface.SubnetMask);
                        output.AppendLine($"   ip address {iface.IpAddress}/{cidr}");
                    }

                    if (iface.IsShutdown)
                    {
                        output.AppendLine("   shutdown");
                    }
                    else
                    {
                        output.AppendLine("   no shutdown");
                    }

                    output.AppendLine("!");
                }
            }

            output.AppendLine("!");
            output.AppendLine("end");

            return Success(output.ToString());
        }

        private CliResult HandleShowStartupConfig(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            output.AppendLine("! Command: show startup-config");
            output.AppendLine("! device: " + device?.Name);
            output.AppendLine("! boot system flash:/EOS.swi");
            output.AppendLine("!");
            output.AppendLine("hostname " + device?.Name);
            output.AppendLine("!");

            // Add interface configurations (same as running-config for Arista)
            if (device != null)
            {
                var interfaces = device.GetAllInterfaces();
                foreach (var kvp in interfaces)
                {
                    var iface = kvp.Value;
                    output.AppendLine($"interface {iface.Name}");

                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        output.AppendLine($"   description {iface.Description}");
                    }

                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        output.AppendLine($"   ip address {iface.IpAddress}/24");
                    }

                    if (!iface.IsShutdown)
                    {
                        output.AppendLine("   no shutdown");
                    }

                    output.AppendLine("!");
                }
            }

            output.AppendLine("end");
            return Success(output.ToString());
        }

        private CliResult HandleShowInterfaces(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var interfaces = device.GetAllInterfaces();

            // Check for sub-commands
            if (context.CommandParts.Length > 2)
            {
                var subCommand = context.CommandParts[2];
                return subCommand switch
                {
                    "status" => HandleShowInterfacesStatus(context),
                    "description" => HandleShowInterfacesDescription(context),
                    _ => HandleShowSpecificInterface(context, subCommand)
                };
            }

            // Show all interfaces summary
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                output.AppendLine($"{iface.Name} is {(iface.IsUp ? "up" : "down")}, line protocol is {(iface.IsUp ? "up" : "down")}");
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowInterfacesStatus(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("Port       Name   Status       Vlan     Duplex Speed  Type         Flags Encapsulation");

            var interfaces = device.GetAllInterfaces();
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "connected" : iface.IsShutdown ? "disabled" : "notconnect";
                var vlan = iface.SwitchportMode == "access" ? iface.VlanId.ToString() : "trunk";
                output.AppendLine($"{iface.Name,-10} {iface.Description,-6} {status,-12} {vlan,-8} full   1G     EtherSVI");
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowInterfacesDescription(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("Interface                      Status         Protocol Description");

            var interfaces = device.GetAllInterfaces();
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";
                output.AppendLine($"{iface.Name,-30} {status,-14} {protocol,-8} {iface.Description}");
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowSpecificInterface(ICliContext context, string interfaceName)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var iface = device.GetInterface(interfaceName);
            if (iface != null)
            {
                output.AppendLine($"{interfaceName} is {(iface.IsUp ? "up" : "down")}, line protocol is {(iface.IsUp ? "up" : "down")} (connected)");
                output.AppendLine($"  Hardware is Ethernet, address is aabb.cc00.0100 (bia aabb.cc00.0100)");
                if (!string.IsNullOrEmpty(iface.Description))
                    output.AppendLine($"  Description: {iface.Description}");
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    output.AppendLine($"  Internet address is {iface.IpAddress}/{MaskToCidr(iface.SubnetMask)}");
                output.AppendLine($"  MTU 1500 bytes");
                output.AppendLine($"  {iface.RxPackets} packets input, {iface.RxBytes} bytes");
                output.AppendLine($"  {iface.TxPackets} packets output, {iface.TxBytes} bytes");
            }
            else
            {
                output.AppendLine($"% Interface {interfaceName} not found");
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowVlan(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            // Check for sub-commands
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "brief")
            {
                return HandleShowVlanBrief(context);
            }

            // Default VLAN display
            output.AppendLine("VLAN Name                             Status    Ports");
            output.AppendLine("---- -------------------------------- --------- -------------------------------");
            output.AppendLine("1    default                          active    Et1, Et2, Et3, Et4");
            output.AppendLine("10   VLAN0010                         active    ");
            output.AppendLine("20   VLAN0020                         active    ");
            output.AppendLine("100  VLAN0100                         active    ");
            output.AppendLine("");
            output.AppendLine("VLAN Type  SAID       MTU   Parent RingNo BridgeNo Stp  BrdgMode Trans1 Trans2");
            output.AppendLine("---- ----- ---------- ----- ------ ------ -------- ---- -------- ------ ------");
            output.AppendLine("1    enet  100001     1500  -      -      -        -    -        0      0");
            output.AppendLine("10   enet  100010     1500  -      -      -        -    -        0      0");
            output.AppendLine("20   enet  100020     1500  -      -      -        -    -        0      0");
            output.AppendLine("100  enet  100100     1500  -      -      -        -    -        0      0");

            return Success(output.ToString());
        }

        private CliResult HandleShowVlanBrief(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("VLAN Name                             Status    Ports");
            output.AppendLine("---- -------------------------------- --------- -------------------------------");

            var vlans = device.GetAllVlans();
            if (vlans.Count == 0)
            {
                output.AppendLine("1    default                          active    Et1, Et2, Et3, Et4");
            }
            else
            {
                foreach (var vlan in vlans.Values.OrderBy(v => v.Id))
                {
                    // Format interface names (Ethernet1 instead of full name)
                    var interfaceNames = vlan.Interfaces.Select(FormatInterfaceName).ToList();
                    var ports = string.Join(", ", interfaceNames.Take(3));
                    if (interfaceNames.Count > 3)
                        ports += "...";

                    output.AppendLine($"{vlan.Id,-4} {vlan.Name,-32} {(vlan.Active ? "active" : "suspend"),-9} {ports}");
                }
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowIp(ICliContext context)
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
                "arp" => HandleShowIpArp(context),
                "bgp" => HandleShowIpBgp(context),
                "ospf" => HandleShowIpOspf(context),
                _ => Error(CliErrorType.InvalidCommand,
                    $"% Invalid IP show option: {ipOption}")
            };
        }

        private CliResult HandleShowIpRoute(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("VRF: default");
            output.AppendLine("Codes: C - connected, S - static, K - kernel,");
            output.AppendLine("       O - OSPF, IA - OSPF inter area, E1 - OSPF external type 1,");
            output.AppendLine("       E2 - OSPF external type 2, N1 - OSPF NSSA external type 1,");
            output.AppendLine("       N2 - OSPF NSSA external type2, B - BGP, B I - iBGP, B E - eBGP,");
            output.AppendLine("       R - RIP, I L1 - IS-IS level 1, I L2 - IS-IS level 2,");
            output.AppendLine("");
            output.AppendLine("Gateway of last resort is not set");
            output.AppendLine("");

            var routes = device.GetRoutingTable().OrderBy(r => r.AdminDistance).ThenBy(r => r.Network);
            if (routes.Any())
            {
                foreach (var route in routes)
                {
                    var code = route.Protocol switch
                    {
                        "Connected" => "C",
                        "Static" => "S",
                        "OSPF" => "O",
                        "BGP" => "B E",
                        "RIP" => "R",
                        _ => "?"
                    };

                    if (route.Protocol == "Connected")
                    {
                        output.AppendLine($" {code}      {route.Network}/24 is directly connected, {route.Interface}");
                    }
                    else
                    {
                        output.AppendLine($" {code}      {route.Network}/24 [{route.AdminDistance}/{route.Metric}] via {route.NextHop}");
                    }
                }
            }
            else
            {
                output.AppendLine(" C        192.168.1.0/24 is directly connected, Management1");
                output.AppendLine(" S        0.0.0.0/0 [1/0] via 192.168.1.1, Management1");
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowIpInterface(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var interfaces = device.GetAllInterfaces();

            // Check for brief sub-command
            if (context.CommandParts.Length > 3 && context.CommandParts[3] == "brief")
            {
                output.AppendLine("                                                                          Address");
                output.AppendLine("Interface       IP Address            Status       Protocol         MTU    Owner");
                output.AppendLine("--------------- --------------------- ------------ ------------- --------- -------");

                foreach (var kvp in interfaces)
                {
                    var iface = kvp.Value;
                    var ip = string.IsNullOrEmpty(iface.IpAddress) ? "unassigned" : $"{iface.IpAddress}/24";
                    var status = iface.IsUp ? "up" : "down";
                    var protocol = iface.IsUp ? "up" : "down";
                    output.AppendLine($"{iface.Name,-15} {ip,-21} {status,-12} {protocol,-13} 1500");
                }
            }
            else if (context.CommandParts.Length > 3)
            {
                // Show specific interface
                var interfaceName = context.CommandParts[3];
                if (interfaces.ContainsKey(interfaceName))
                {
                    var iface = interfaces[interfaceName];
                    output.AppendLine($"{iface.Name} is {(iface.IsUp ? "up" : "down")}, line protocol is {(iface.IsUp ? "up" : "down")}");
                    output.AppendLine($"  Internet address is {iface.IpAddress}/{iface.SubnetMask}");
                    output.AppendLine($"  Broadcast address is 255.255.255.255");
                    output.AppendLine($"  Address determined by manual configuration");
                    output.AppendLine($"  MTU is {iface.Mtu} bytes");
                    output.AppendLine($"  Directed broadcast forwarding is disabled");
                    output.AppendLine($"  Multicast reserved groups joined: 224.0.0.1");
                    output.AppendLine($"  Outgoing access list is not set");
                    output.AppendLine($"  Inbound access list is not set");
                }
                else
                {
                    return Error(CliErrorType.InvalidParameter,
                        $"% Interface {interfaceName} not found");
                }
            }
            else
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need interface name or 'brief'");
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowIpArp(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("Address         Age (min)  Hardware Addr   Interface");

            var interfaces = device.GetAllInterfaces();
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    output.AppendLine($"{iface.IpAddress,-15} 0          aabb.cc00.0100  {iface.Name}");
                }
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowLldp(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need LLDP option");
            }

            var lldpOption = context.CommandParts[2];

            return lldpOption switch
            {
                "neighbors" => HandleShowLldpNeighbors(context),
                _ => Error(CliErrorType.InvalidCommand,
                    $"% Invalid LLDP show option: {lldpOption}")
            };
        }

        private CliResult HandleShowLldpNeighbors(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("Last table change time   : Never");
            output.AppendLine("Number of table inserts  : 0");
            output.AppendLine("Number of table deletes  : 0");
            output.AppendLine("Number of table drops    : 0");
            output.AppendLine("Number of table age-outs : 0");
            output.AppendLine("");
            output.AppendLine("Local Interface    Neighbor Device ID           Neighbor Port ID    TTL");
            output.AppendLine("Et1                Switch2.example.com          Et1                 120");

            return Success(output.ToString());
        }

        private CliResult HandleShowSpanningTree(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("MST0");
            output.AppendLine("  Spanning tree enabled protocol mstp");
            output.AppendLine("  Root ID    Priority    32768");
            output.AppendLine($"             Address     {device.Name.ToLower()}.local");
            output.AppendLine("             This bridge is the root");
            output.AppendLine("             Hello Time   2 sec  Max Age 20 sec  Forward Delay 15 sec");
            output.AppendLine("");
            output.AppendLine("Interface           Role Sts Cost      Prio.Nbr Type");
            output.AppendLine("------------------- ---- --- --------- -------- --------------------------------");

            var interfaces = device.GetAllInterfaces();
            foreach (var kvp in interfaces.Take(5))
            {
                var iface = kvp.Value;
                if (iface.Name.StartsWith("Et"))
                {
                    var status = iface.IsUp ? "FWD" : "BLK";
                    output.AppendLine($"{iface.Name,-19} Desg {status} 20000     128.1    P2p");
                }
            }

            return Success(output.ToString());
        }

        // Helper methods
        private string GetUptime(INetworkDevice device)
        {
            // Simulate uptime
            var uptime = DateTime.Now.Subtract(DateTime.Today.AddHours(8));
            return $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
        }

        private string FormatInterfaceName(string interfaceName)
        {
            // Convert interface names to match Arista format
            // Examples: Ethernet1, Ethernet2/1, etc.
            if (interfaceName.StartsWith("Ethernet"))
                return interfaceName; // Already in correct format
            if (interfaceName.StartsWith("Et"))
                return "Ethernet" + interfaceName.Substring(2);
            return interfaceName;
        }

        private int MaskToCidr(string mask)
        {
            if (string.IsNullOrEmpty(mask)) return 24; // Default

            var parts = mask.Split('.');
            if (parts.Length != 4) return 24;

            uint maskValue = 0;
            for (int i = 0; i < 4; i++)
            {
                if (uint.TryParse(parts[i], out uint octet))
                {
                    maskValue = (maskValue << 8) | octet;
                }
            }

            // Count the number of 1 bits
            int cidr = 0;
            for (int i = 31; i >= 0; i--)
            {
                if ((maskValue & (1u << i)) != 0)
                    cidr++;
                else
                    break;
            }

            return cidr;
        }

        private CliResult HandleShowArp(ICliContext context)
        {
            // This is just an alias for "show ip arp"
            return HandleShowIpArp(context);
        }

        private CliResult HandleShowEnvironment(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("System Environment:");
            output.AppendLine("");
            output.AppendLine("Temperature Status:");
            output.AppendLine("  System Temperature:  Normal (35C)");
            output.AppendLine("  CPU Temperature:     Normal (42C)");
            output.AppendLine("");
            output.AppendLine("Fan Status:");
            output.AppendLine("  Fan 1:               OK (3000 RPM)");
            output.AppendLine("  Fan 2:               OK (2950 RPM)");
            output.AppendLine("  Fan 3:               OK (3100 RPM)");
            output.AppendLine("");
            output.AppendLine("Power Supply Status:");
            output.AppendLine("  PSU 1:               OK (350W)");
            output.AppendLine("  PSU 2:               OK (350W)");

            return Success(output.ToString());
        }

        private CliResult HandleShowInventory(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("NAME: \"Chassis\", DESCR: \"Arista DCS-7050TX-64\"");
            output.AppendLine("PID: DCS-7050TX-64         , VID: 01, SN: JPE12345678");
            output.AppendLine("");
            output.AppendLine("NAME: \"Power Supply 1\", DESCR: \"350W AC Power Supply\"");
            output.AppendLine("PID: PWR-350-AC           , VID: 01, SN: PWR123456");
            output.AppendLine("");
            output.AppendLine("NAME: \"Power Supply 2\", DESCR: \"350W AC Power Supply\"");
            output.AppendLine("PID: PWR-350-AC           , VID: 01, SN: PWR123457");
            output.AppendLine("");
            output.AppendLine("NAME: \"Fan Module 1\", DESCR: \"System Fan Module\"");
            output.AppendLine("PID: FAN-7050-F           , VID: 01, SN: FAN123456");

            return Success(output.ToString());
        }

        private CliResult HandleShowVxlan(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("VXLAN Information:");
            output.AppendLine("");
            output.AppendLine("UDP Port: 4789");
            output.AppendLine("Source Interface: Loopback0");
            output.AppendLine("");
            output.AppendLine("VNI       Multicast Group   VRF       Interface");
            output.AppendLine("--------- ----------------- --------- ---------");
            output.AppendLine("10100     239.1.1.100       default   Vlan100");
            output.AppendLine("10200     239.1.1.200       default   Vlan200");

            return Success(output.ToString());
        }

        private CliResult HandleShowClock(ICliContext context)
        {
            var output = new StringBuilder();
            var now = DateTime.Now;
            output.AppendLine($"{now:ddd MMM dd HH:mm:ss yyyy}");
            output.AppendLine("Time source is NTP");

            return Success(output.ToString());
        }

        private CliResult HandleShowMlag(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("MLAG Configuration:");
            output.AppendLine("Domain-id          : mlag-domain");
            output.AppendLine("Local-interface    : Vlan4094");
            output.AppendLine("Peer-address       : 192.168.1.2");
            output.AppendLine("Peer-link          : Port-Channel1");
            output.AppendLine("Peer-config        : consistent");
            output.AppendLine("");
            output.AppendLine("MLAG Ports:");
            output.AppendLine("MLAG ID  State   Local ports   Peer ports");
            output.AppendLine("-------  ------  ------------  ------------");
            output.AppendLine("1        Active  Po2           Po2");
            output.AppendLine("2        Active  Po3           Po3");

            return Success(output.ToString());
        }

        private CliResult HandleShowSystem(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            output.AppendLine("System Information:");
            output.AppendLine($"  System Name:       {device?.Name ?? "Unknown"}");
            output.AppendLine("  System Model:      DCS-7050TX-64");
            output.AppendLine("  System Version:    4.25.3F");
            output.AppendLine("  System Uptime:     1 week, 2 days, 3 hours");
            output.AppendLine("  Total Memory:      3891940 kB");
            output.AppendLine("  Available Memory:  2170728 kB");
            output.AppendLine("  CPU Utilization:   15%");

            return Success(output.ToString());
        }

        private CliResult HandleShowPortChannel(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Port-Channel Summary:");
            output.AppendLine("Group  Port-Channel  Protocol    Ports");
            output.AppendLine("-----  ------------  ----------  -----");
            output.AppendLine("1      Po1(RU)       LACP        Et1(P) Et2(P)");
            output.AppendLine("2      Po2(RU)       LACP        Et3(P) Et4(P)");
            output.AppendLine("3      Po3(RU)       LACP        Et5(P) Et6(P)");
            output.AppendLine("");
            output.AppendLine("Flags: (R) Routed, (S) Suspended, (I) Individual");
            output.AppendLine("       (P) Port-channel member, (U) Up");

            return Success(output.ToString());
        }

        private CliResult HandleShowBgp(ICliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 2)
            {
                var subCommand = context.CommandParts[2];
                return subCommand switch
                {
                    "evpn" => HandleShowBgpEvpn(context),
                    "summary" => HandleShowBgpSummary(context),
                    _ => Error(CliErrorType.InvalidCommand,
                        $"% Invalid BGP show option: {subCommand}")
                };
            }

            return HandleShowBgpSummary(context);
        }

        private CliResult HandleShowBgpEvpn(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("BGP EVPN Table:");
            output.AppendLine("Route Distinguisher: 65001:100");
            output.AppendLine("");
            output.AppendLine("   Network          Next Hop            Metric LocPrf Weight Path");
            output.AppendLine("*> [2][0][48][aabb.cc00.0100][0]/216");
            output.AppendLine("                    192.168.1.1              0      0  32768 i");
            output.AppendLine("*> [2][0][48][aabb.cc00.0200][0]/216");
            output.AppendLine("                    192.168.1.2              0      0  32768 i");

            return Success(output.ToString());
        }

        private CliResult HandleShowBgpSummary(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("BGP router identifier 192.168.1.1, local AS number 65001");
            output.AppendLine("BGP table version is 5, main routing table version 5");
            output.AppendLine("2 network entries using 496 bytes of memory");
            output.AppendLine("2 path entries using 160 bytes of memory");
            output.AppendLine("");
            output.AppendLine("Neighbor        V           AS MsgRcvd MsgSent   TblVer  InQ OutQ Up/Down  State/PfxRcd");
            output.AppendLine("192.168.1.2     4        65002      45      47        5    0    0 00:30:15        1");
            output.AppendLine("192.168.1.3     4        65003      32      35        5    0    0 00:25:10        1");

            return Success(output.ToString());
        }

        private CliResult HandleShowMac(ICliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "address-table")
            {
                return HandleShowMacAddressTable(context);
            }

            return HandleShowMacAddressTable(context);
        }

        private CliResult HandleShowMacAddressTable(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("          Mac Address Table");
            output.AppendLine("-------------------------------------------");
            output.AppendLine("");
            output.AppendLine("Vlan    Mac Address       Type        Ports");
            output.AppendLine("----    -----------       --------    -----");
            output.AppendLine("100     aabb.cc00.0100    DYNAMIC     Et1");
            output.AppendLine("100     aabb.cc00.0200    DYNAMIC     Et2");
            output.AppendLine("200     aabb.cc00.0300    DYNAMIC     Et3");
            output.AppendLine("200     aabb.cc00.0400    DYNAMIC     Et4");
            output.AppendLine("");
            output.AppendLine("Total Mac Addresses for this criterion: 4");

            return Success(output.ToString());
        }

        private CliResult HandleShowIpBgp(ICliContext context)
        {
            // Check for sub-commands
            if (context.CommandParts.Length > 3)
            {
                var subCommand = context.CommandParts[3];
                return subCommand switch
                {
                    "summary" => HandleShowBgpSummary(context),
                    _ => Error(CliErrorType.InvalidCommand,
                        $"% Invalid IP BGP show option: {subCommand}")
                };
            }

            var output = new StringBuilder();
            output.AppendLine("BGP table version is 5, local router ID is 192.168.1.1");
            output.AppendLine("Status codes: s suppressed, d damped, h history, * valid, > best, i - internal,");
            output.AppendLine("              r RIB-failure, S Stale, m multipath, b backup-path, f RT-Filter,");
            output.AppendLine("              x best-external, a additional-path, c RIB-compressed,");
            output.AppendLine("Origin codes: i - IGP, e - EGP, ? - incomplete");
            output.AppendLine("");
            output.AppendLine("   Network          Next Hop            Metric LocPrf Weight Path");
            output.AppendLine("*> 10.1.1.0/24      0.0.0.0                  0         32768 i");
            output.AppendLine("*> 10.2.2.0/24      192.168.1.2              0             0 65002 i");

            return Success(output.ToString());
        }

        private CliResult HandleShowIpOspf(ICliContext context)
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
                        $"% Invalid IP OSPF show option: {subCommand}")
                };
            }

            var output = new StringBuilder();
            output.AppendLine("Routing Process \"ospf 1\" with ID 192.168.1.1");
            output.AppendLine("Process has been running for 1 week, 2 days");
            output.AppendLine("Supports only single TOS(TOS0) routes");
            output.AppendLine("SPF schedule delay 5 secs, Hold time between two SPFs 10 secs");
            output.AppendLine("");
            output.AppendLine("Area 0.0.0.0:");
            output.AppendLine("    Number of interfaces in this area is 2");
            output.AppendLine("    Number of fully adjacent neighbors is 1");

            return Success(output.ToString());
        }

        private CliResult HandleShowIpOspfNeighbor(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Neighbor ID     Pri   State           Dead Time   Address         Interface");
            output.AppendLine("192.168.1.2       1   Full/DR         00:00:35    192.168.1.2     Ethernet1");
            output.AppendLine("192.168.1.3       1   Full/BDR        00:00:38    192.168.1.3     Ethernet2");

            return Success(output.ToString());
        }

        private CliResult HandleShowIpOspfDatabase(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("            OSPF Router with ID (192.168.1.1) (Process ID 1)");
            output.AppendLine("");
            output.AppendLine("                Router Link States (Area 0.0.0.0)");
            output.AppendLine("");
            output.AppendLine("Link ID         ADV Router      Age         Seq#       Checksum Link count");
            output.AppendLine("192.168.1.1     192.168.1.1     156         0x80000003 0x004D62  2");
            output.AppendLine("192.168.1.2     192.168.1.2     145         0x80000002 0x003F52  2");

            return Success(output.ToString());
        }
    }
}
