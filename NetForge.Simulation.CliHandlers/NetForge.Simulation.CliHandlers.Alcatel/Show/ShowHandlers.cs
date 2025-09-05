using System.Text;
using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Alcatel.Show
{
    /// <summary>
    /// Comprehensive Alcatel show command handler with full feature parity
    /// </summary>
    public class ShowCommandHandler : VendorAgnosticCliHandler
    {
        ShowVersionSubhandler showVersionSubhandler;

        public ShowCommandHandler() : base("show", "Display device information")
        {
            AddAlias("sh");
            AddAlias("sho");

            showVersionSubhandler = new ShowVersionSubhandler(this);
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }

            if (context.CommandParts.Length < 2)
            {
                return Success(GetAvailableShowCommands());
            }

            var showType = context.CommandParts[1].ToLower();

            return showType switch
            {
                "version" => showVersionSubhandler.Handle(context),
                "system" => HandleShowSystem(context),
                "interfaces" => HandleShowInterfaces(context),
                "interface" => HandleShowInterface(context),
                "running-config" => HandleShowRunningConfig(context),
                "startup-config" => HandleShowStartupConfig(context),
                "vlan" => HandleShowVlan(context),
                "mac-address-table" => HandleShowMacAddressTable(context),
                "arp" => HandleShowArp(context),
                "ip" => HandleShowIp(context),
                "spanning-tree" => HandleShowSpanningTree(context),
                "hardware" => HandleShowHardware(context),
                "port" => HandleShowPort(context),
                "router" => HandleShowRouter(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid show option: {showType}")
            };
        }

        private string GetAvailableShowCommands()
        {
            return @"Available show commands:
  arp                    Display ARP table
  hardware               Display hardware information
  interfaces             Display interface information
  interface              Display specific interface information
  ip                     Display IP information
  mac-address-table      Display MAC address table
  port                   Display port information
  running-config         Display current configuration
  spanning-tree          Display spanning tree information
  startup-config         Display startup configuration
  system                 Display system information
  version                Display system version and uptime
  vlan                   Display VLAN information
";
        }

        private CliResult HandleShowSystem(ICliContext context)
        {
            if (context.CommandParts.Length >= 3)
            {
                var systemOption = context.CommandParts[2];

                return systemOption switch
                {
                    "uptime" => HandleShowSystemUptime(context),
                    "time" => HandleShowSystemTime(context),
                    _ => Error(CliErrorType.InvalidCommand, $"% Invalid system option: {systemOption}")
                };
            }

            var device = context.Device;
            var output = new StringBuilder();

            output.AppendLine("System Information:");
            output.AppendLine($"System Name:        {device?.Name}");
            output.AppendLine($"System Type:        OmniSwitch 6850");
            output.AppendLine($"System Contact:     Not configured");
            output.AppendLine($"System Location:    Not configured");
            output.AppendLine($"System Description: Alcatel-Lucent OmniSwitch");
            output.AppendLine($"System Object ID:   1.3.6.1.4.1.6486.801.1.1.2.1.10.1.1");
            output.AppendLine($"System Up Time:     {GetSystemUptime()}");
            output.AppendLine($"Current Date:       {DateTime.Now:MMM dd yyyy HH:mm:ss}");
            output.AppendLine();

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

            output.AppendLine("Interface Status Summary");
            output.AppendLine("Port     Admin   Oper    Speed   Duplex  Flow Control");
            output.AppendLine("-------- ------- ------- ------- ------- ------------");

            foreach (var iface in interfaces.Values.OrderBy(i => i.Name))
            {
                var admin = iface.IsShutdown ? "down" : "up";
                var oper = iface.IsUp ? "up" : "down";
                var speed = "1000";
                var duplex = "full";
                var flowControl = "off";

                output.AppendLine($"{iface.Name,-8} {admin,-7} {oper,-7} {speed,-7} {duplex,-7} {flowControl}");
            }

            output.AppendLine();
            return Success(output.ToString());
        }

        private CliResult HandleShowInterface(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need interface name");
            }

            var device = context.Device;
            var interfaceName = context.CommandParts[2];
            var iface = device?.GetInterface(interfaceName);

            if (iface == null)
            {
                return Error(CliErrorType.InvalidParameter, $"% Interface {interfaceName} not found");
            }

            var output = new StringBuilder();
            output.AppendLine($"Interface {iface.Name} configuration:");
            output.AppendLine($"  Description:      {iface.Description ?? "Not configured"}");
            output.AppendLine($"  Admin Status:     {(iface.IsShutdown ? "Down" : "Up")}");
            output.AppendLine($"  Operational Status: {(iface.IsUp ? "Up" : "Down")}");
            output.AppendLine($"  Speed:            1000 Mbps");
            output.AppendLine($"  Duplex:           Full");
            output.AppendLine($"  Auto-negotiation: Enabled");
            output.AppendLine($"  Flow Control:     Disabled");
            output.AppendLine($"  IP Address:       {iface.IpAddress ?? "Not configured"}");
            output.AppendLine($"  Subnet Mask:      {iface.SubnetMask ?? "Not configured"}");
            output.AppendLine($"  MAC Address:      {iface.MacAddress ?? GenerateMacAddress()}");
            output.AppendLine($"  MTU:              {iface.Mtu}");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowRunningConfig(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            output.AppendLine("Building configuration...");
            output.AppendLine();
            output.AppendLine("Current configuration:");
            output.AppendLine("!");
            output.AppendLine($"system name {device.Name}");
            output.AppendLine("!");

            // Show interface configurations
            var interfaces = device.GetAllInterfaces();
            foreach (var iface in interfaces.Values.OrderBy(i => i.Name))
            {
                output.AppendLine($"interface {iface.Name}");
                if (!string.IsNullOrEmpty(iface.Description))
                {
                    output.AppendLine($"  description {iface.Description}");
                }

                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    output.AppendLine($"  ip interface {iface.IpAddress}/{MaskToCidr(iface.SubnetMask)}");
                }

                if (iface.IsShutdown)
                {
                    output.AppendLine("  admin-state disable");
                }

                output.AppendLine("!");
            }

            output.AppendLine("end");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowStartupConfig(ICliContext context)
        {
            return Success("Startup configuration is empty or not saved.\n");
        }

        private CliResult HandleShowVlan(ICliContext context)
        {
            if (context.CommandParts.Length >= 3)
            {
                var vlanOption = context.CommandParts[2];

                return vlanOption switch
                {
                    "info" => HandleShowVlanInfo(context),
                    _ => Error(CliErrorType.InvalidCommand, $"% Invalid vlan option: {vlanOption}")
                };
            }

            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var vlans = device.GetAllVlans();

            output.AppendLine("VLAN Configuration");
            output.AppendLine("VLAN ID  Name                     Status   Ports");
            output.AppendLine("-------- ------------------------ -------- --------");

            if (vlans.Count == 0)
            {
                output.AppendLine("1        default                  active   All");
            }
            else
            {
                foreach (var vlan in vlans.Values.OrderBy(v => v.Id))
                {
                    output.AppendLine($"{vlan.Id,-8} {vlan.Name,-24} active   ");
                }
            }

            output.AppendLine();
            return Success(output.ToString());
        }

        private CliResult HandleShowMacAddressTable(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("MAC Address Table");
            output.AppendLine("VLAN  MAC Address       Type      Port");
            output.AppendLine("----- ----------------- --------- --------");
            output.AppendLine("1     00:11:22:33:44:01  dynamic   1/1/1");
            output.AppendLine("1     00:11:22:33:44:02  dynamic   1/1/2");
            output.AppendLine("1     00:11:22:33:44:03  static    1/1/3");
            output.AppendLine();
            output.AppendLine("Total MAC Addresses: 3");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowArp(ICliContext context)
        {
            var device = context.Device;
            var arpTable = device?.GetArpTableOutput();

            if (string.IsNullOrEmpty(arpTable))
            {
                return Success("ARP table is empty.\n");
            }

            return Success(arpTable);
        }

        private CliResult HandleShowIp(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Success("Available IP show commands:\n  interface    Show IP interface information\n  route        Show IP routing table\n");
            }

            var subCommand = context.CommandParts[2].ToLower();
            return subCommand switch
            {
                "interface" => HandleShowIpInterface(context),
                "route" => HandleShowIpRoute(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid IP show option: {subCommand}")
            };
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

            output.AppendLine("IP Interface Status");
            output.AppendLine("Interface        IP Address        Status     Protocol");
            output.AppendLine("---------------- ----------------- ---------- ----------");

            foreach (var iface in interfaces.Values.Where(i => !string.IsNullOrEmpty(i.IpAddress)).OrderBy(i => i.Name))
            {
                var status = GetInterfaceStatus(iface);
                output.AppendLine($"{iface.Name,-16} {iface.IpAddress,-17} {status,-10} up");
            }

            output.AppendLine();
            return Success(output.ToString());
        }

        private CliResult HandleShowIpRoute(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var routes = device.GetRoutingTable();

            output.AppendLine("IP Route Table");
            output.AppendLine("Destination       Gateway           Interface   Metric  Type");
            output.AppendLine("----------------- ----------------- ----------- ------- --------");

            if (routes.Count == 0)
            {
                output.AppendLine("No routes configured.");
            }
            else
            {
                foreach (var route in routes.OrderBy(r => r.Network))
                {
                    var cidr = MaskToCidr(route.Mask);
                    output.AppendLine($"{route.Network}/{cidr,-16} {route.NextHop,-17} {route.Interface,-11} {route.Metric,-7} {route.Protocol}");
                }
            }

            output.AppendLine();
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

            output.AppendLine("Spanning Tree Protocol Information");
            output.AppendLine("VLAN 1 - Spanning Tree Enabled");
            output.AppendLine($"Root ID:     Priority 32769, Address {GenerateMacAddress()}");
            output.AppendLine("             This bridge is the root");
            output.AppendLine("Bridge ID:   Priority 32769");
            output.AppendLine("Hello Time:  2 sec  Max Age: 20 sec  Forward Delay: 15 sec");
            output.AppendLine();
            output.AppendLine("Port     Role Sts Cost     Prio.Nbr Type");
            output.AppendLine("-------- ---- --- -------- -------- ----");

            var interfaces = device.GetAllInterfaces();
            foreach (var iface in interfaces.Values.Where(i => i.Name.Contains("/")).OrderBy(i => i.Name))
            {
                var role = "Desg";
                var status = iface.IsUp ? "FWD" : "BLK";
                var cost = "19";
                var priority = "128";
                output.AppendLine($"{iface.Name,-8} {role,-4} {status,-3} {cost,-8} {priority,-8} P2p");
            }

            output.AppendLine();
            return Success(output.ToString());
        }

        private CliResult HandleShowHardware(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Hardware Information");
            output.AppendLine("System Type:         OmniSwitch 6850");
            output.AppendLine("Chassis Serial Number: ABC123456789");
            output.AppendLine("Power Supply 1:      150W - OK");
            output.AppendLine("Power Supply 2:      Not Present");
            output.AppendLine("Fan Tray 1:          OK");
            output.AppendLine("Fan Tray 2:          OK");
            output.AppendLine("Temperature:         Normal (35°C)");
            output.AppendLine("CPU:                 ARM Cortex-A9");
            output.AppendLine("Memory:              1GB DDR3");
            output.AppendLine("Flash:               512MB");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowPort(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var interfaces = device.GetAllInterfaces();

            output.AppendLine("Port Configuration and Status");
            output.AppendLine("Port     Type     Admin  Oper   Speed    Duplex   Auto-neg");
            output.AppendLine("-------- -------- ------ ------ -------- -------- --------");

            foreach (var iface in interfaces.Values.OrderBy(i => i.Name))
            {
                var type = "1000Base-T";
                var admin = iface.IsShutdown ? "down" : "up";
                var oper = iface.IsUp ? "up" : "down";
                var speed = "1000";
                var duplex = "full";
                var autoneg = "enabled";

                output.AppendLine($"{iface.Name,-8} {type,-8} {admin,-6} {oper,-6} {speed,-8} {duplex,-8} {autoneg}");
            }

            output.AppendLine();
            return Success(output.ToString());
        }

        private CliResult HandleShowRouter(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need router option");
            }

            var routerOption = context.CommandParts[2];

            return routerOption switch
            {
                "route-table" => HandleShowRouterRouteTable(context),
                "ldp" => HandleShowRouterLdp(context),
                "mpls" => HandleShowRouterMpls(context),
                "isis" => HandleShowRouterIsis(context),
                "ospf" => HandleShowRouterOspf(context),
                "bgp" => HandleShowRouterBgp(context),
                "arp" => HandleShowRouterArp(context),
                "rip" => HandleShowRouterRip(context),
                "interface" => HandleShowRouterInterface(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid router option: {routerOption}")
            };
        }

        private CliResult HandleShowSystemUptime(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine($"System uptime: {GetSystemUptime()}");
            output.AppendLine($"System started: {DateTime.Now.AddDays(-7):MMM dd yyyy HH:mm:ss}");
            output.AppendLine($"Last restart reason: Power-on");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowSystemTime(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine($"Current system time: {DateTime.Now:MMM dd yyyy HH:mm:ss}");
            output.AppendLine($"Time zone: UTC");
            output.AppendLine($"NTP status: Not synchronized");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowVlanInfo(ICliContext context)
        {
            var device = context.Device;
            var output = new StringBuilder();

            output.AppendLine("VLAN Information Details");
            output.AppendLine("VLAN ID  Name                     Status   Type    Ports");
            output.AppendLine("-------- ------------------------ -------- ------- --------");
            output.AppendLine("1        default                  active   ethernet All");
            output.AppendLine("10       Engineering              active   ethernet 1-8");
            output.AppendLine("20       Sales                    active   ethernet 9-16");
            output.AppendLine("30       Management               active   ethernet 17-24");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowRouterRouteTable(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Route Table");
            output.AppendLine("Destination         Gateway             Interface     Metric Protocol");
            output.AppendLine("------------------- ------------------- ------------- ------ --------");
            output.AppendLine("0.0.0.0/0          192.168.1.1         system       1      static");
            output.AppendLine("192.168.1.0/24     0.0.0.0            system       0      local");
            output.AppendLine("127.0.0.0/8        0.0.0.0            loopback     0      local");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowRouterLdp(ICliContext context)
        {
            if (context.CommandParts.Length >= 4 && context.CommandParts[3] == "interface")
            {
                var output = new StringBuilder();
                output.AppendLine("LDP Interface Status");
                output.AppendLine("Interface      Status      Discovery Transport");
                output.AppendLine("-------------- ----------- --------- ---------");
                output.AppendLine("system         enabled     enabled   enabled");
                output.AppendLine();

                return Success(output.ToString());
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid LDP option");
        }

        private CliResult HandleShowRouterMpls(ICliContext context)
        {
            if (context.CommandParts.Length >= 4 && context.CommandParts[3] == "lsp")
            {
                var output = new StringBuilder();
                output.AppendLine("MPLS LSP Information");
                output.AppendLine("LSP Name         Status    Path        Bandwidth");
                output.AppendLine("---------------- --------- ----------- ---------");
                output.AppendLine("to-node-1       up        dynamic     100M");
                output.AppendLine("to-node-2       down      explicit    50M");
                output.AppendLine();

                return Success(output.ToString());
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid MPLS option");
        }

        private CliResult HandleShowRouterIsis(ICliContext context)
        {
            if (context.CommandParts.Length >= 4 && context.CommandParts[3] == "adjacency")
            {
                var output = new StringBuilder();
                output.AppendLine("ISIS Adjacency Database");
                output.AppendLine("System ID      Interface  Level State   Hold Time");
                output.AppendLine("-------------- ---------- ----- ------- ---------");
                output.AppendLine("1234.5678.9abc system     2     Up      27");
                output.AppendLine();

                return Success(output.ToString());
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid ISIS option");
        }

        private CliResult HandleShowRouterOspf(ICliContext context)
        {
            if (context.CommandParts.Length >= 4 && context.CommandParts[3] == "neighbor")
            {
                var output = new StringBuilder();
                output.AppendLine("OSPF Neighbor Information");
                output.AppendLine("Neighbor ID     Priority State      Dead Time Interface");
                output.AppendLine("--------------- -------- ---------- --------- ---------");
                output.AppendLine("192.168.1.2     1        Full       00:00:37  system");
                output.AppendLine("192.168.1.3     1        Full       00:00:31  system");
                output.AppendLine();

                return Success(output.ToString());
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid OSPF option");
        }

        private CliResult HandleShowRouterBgp(ICliContext context)
        {
            if (context.CommandParts.Length >= 4 && context.CommandParts[3] == "summary")
            {
                var output = new StringBuilder();
                output.AppendLine("BGP Summary Information");
                output.AppendLine("Neighbor        AS    State       Up/Down   PfxRcd PfxSnt");
                output.AppendLine("--------------- ----- ----------- --------- ------ ------");
                output.AppendLine("192.168.1.2     65001 Established 2d17h     10     5");
                output.AppendLine("192.168.1.3     65002 Established 1d05h     8      3");
                output.AppendLine();

                return Success(output.ToString());
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid BGP option");
        }

        private CliResult HandleShowRouterArp(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("ARP Table");
            output.AppendLine("IP Address      HW Address         Interface");
            output.AppendLine("--------------- ------------------ ---------");
            output.AppendLine("192.168.1.1     00:11:22:33:44:01  system");
            output.AppendLine("192.168.1.2     00:11:22:33:44:02  system");
            output.AppendLine("192.168.1.3     00:11:22:33:44:03  system");
            output.AppendLine();

            return Success(output.ToString());
        }

        private CliResult HandleShowRouterRip(ICliContext context)
        {
            if (context.CommandParts.Length >= 4 && context.CommandParts[3] == "neighbor")
            {
                var output = new StringBuilder();
                output.AppendLine("RIP Neighbor Information");
                output.AppendLine("Neighbor        Interface  Version  Last Update");
                output.AppendLine("--------------- ---------- -------- -----------");
                output.AppendLine("192.168.1.2     system     2        00:00:15");
                output.AppendLine("192.168.1.3     system     2        00:00:22");
                output.AppendLine();

                return Success(output.ToString());
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid RIP option");
        }

        private CliResult HandleShowRouterInterface(ICliContext context)
        {
            var output = new StringBuilder();

            output.AppendLine("Router Interface Status");
            output.AppendLine("Interface      Status       IP Address      Subnet Mask");
            output.AppendLine("-------------- ------------ --------------- ---------------");
            output.AppendLine("system         up           192.168.1.1     255.255.255.0");
            output.AppendLine("loopback       up           127.0.0.1       255.0.0.0");
            output.AppendLine();

            return Success(output.ToString());
        }

        // Helper methods
        internal string GetSystemUptime()
        {
            // Simulate uptime - in real implementation this would track actual uptime
            return "7 days, 14 hours, 35 minutes";
        }

        private string GenerateMacAddress()
        {
            // Generate a simple MAC address for display
            return "00:80:f0:12:34:56";
        }

        private string GetInterfaceStatus(dynamic iface)
        {
            if (iface.IsShutdown)
                return "admin-down";
            return iface.IsUp ? "up" : "down";
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
    }
}
