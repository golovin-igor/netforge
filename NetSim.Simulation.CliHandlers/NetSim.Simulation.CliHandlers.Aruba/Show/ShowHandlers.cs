using System.Text;
using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.Aruba.Show
{
    public static class ShowHandlers
    {
        /// <summary>
        /// Aruba show command handler - handles all show subcommands
        /// </summary>
        public class ArubaShowHandler : VendorAgnosticCliHandler
        {
            public ArubaShowHandler() : base("show", "Display system information") 
            { 
                AddAlias("sh");
                AddAlias("sho");
            }

            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "show", StringComparison.OrdinalIgnoreCase);
            }

            protected override CliResult ExecuteCommand(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: show <subcommand>");

                var subcommand = string.Join(" ", args.Skip(1)).ToLower();
                
                return subcommand switch
                {
                    "running-config" => ShowRunningConfig(context),
                    "startup-config" => ShowStartupConfig(context),
                    "version" => ShowVersion(context),
                    "interfaces" => ShowInterfaces(context),
                    "interfaces brief" => ShowInterfaceBrief(context),
                    "interface brief" => ShowInterfaceBrief(context),
                    "ip interface brief" => ShowIpInterfaceBrief(context),
                    "interface" => ShowSpecificInterface(context, args),
                    "vlan" => ShowVlan(context),
                    "vlan brief" => ShowVlanBrief(context),
                    "vlans" => ShowVlan(context),
                    "ip route" => ShowIpRoute(context),
                    "arp" => ShowArp(context),
                    "ip arp" => ShowArp(context),
                    "mac-address-table" => ShowMacAddressTable(context),
                    "mac-address" => ShowMacAddressTable(context),
                    "system" => ShowSystem(context),
                    "spanning-tree" => ShowSpanningTree(context),
                    "tech" => ShowTech(context),
                    "time" => ShowTime(context),
                    "log" => ShowLog(context),
                    "logging" => ShowLogging(context),
                    "trunk" => ShowTrunk(context),
                    "ip bgp summary" => ShowBgpSummary(context),
                    "bgp summary" => ShowBgpSummary(context),
                    "ip ospf neighbor" => ShowOspfNeighbor(context),
                    "ospf neighbor" => ShowOspfNeighbor(context),
                    _ => HandleDynamicInterfaceCommand(context, subcommand)
                };
            }

            private CliResult ShowRunningConfig(CliContext context)
            {
                var vendorCaps = GetVendorCapabilities(context);
                if (vendorCaps != null)
                {
                    var config = vendorCaps.GetRunningConfiguration();
                    return Success(config);
                }
                return Error(CliErrorType.InvalidCommand, "Failed to retrieve running configuration");
            }

            private CliResult ShowStartupConfig(CliContext context)
            {
                var vendorCaps = GetVendorCapabilities(context);
                if (vendorCaps != null)
                {
                    var config = vendorCaps.GetStartupConfiguration();
                    return Success(config);
                }
                return Error(CliErrorType.InvalidCommand, "Failed to retrieve startup configuration");
            }

            private CliResult ShowVersion(CliContext context)
            {
                var output = new StringBuilder();
                output.AppendLine("Aruba Switch");
                output.AppendLine("ArubaOS Version 16.10.0014");
                output.AppendLine("Compiled Jan 01 2024 at 12:00:00");
                output.AppendLine("System uptime is 5 days, 12 hours, 30 minutes");
                output.AppendLine("Base ethernet MAC Address: 00:1b:21:12:34:56");
                output.AppendLine("Hardware: Aruba 2930F-48G-4SFP+ Switch");
                output.AppendLine("Serial number: SG12345678");
                output.AppendLine("Image stamp: FL_16_10_0014");
                output.AppendLine("Boot Image: Primary");
                output.AppendLine("Version information:");
                output.AppendLine("  Built: Jan 01 2024 at 12:00:00");
                return Success(output.ToString());
            }

            private CliResult ShowInterfaces(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                
                if (interfaces == null)
                    return Error(CliErrorType.InvalidCommand, "No interfaces found");

                var output = new StringBuilder();
                foreach (var iface in interfaces.Values)
                {
                    output.AppendLine($"Interface {iface.Name}:");
                    output.AppendLine($"  Type: Ethernet");
                    output.AppendLine($"  Enabled: {(!iface.IsShutdown ? "Yes" : "No")}");
                    output.AppendLine($"  Status: {(iface.IsUp ? "Up" : "Down")}");
                    output.AppendLine($"  Mode: {(string.IsNullOrEmpty(iface.IpAddress) ? "Switched" : "Routed")}");
                    
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        output.AppendLine($"  IP Address: {iface.IpAddress}/{iface.SubnetMask}");
                    }
                    
                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        output.AppendLine($"  Description: {iface.Description}");
                    }
                    
                    output.AppendLine($"  MTU: {iface.Mtu}");
                    output.AppendLine($"  MAC Address: {iface.MacAddress}");
                    output.AppendLine();
                }

                return Success(output.ToString());
            }

            private CliResult ShowInterfaceBrief(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                
                if (interfaces == null)
                    return Error(CliErrorType.InvalidCommand, "No interfaces found");

                var output = new StringBuilder();
                output.AppendLine("Port     Name   Status   VLAN    Type      Mode");
                output.AppendLine("-------- ------ -------- ------- --------- --------");
                
                foreach (var iface in interfaces.Values)
                {
                    var status = iface.IsUp ? "Up" : "Down";
                    var vlan = iface.VlanId > 0 ? iface.VlanId.ToString() : "1";
                    var type = "1000T";
                    var mode = string.IsNullOrEmpty(iface.IpAddress) ? "Access" : "Routed";
                    
                    output.AppendLine($"{iface.Name,-8} {(iface.Description?.Length > 6 ? iface.Description.Substring(0, 6) : iface.Description ?? ""),-6} {status,-8} {vlan,-7} {type,-9} {mode}");
                }

                return Success(output.ToString());
            }

            private CliResult ShowVlan(CliContext context)
            {
                var output = new StringBuilder();
                output.AppendLine("VLAN ID  Name                 Status   Ports");
                output.AppendLine("-------  -------------------- -------- --------");
                output.AppendLine("1        Default              Active   1-48");
                output.AppendLine("10       Guest                Active   1-24");
                output.AppendLine("20       Servers              Active   25-48");
                return Success(output.ToString());
            }

            private CliResult ShowVlanBrief(CliContext context)
            {
                var output = new StringBuilder();
                output.AppendLine("VLAN  Name      Status");
                output.AppendLine("----  --------  ------");
                output.AppendLine("1     Default   Active");
                output.AppendLine("10    Guest     Active");
                output.AppendLine("20    Servers   Active");
                return Success(output.ToString());
            }

            private CliResult ShowIpRoute(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var routes = device?.GetRoutingTable();
                
                if (routes == null)
                    return Error(CliErrorType.InvalidCommand, "No routing table found");

                var output = new StringBuilder();
                output.AppendLine("IP Route Table");
                output.AppendLine("Destination        Gateway         Distance  Type   Interface");
                output.AppendLine("------------------ --------------- --------- ------ ---------");
                
                foreach (var route in routes)
                {
                    output.AppendLine($"{route.Network,-18} {route.NextHop,-15} {route.AdminDistance,-9} {"Static",-6} {route.Interface}");
                }

                return Success(output.ToString());
            }

            private CliResult ShowArp(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var arpTable = device?.GetArpTable();
                
                if (arpTable == null)
                    return Error(CliErrorType.InvalidCommand, "No ARP table found");

                var output = new StringBuilder();
                output.AppendLine("IP ARP table");
                output.AppendLine("IP Address      MAC Address       Type   Port");
                output.AppendLine("--------------- ----------------- ------ ----");
                
                foreach (var entry in arpTable)
                {
                    output.AppendLine($"{entry.Key,-15} {entry.Value,-17} {"Dynamic",-6} {"1"}");
                }

                return Success(output.ToString());
            }

            private CliResult ShowMacAddressTable(CliContext context)
            {
                var output = new StringBuilder();
                output.AppendLine("Status and Counters - Port Address Table");
                output.AppendLine();
                output.AppendLine("MAC Address       VLAN  Port  Type");
                output.AppendLine("----------------- ----- ----- -----");
                output.AppendLine("00:1b:21:12:34:01 1     1     Dynamic");
                output.AppendLine("00:1b:21:12:34:02 1     2     Dynamic");
                output.AppendLine("00:1b:21:12:34:03 10    3     Dynamic");
                output.AppendLine("00:1b:21:12:34:04 20    25    Dynamic");
                return Success(output.ToString());
            }

            private CliResult ShowSystem(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var output = new StringBuilder();
                output.AppendLine("System Information");
                output.AppendLine("==================");
                output.AppendLine($"System Name       : {device?.Name ?? "Unknown"}");
                output.AppendLine($"System Contact    : administrator@company.com");
                output.AppendLine($"System Location   : Data Center");
                output.AppendLine($"System Description: Aruba 2930F Switch");
                output.AppendLine($"Base MAC Address  : 00:1b:21:12:34:56");
                output.AppendLine($"Serial Number     : SG12345678");
                output.AppendLine($"Software Version  : ArubaOS 16.10.0014");
                output.AppendLine($"Bootloader Version: 1.0.0");
                output.AppendLine($"Hardware Version  : Rev A");
                return Success(output.ToString());
            }

            private CliResult ShowSpecificInterface(CliContext context, string[] args)
            {
                if (args.Length < 3)
                    return Error(CliErrorType.InvalidParameter, "Usage: show interface <interface-name>");

                var interfaceName = args[2];
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                
                if (interfaces == null || !interfaces.ContainsKey(interfaceName))
                    return Error(CliErrorType.InvalidParameter, "Interface does not exist");

                var iface = interfaces[interfaceName];
                var output = new StringBuilder();
                
                output.AppendLine();
                output.AppendLine($" Interface {iface.Name} is {(iface.IsUp ? "up" : "down")}, line protocol is {(iface.IsUp ? "up" : "down")}");
                output.AppendLine($"  Hardware is 10/100/1000T, address is {iface.MacAddress}");
                
                if (!string.IsNullOrEmpty(iface.Description))
                {
                    output.AppendLine($"  Description \"{iface.Description}\"");
                }
                
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    output.AppendLine($"  Internet address is {iface.IpAddress}/{MaskToCidr(iface.SubnetMask)}");
                }
                
                output.AppendLine($"  MTU 1500 bytes, encapsulation ethernet");
                output.AppendLine($"  Flow control is off, input flow-control is off");
                output.AppendLine($"     {iface.RxPackets} packets input, {iface.RxBytes} bytes");
                output.AppendLine($"     {iface.TxPackets} packets output, {iface.TxBytes} bytes");
                
                return Success(output.ToString());
            }

            private CliResult ShowSpanningTree(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var output = new StringBuilder();
                
                output.AppendLine();
                output.AppendLine(" Multiple Spanning Trees");
                output.AppendLine();
                output.AppendLine("   STP Enabled   : Yes");
                output.AppendLine("   Force Version : RSTP");
                output.AppendLine("   Priority      : 32768");
                output.AppendLine("   Hello Time    : 2 seconds");
                output.AppendLine("   Max Age       : 20 seconds");
                output.AppendLine("   Forward Delay : 15 seconds");
                output.AppendLine();
                output.AppendLine("   CST Root ID   : 4096 " + (device?.Name ?? "SW1"));
                output.AppendLine("   Root ID       : 4096 " + (device?.Name ?? "SW1"));
                output.AppendLine("   Priority      : 4096");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private CliResult ShowTech(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var output = new StringBuilder();
                
                output.AppendLine("Technical support information:");
                output.AppendLine("Product Model: J9019A");
                output.AppendLine("Software Version: K.15.16.0012");
                output.AppendLine("ROM Version: K.15.07");
                output.AppendLine($"System Name: {device?.Name ?? "Unknown"}");
                output.AppendLine("System Up Time: 1d 2h 34m");
                output.AppendLine("CPU Utilization: 3%");
                output.AppendLine("Memory Utilization: 35%");
                output.AppendLine("Flash Utilization: 38%");
                output.AppendLine("Temperature: Normal");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private CliResult ShowTime(CliContext context)
            {
                var currentTime = DateTime.Now.ToString("ddd MMM  d HH:mm:ss yyyy");
                return Success($"{currentTime}\n");
            }

            private CliResult ShowLog(CliContext context)
            {
                var output = new StringBuilder();
                var currentTime = DateTime.Now.ToString("MMM  d HH:mm:ss");
                
                output.AppendLine();
                output.AppendLine(" Event Log");
                output.AppendLine();
                output.AppendLine($"{currentTime} FFI: port 1-Up");
                output.AppendLine($"{currentTime} STM: port 1-Forwarding");
                output.AppendLine($"{currentTime} ports: port 1 is now on-line");
                output.AppendLine($"{currentTime} FFI: port 2-Up");
                output.AppendLine($"{currentTime} STM: port 2-Forwarding");
                output.AppendLine($"{currentTime} ports: port 2 is now on-line");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private int MaskToCidr(string mask)
            {
                if (string.IsNullOrEmpty(mask)) return 24;
                
                // Convert subnet mask to CIDR notation
                return mask switch
                {
                    "255.255.255.0" => 24,
                    "255.255.0.0" => 16,
                    "255.0.0.0" => 8,
                    "255.255.255.128" => 25,
                    "255.255.255.192" => 26,
                    "255.255.255.224" => 27,
                    "255.255.255.240" => 28,
                    "255.255.255.248" => 29,
                    "255.255.255.252" => 30,
                    _ => 24
                };
            }

            private CliResult ShowIpInterfaceBrief(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                
                if (interfaces == null)
                    return Error(CliErrorType.InvalidCommand, "No interfaces found");

                var output = new StringBuilder();
                output.AppendLine("Internet (IP) Service");
                output.AppendLine();
                output.AppendLine("Interface   IP Address      OK?  Method Status                Protocol");
                output.AppendLine("----------- --------------- ---- ------ ---------------------- --------");
                
                foreach (var iface in interfaces.Values)
                {
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        var status = iface.IsUp ? "up" : "down";
                        var protocol = iface.IsUp ? "up" : "down";
                        output.AppendLine($"{iface.Name,-11} {iface.IpAddress,-15} {"YES",-4} {"manual",-6} {status,-22} {protocol}");
                    }
                }

                return Success(output.ToString());
            }

            private CliResult ShowLogging(CliContext context)
            {
                var output = new StringBuilder();
                var currentTime = DateTime.Now.ToString("MMM  d HH:mm:ss");
                
                output.AppendLine();
                output.AppendLine(" Event Log");
                output.AppendLine();
                output.AppendLine($"{currentTime} FFI: port 1-Up");
                output.AppendLine($"{currentTime} STM: port 1-Forwarding");
                output.AppendLine($"{currentTime} ports: port 1 is now on-line");
                output.AppendLine($"{currentTime} System coldstart");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private CliResult ShowTrunk(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var output = new StringBuilder();
                
                output.AppendLine("Load Balancing Method");
                output.AppendLine();
                output.AppendLine("Port | Name  | Type | Group | Status     | Speed  | Duplex | Flow Ctrl");
                output.AppendLine("---- | ----- | ---- | ----- | ---------- | ------ | ------ | ---------");
                output.AppendLine("1    |       | lacp | Trk1  | Up         | 1000   | Full   | No");
                output.AppendLine("2    |       | lacp | Trk1  | Up         | 1000   | Full   | No");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private CliResult ShowBgpSummary(CliContext context)
            {
                var output = new StringBuilder();
                output.AppendLine("BGP Peer Information");
                output.AppendLine();
                output.AppendLine("Local AS: 65001");
                output.AppendLine("Router ID: 1.1.1.1");
                output.AppendLine();
                output.AppendLine("Neighbor    AS    State    Time    Queue");
                output.AppendLine("----------- ----- -------- ------- -----");
                output.AppendLine("172.16.0.2  65002 Estab    00:05:30    0");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private CliResult ShowOspfNeighbor(CliContext context)
            {
                var output = new StringBuilder();
                output.AppendLine("OSPF Neighbor Information");
                output.AppendLine();
                output.AppendLine("Neighbor ID     Pri   State           Dead Time   Address         Interface");
                output.AppendLine("--------------- ----- --------------- ----------- --------------- ---------");
                output.AppendLine("2.2.2.2         1     Full/DR         00:00:35    10.0.0.2        vlan 1");
                output.AppendLine();
                
                return Success(output.ToString());
            }

            private CliResult HandleDynamicInterfaceCommand(CliContext context, string subcommand)
            {
                // Handle specific interface commands like "interfaces 1", "interfaces vlan 10", etc.
                var parts = subcommand.Split(' ');
                
                if (parts.Length >= 2 && parts[0] == "interfaces")
                {
                    var interfaceName = parts[1];
                    var device = context.Device as NetworkDevice;
                    var interfaces = device?.GetAllInterfaces();
                    
                    if (interfaces == null || !interfaces.ContainsKey(interfaceName))
                        return Error(CliErrorType.InvalidParameter, "Interface does not exist");

                    var iface = interfaces[interfaceName];
                    var output = new StringBuilder();
                    
                    output.AppendLine();
                    output.AppendLine(" Status and Counters - General System Information");
                    output.AppendLine();
                    output.AppendLine($"  System Name        : {device?.Name ?? "Unknown"}");
                    output.AppendLine($"  System Contact     : ");
                    output.AppendLine($"  System Location    : ");
                    output.AppendLine();
                    output.AppendLine($" Interface {iface.Name} Information");
                    output.AppendLine();
                    output.AppendLine($"  Link Status        : {(iface.IsUp ? "Up" : "Down")}");
                    output.AppendLine($"  Port Type          : 1000T");
                    output.AppendLine($"  Port Mode          : {(string.IsNullOrEmpty(iface.IpAddress) ? "Access" : "Routed")}");
                    
                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        output.AppendLine($"  Name               : {iface.Description}");
                    }
                    
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        output.AppendLine($"  IP Address         : {iface.IpAddress}");
                        output.AppendLine($"  Subnet Mask        : {iface.SubnetMask}");
                    }
                    
                    output.AppendLine($"  MAC Address        : {iface.MacAddress}");
                    output.AppendLine($"  Enabled            : {(!iface.IsShutdown ? "Yes" : "No")}");
                    output.AppendLine();
                    
                    return Success(output.ToString());
                }
                
                return Error(CliErrorType.InvalidParameter, $"Unknown show command: {subcommand}");
            }
        }
    }
} 
