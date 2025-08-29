using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Extreme.Show
{
    /// <summary>
    /// Extreme show command handler
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
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
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
                "iparp" => HandleShowIparp(context),
                "mlag" => HandleShowMlag(context),
                "igmp" => HandleShowIgmp(context),
                "rip" => HandleShowRip(context),
                "pim" => HandleShowPim(context),
                "log" => HandleShowLog(context),
                "access-list" => HandleShowAccessList(context),
                "ospf" => HandleShowOspf(context),
                "ports" => HandleShowPorts(context),
                "configuration" => HandleShowConfiguration(context),
                "ssh2" => HandleShowSsh2(context),
                "snmp" => HandleShowSnmp(context),
                "vlan" => HandleShowVlan(context),
                "iproute" => HandleShowIproute(context),
                "isis" => HandleShowIsis(context),
                "accounts" => HandleShowAccounts(context),
                "ipforwarding" => HandleShowIpforwarding(context),
                "ntp" => HandleShowNtp(context),
                "bgp" => HandleShowBgp(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid show option: {option}")
            };
        }

        private CliResult HandleShowVersion(ICliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();

            output.AppendLine($"Extreme Network Device");
            output.AppendLine($"Device name: {device?.Name}");
            output.AppendLine($"Software version: 1.0");
            output.AppendLine($"Hardware: Generic");
            output.AppendLine($"Uptime: 1 day, 0 hours, 0 minutes");

            return Success(output.ToString());
        }

        private CliResult HandleShowInterfaces(ICliContext context)
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

        private CliResult HandleShowIparp(ICliContext context)
        {
            var device = context.Device as NetworkDevice;
            var table = device?.GetArpTableOutput();
            if (string.IsNullOrEmpty(table))
                return Success("ARP table is empty.\n");
            return Success(table);
        }

        private CliResult HandleShowMlag(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("MLAG Status:");
            output.AppendLine("MLAG State: Active");
            output.AppendLine("Peer State: Up");
            output.AppendLine("Local Role: Primary");
            output.AppendLine("Peer Role: Secondary");

            return Success(output.ToString());
        }

        private CliResult HandleShowIgmp(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("IGMP Groups:");
            output.AppendLine("Interface   Group           Source          Age    Uptime   Last Reporter");
            output.AppendLine("Vlan100     224.0.0.1       *               10     00:01:30 192.168.1.10");

            return Success(output.ToString());
        }

        private CliResult HandleShowRip(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("RIP Interface Status:");
            output.AppendLine("Interface   State    Metric   Split-Horizon   Auth");
            output.AppendLine("Vlan100     Up       1        Enabled         None");
            output.AppendLine("Vlan200     Up       1        Enabled         None");

            return Success(output.ToString());
        }

        private CliResult HandleShowPim(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("PIM Neighbors:");
            output.AppendLine("Interface   Neighbor        Uptime    Expires   Mode");
            output.AppendLine("Vlan100     192.168.1.1     01:23:45  00:01:30  Sparse");

            return Success(output.ToString());
        }

        private CliResult HandleShowLog(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Log Configuration:");
            output.AppendLine("Syslog logging: Enabled");
            output.AppendLine("Console logging: Enabled");
            output.AppendLine("Facility: Local7");
            output.AppendLine("Severity: Info");

            return Success(output.ToString());
        }

        private CliResult HandleShowAccessList(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Access Control Lists:");
            output.AppendLine("ACL Name: Standard-ACL-1");
            output.AppendLine("  10 permit 192.168.1.0/24");
            output.AppendLine("  20 deny any");

            return Success(output.ToString());
        }

        private CliResult HandleShowOspf(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("OSPF Neighbors:");
            output.AppendLine("Neighbor ID     Pri   State      Dead Time   Address         Interface");
            output.AppendLine("192.168.1.1     1     Full/DR    00:00:35    192.168.1.1     Vlan100");

            return Success(output.ToString());
        }

        private CliResult HandleShowPorts(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Port Information:");
            output.AppendLine("Port   State    Speed    Duplex   Type");
            output.AppendLine("1:1    Up       1000M    Full     Copper");
            output.AppendLine("1:2    Down     Auto     Auto     Copper");

            return Success(output.ToString());
        }

        private CliResult HandleShowConfiguration(ICliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();

            output.AppendLine("# Configuration dump for Extreme device");
            output.AppendLine($"# Device: {device?.Name}");
            output.AppendLine("");
            output.AppendLine("configure snmp sysName \"ExtremeSwitch\"");
            output.AppendLine("configure snmp sysLocation \"Lab\"");
            output.AppendLine("configure snmp sysContact \"admin@example.com\"");
            output.AppendLine("");
            output.AppendLine("create vlan \"default\"");
            output.AppendLine("configure default ipaddress 192.168.1.100/24");

            return Success(output.ToString());
        }

        private CliResult HandleShowSsh2(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("SSH2 Status:");
            output.AppendLine("SSH2 Service: Enabled");
            output.AppendLine("Port: 22");
            output.AppendLine("Host Key: RSA 2048-bit");
            output.AppendLine("Active Sessions: 1");

            return Success(output.ToString());
        }

        private CliResult HandleShowSnmp(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("SNMP Community Strings:");
            output.AppendLine("Community   Access   Views");
            output.AppendLine("public      Read     Default");
            output.AppendLine("private     Write    Default");

            return Success(output.ToString());
        }

        private CliResult HandleShowVlan(ICliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();

            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var vlans = device.GetAllVlans();

            output.AppendLine("VLAN Configuration:");
            output.AppendLine("VLAN Name        Tag    Ports           Type");
            output.AppendLine("---- ----------- ------ --------------- --------");

            if (vlans.Count == 0)
            {
                output.AppendLine("1    default     1      1-24           Static");
            }
            else
            {
                foreach (var vlan in vlans.Values.OrderBy(v => v.Id))
                {
                    output.AppendLine($"{vlan.Id,-4} {vlan.Name,-11} {vlan.Id,-6} 1-24           Static");
                }
            }

            return Success(output.ToString());
        }

        private CliResult HandleShowIproute(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("IP Route Table:");
            output.AppendLine("Destination     Gateway         Interface   Metric   Type");
            output.AppendLine("0.0.0.0/0       192.168.1.1     Vlan100     1        Static");
            output.AppendLine("192.168.1.0/24  0.0.0.0         Vlan100     0        Direct");

            return Success(output.ToString());
        }

        private CliResult HandleShowIsis(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("IS-IS Adjacencies:");
            output.AppendLine("System ID       Interface   SNPA            State  Type  Priority  Circuit ID");
            output.AppendLine("0000.0000.0001  Vlan100     0012.3456.789a  Up     L2    64        01");

            return Success(output.ToString());
        }

        private CliResult HandleShowAccounts(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("User Accounts:");
            output.AppendLine("Username   Access Level   Last Login");
            output.AppendLine("admin      Full           2023-01-01 10:00:00");
            output.AppendLine("user       Read-Only      2023-01-01 09:30:00");

            return Success(output.ToString());
        }

        private CliResult HandleShowIpforwarding(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("IP Forwarding Status:");
            output.AppendLine("IPv4 Forwarding: Enabled");
            output.AppendLine("IPv6 Forwarding: Disabled");
            output.AppendLine("ICMP Redirects: Enabled");

            return Success(output.ToString());
        }

        private CliResult HandleShowNtp(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("NTP Status:");
            output.AppendLine("NTP Service: Enabled");
            output.AppendLine("Server: pool.ntp.org");
            output.AppendLine("Stratum: 3");
            output.AppendLine("Last Sync: 2023-01-01 10:00:00");

            return Success(output.ToString());
        }

        private CliResult HandleShowBgp(ICliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("BGP Neighbors:");
            output.AppendLine("Neighbor       AS      State      PfxRcd   Uptime");
            output.AppendLine("192.168.1.1    65001   Established  100    01:23:45");
            output.AppendLine("192.168.1.2    65002   Active       0      00:00:00");

            return Success(output.ToString());
        }
    }
}
