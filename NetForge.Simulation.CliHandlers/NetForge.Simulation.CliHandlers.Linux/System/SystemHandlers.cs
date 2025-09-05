using System.Text;
using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Simulation.CliHandlers.Linux.System;

public static class SystemHandlers
{
    public class IpLinkSetHandler() : VendorAgnosticCliHandler("ip", "IP configuration commands")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            var args = context.CommandParts;

            if (!IsVendor(context, "Linux"))
                return Error(CliErrorType.InvalidMode, "This command is only available on Linux devices");

            if (args.Length < 2)
                return Error(CliErrorType.InvalidParameter, "Usage: ip <subcommand>");

            var subcommand = args[1];

            return subcommand switch
            {
                "link" => HandleIpLink(context, args),
                "addr" => HandleIpAddress(context, args),
                "route" => HandleIpRoute(context, args),
                _ => Error(CliErrorType.InvalidParameter, $"Unknown subcommand: {subcommand}")
            };
        }

        private CliResult HandleIpLink(ICliContext context, string[] args)
        {
            if (args.Length < 3)
                return Error(CliErrorType.InvalidParameter, "Usage: ip link <show|set>");

            var action = args[2];

            return action switch
            {
                "show" => ShowInterfaces(context, args),
                "set" => SetInterface(context, args),
                _ => Error(CliErrorType.InvalidParameter, $"Unknown action: {action}")
            };
        }

        private CliResult ShowInterfaces(ICliContext context, string[] args)
        {
            var output = new StringBuilder();

            // Show standard Linux interfaces
            output.AppendLine("1: lo: <LOOPBACK,UP,LOWER_UP> mtu 65536 qdisc noqueue state UNKNOWN mode DEFAULT");
            output.AppendLine("    link/loopback 00:00:00:00:00:00 brd 00:00:00:00:00:00");
            output.AppendLine("2: eth0: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc pfifo_fast state UP mode DEFAULT");
            output.AppendLine("    link/ether 00:1b:21:12:34:56 brd ff:ff:ff:ff:ff:ff");
            output.AppendLine("3: eth1: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc pfifo_fast state UP mode DEFAULT");
            output.AppendLine("    link/ether 00:1b:21:12:34:57 brd ff:ff:ff:ff:ff:ff");

            return Success(output.ToString());
        }

        private CliResult SetInterface(ICliContext context, string[] args)
        {
            if (args.Length < 5)
                return Error(CliErrorType.InvalidParameter, "Usage: ip link set <interface> <up|down>");

            var interfaceName = args[3];
            var state = args[4];

            return Success($"Interface {interfaceName} set {state}");
        }

        private CliResult HandleIpAddress(ICliContext context, string[] args)
        {
            if (args.Length < 3)
                return Error(CliErrorType.InvalidParameter, "Usage: ip addr <add|del|show>");

            var action = args[2];

            return action switch
            {
                "add" => AddIpAddress(context, args),
                "del" => DelIpAddress(context, args),
                "show" => ShowIpAddresses(context, args),
                _ => Error(CliErrorType.InvalidParameter, $"Unknown action: {action}")
            };
        }

        private CliResult AddIpAddress(ICliContext context, string[] args)
        {
            if (args.Length < 6)
                return Error(CliErrorType.InvalidParameter, "Usage: ip addr add <ip/mask> dev <interface>");

            var ipWithMask = args[3];
            var interfaceName = args[5];

            var parts = ipWithMask.Split('/');
            if (parts.Length != 2)
                return Error(CliErrorType.InvalidParameter, "Invalid IP address format. Use IP/mask");

            return Success($"IP address {ipWithMask} added to {interfaceName}");
        }

        private CliResult DelIpAddress(ICliContext context, string[] args)
        {
            if (args.Length < 6)
                return Error(CliErrorType.InvalidParameter, "Usage: ip addr del <ip/mask> dev <interface>");

            var ipWithMask = args[3];
            var interfaceName = args[5];

            return Success($"IP address {ipWithMask} removed from {interfaceName}");
        }

        private CliResult ShowIpAddresses(ICliContext context, string[] args)
        {
            return ShowInterfaces(context, args);
        }

        private CliResult HandleIpRoute(ICliContext context, string[] args)
        {
            if (args.Length < 3)
                return Error(CliErrorType.InvalidParameter, "Usage: ip route <add|del|show>");

            var action = args[2];

            return action switch
            {
                "add" => AddRoute(context, args),
                "del" => DelRoute(context, args),
                "show" => ShowRoutes(context, args),
                _ => Error(CliErrorType.InvalidParameter, $"Unknown action: {action}")
            };
        }

        private CliResult AddRoute(ICliContext context, string[] args)
        {
            if (args.Length < 5)
                return Error(CliErrorType.InvalidParameter, "Usage: ip route add <network> via <gateway>");

            var network = args[3];
            var gateway = args[5];

            return Success($"Route {network} via {gateway} added");
        }

        private CliResult DelRoute(ICliContext context, string[] args)
        {
            if (args.Length < 4)
                return Error(CliErrorType.InvalidParameter, "Usage: ip route del <network>");

            var network = args[3];

            return Success($"Route {network} deleted");
        }

        private CliResult ShowRoutes(ICliContext context, string[] args)
        {
            var output = "default via 192.168.1.1 dev eth0\n" +
                        "192.168.1.0/24 dev eth0 proto kernel scope link src 192.168.1.100\n";

            return Success(output);
        }
    }

    public class IpAddressHandler() : VendorAgnosticCliHandler("ip address", "Configure interface IP addresses")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 5 &&
                   string.Equals(context.CommandParts[0], "ip", StringComparison.OrdinalIgnoreCase) &&
                   (string.Equals(context.CommandParts[1], "addr", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(context.CommandParts[1], "address", StringComparison.OrdinalIgnoreCase));
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 6)
                return Error(CliErrorType.IncompleteCommand, "Usage: ip addr <add|del> <ip>/<cidr> dev <iface>");

            var action = context.CommandParts[2].ToLower();
            var addrPart = context.CommandParts[3];
            var devIndex = Array.IndexOf(context.CommandParts, "dev");
            if (devIndex < 0 || devIndex + 1 >= context.CommandParts.Length)
                return Error(CliErrorType.IncompleteCommand, "Usage: ip addr <add|del> <ip>/<cidr> dev <iface>");
            var ifaceName = context.CommandParts[devIndex + 1];
            var iface = context.Device.GetInterface(ifaceName);
            if (iface == null)
                return Error(CliErrorType.InvalidParameter, "Interface not found");

            var parts = addrPart.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int cidr))
                return Error(CliErrorType.InvalidParameter, "Invalid address format");
            var ip = parts[0];
            var mask = CidrToMask(cidr);

            switch (action)
            {
                case "add":
                    iface.IpAddress = ip;
                    iface.SubnetMask = mask;
                    context.Device.ForceUpdateConnectedRoutes();
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                case "del":
                    iface.IpAddress = null;
                    iface.SubnetMask = null;
                    context.Device.ForceUpdateConnectedRoutes();
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected add or del");
            }
        }

        private string CidrToMask(int cidr)
        {
            uint mask = cidr == 0 ? 0u : 0xffffffffu << (32 - cidr);
            return $"{(mask >> 24) & 0xff}.{(mask >> 16) & 0xff}.{(mask >> 8) & 0xff}.{mask & 0xff}";
        }
    }

    public class IpRouteHandler() : VendorAgnosticCliHandler("ip route", "Manage static routes")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 2 &&
                   string.Equals(context.CommandParts[0], "ip", StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(context.CommandParts[1], "route", StringComparison.OrdinalIgnoreCase);
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
                return Error(CliErrorType.IncompleteCommand, "Usage: ip route <show|add|del>");

            var action = context.CommandParts[2].ToLower();

            switch (action)
            {
                case "show":
                    return Success(ShowRoutes(context));
                case "add":
                case "del":
                    return HandleRouteCommand(context, action);
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected show, add, or del");
            }
        }

        private CliResult HandleRouteCommand(ICliContext context, string action)
        {
            if (context.CommandParts.Length < 6)
                return Error(CliErrorType.IncompleteCommand, "Usage: ip route <add|del> <network>/<cidr> via <next-hop>");

            var networkPart = context.CommandParts[3];
            if (!context.CommandParts[4].Equals("via", StringComparison.OrdinalIgnoreCase))
                return Error(CliErrorType.IncompleteCommand, "Usage: ip route <add|del> <network>/<cidr> via <next-hop>");
            var nextHop = context.CommandParts[5];

            var parts = networkPart.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int cidr))
                return Error(CliErrorType.InvalidParameter, "Invalid network format");
            var network = parts[0];
            var mask = CidrToMask(cidr);

            switch (action)
            {
                case "add":
                    context.Device.AddStaticRoute(network, mask, nextHop, 1);
                    return Success("");
                case "del":
                    context.Device.RemoveStaticRoute(network, mask);
                    return Success("");
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected add or del");
            }
        }

        private string ShowRoutes(ICliContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Destination     Gateway         Mask            Interface");
            foreach (var route in context.Device.GetRoutingTable())
            {
                sb.AppendLine($"{route.Network,-15} {route.NextHop,-15} {route.Mask,-15} {route.Interface}");
            }
            return sb.ToString();
        }

        private string CidrToMask(int cidr)
        {
            uint mask = cidr == 0 ? 0u : 0xffffffffu << (32 - cidr);
            return $"{(mask >> 24) & 0xff}.{(mask >> 16) & 0xff}.{(mask >> 8) & 0xff}.{mask & 0xff}";
        }
    }

    public class IfconfigHandler() : VendorAgnosticCliHandler("ifconfig", "Display interface configuration")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 1 &&
                   string.Equals(context.CommandParts[0], "ifconfig", StringComparison.OrdinalIgnoreCase);
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            return Success(ShowInterfaces(context));
        }

        private string ShowInterfaces(ICliContext context)
        {
            var sb = new StringBuilder();
            foreach (var iface in context.Device.GetAllInterfaces().Values)
            {
                sb.AppendLine($"{iface.Name}: flags=<{iface.GetStatus()}> mtu {iface.Mtu}");
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    sb.AppendLine($"        inet {iface.IpAddress} netmask {iface.SubnetMask}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    public class RouteHandler() : VendorAgnosticCliHandler("route", "Display routing table")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 1 &&
                   string.Equals(context.CommandParts[0], "route", StringComparison.OrdinalIgnoreCase);
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            return Success(ShowRoutes(context));
        }

        private string ShowRoutes(ICliContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Kernel IP routing table");
            sb.AppendLine("Destination     Gateway         Genmask         Flags   Metric Ref    Use Iface");
            foreach (var route in context.Device.GetRoutingTable())
            {
                sb.AppendLine($"{route.Network,-15} {route.NextHop,-15} {route.Mask,-15} UG      {route.AdminDistance,3}    0        0 {route.Interface}");
            }
            return sb.ToString();
        }
    }

    public class ArpHandler() : VendorAgnosticCliHandler("arp", "Display ARP table")
    {
        public override bool CanHandle(ICliContext context)
        {
            if (context.CommandParts.Length == 0)
                return false;
            var cmd = context.CommandParts[0].ToLower();
            if (cmd == "arp")
                return true;
            if (cmd == "ip" && context.CommandParts.Length >= 2 &&
                (context.CommandParts[1].Equals("neigh", StringComparison.OrdinalIgnoreCase) ||
                 context.CommandParts[1].Equals("neighbor", StringComparison.OrdinalIgnoreCase)))
                return true;
            return false;
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            return Success(BuildArpOutput(context));
        }

        private string BuildArpOutput(ICliContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Address                  HWtype  HWaddress           Flags Mask            Iface");
            foreach (var entry in context.Device.GetArpTable())
            {
                sb.AppendLine($"{entry.Key,-23} ether   {entry.Value,-17} C                     -");
            }
            return sb.ToString();
        }
    }

    public class OspfHandler() : VendorAgnosticCliHandler("ospf", "OSPF routing protocol")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 1 &&
                   string.Equals(context.CommandParts[0], "ospf", StringComparison.OrdinalIgnoreCase);
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 2)
                return Error(CliErrorType.IncompleteCommand, "Usage: ospf <enable|disable>");

            var action = context.CommandParts[1].ToLower();
            var config = context.Device.GetOspfConfiguration();
            if (config == null)
            {
                config = new OspfConfig(1);
                context.Device.SetOspfConfiguration(config);
            }
            // OSPF protocol is auto-registered based on vendor compatibility

            switch (action)
            {
                case "enable":
                    config.IsEnabled = true;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                case "disable":
                    config.IsEnabled = false;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected enable or disable");
            }
        }
    }

    public class BgpHandler() : VendorAgnosticCliHandler("bgp", "BGP routing protocol")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 1 &&
                   string.Equals(context.CommandParts[0], "bgp", StringComparison.OrdinalIgnoreCase);
        }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 2)
                return Error(CliErrorType.IncompleteCommand, "Usage: bgp <enable|disable>");

            var action = context.CommandParts[1].ToLower();
            var config = context.Device.GetBgpConfiguration();
            if (config == null)
            {
                config = new BgpConfig(65000);
                context.Device.SetBgpConfiguration(config);
            }
            // BGP protocol is auto-registered based on vendor compatibility

            switch (action)
            {
                case "enable":
                    config.IsEnabled = true;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                case "disable":
                    config.IsEnabled = false;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected enable or disable");
            }
        }
    }

    public class RipHandler() : VendorAgnosticCliHandler("rip", "RIP routing protocol")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 1 &&
                   string.Equals(context.CommandParts[0], "rip", StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 2)
                return Error(CliErrorType.IncompleteCommand, "Usage: rip <enable|disable>");

            var action = context.CommandParts[1].ToLower();
            var config = context.Device.GetRipConfiguration();
            if (config == null)
            {
                config = new RipConfig();
                context.Device.SetRipConfiguration(config);
            }
            // RIP protocol is auto-registered based on vendor compatibility

            switch (action)
            {
                case "enable":
                    config.IsEnabled = true;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                case "disable":
                    config.IsEnabled = false;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected enable or disable");
            }
        }
    }

    public class LsmodHandler() : VendorAgnosticCliHandler("lsmod", "Show loaded kernel modules")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 1 &&
                   string.Equals(context.CommandParts[0], "lsmod", StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            return Success("Module                  Size  Used by\nip_tables              32768  0\nxt_conntrack           16384  0\nnf_conntrack          131072  1 xt_conntrack\n");
        }
    }

    public class IpLinkHandler() : VendorAgnosticCliHandler("ip link", "Configure interface link state")
    {
        public override bool CanHandle(ICliContext context)
        {
            return context.CommandParts.Length >= 2 &&
                   string.Equals(context.CommandParts[0], "ip", StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(context.CommandParts[1], "link", StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
                return Error(CliErrorType.IncompleteCommand, "Usage: ip link <show|set>");

            var action = context.CommandParts[2].ToLower();

            switch (action)
            {
                case "show":
                    return Success(ShowInterfaces(context));
                case "set":
                    return HandleSetCommand(context);
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected show or set");
            }
        }

        private CliResult HandleSetCommand(ICliContext context)
        {
            if (context.CommandParts.Length < 5)
                return Error(CliErrorType.IncompleteCommand, "Usage: ip link set <iface> <up|down>");

            var ifaceName = context.CommandParts[3];
            var iface = context.Device.GetInterface(ifaceName);
            if (iface == null)
                return Error(CliErrorType.InvalidParameter, "Interface not found");

            var state = context.CommandParts[4].ToLower();
            switch (state)
            {
                case "up":
                    iface.IsShutdown = false;
                    iface.IsUp = true;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                case "down":
                    iface.IsUp = false;
                    context.Device.ParentNetwork?.UpdateProtocols();
                    return Success("");
                default:
                    return Error(CliErrorType.InvalidParameter, "Expected 'up' or 'down'");
            }
        }

        private string ShowInterfaces(ICliContext context)
        {
            var sb = new StringBuilder();
            foreach (var iface in context.Device.GetAllInterfaces().Values)
            {
                sb.AppendLine($"{iface.Name}: <{iface.GetStatus()}> mtu {iface.Mtu}");
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    sb.AppendLine($"    inet {iface.IpAddress}/{GetCidrFromMask(iface.SubnetMask)}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private int GetCidrFromMask(string mask)
        {
            if (string.IsNullOrEmpty(mask)) return 0;
            var parts = mask.Split('.');
            if (parts.Length != 4) return 0;

            uint maskInt = 0;
            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out byte b)) return 0;
                maskInt = (maskInt << 8) | b;
            }

            return 32 - (int)Math.Log2(~maskInt + 1);
        }
    }
}
