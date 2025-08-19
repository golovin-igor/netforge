using System.Globalization;
using NetForge.Simulation.Interfaces;
using System.Text;

namespace NetForge.Simulation.CliHandlers.Extensions
{
    /// <summary>
    /// Extensions for enhanced question mark (?) help support across all vendors
    /// </summary>
    public static class QuestionMarkSupport
    {
        /// <summary>
        /// Gets formatted help text for any command with question mark
        /// </summary>
        public static string GetQuestionMarkHelp(this ICliHandler handler, CliContext context)
        {
            var help = new StringBuilder();

            // Add command header
            var info = handler.GetCommandInfo();
            if (info.HasValue)
            {
                var (cmdName, cmdDesc) = info.Value;
                help.Append($"{cmdName} - {cmdDesc}").AppendLine();
                help.AppendLine();
            }

            // Add available sub-commands
            var subCommands = handler.GetSubCommands(context);
            if (subCommands.Count != 0)
            {
                help.Append("Available options:").AppendLine();
                foreach (var (subCmd, subDesc) in subCommands.OrderBy(x => x.Item1))
                {
                    help.Append($"  {subCmd,-15} {subDesc}").AppendLine();
                }
                help.AppendLine();
            }

            // Add vendor-specific help
            if (context.VendorContext != null)
            {
                var vendorHelp = GetVendorSpecificHelp(context);
                if (!string.IsNullOrEmpty(vendorHelp))
                {
                    help.Append(vendorHelp).AppendLine();
                    help.AppendLine();
                }
            }

            // Add contextual information
            help.Append(GetContextualInformation(context)).AppendLine();

            return help.ToString();
        }

        /// <summary>
        /// Gets vendor-specific help information
        /// </summary>
        private static string GetVendorSpecificHelp(CliContext context)
        {
            var help = new StringBuilder();
            var vendorContext = context.VendorContext;

            if (vendorContext == null) return "";

            help.Append($"Vendor: {vendorContext.VendorName}").AppendLine();

            // Add vendor-specific command completions
            var vendorCompletions = vendorContext.GetCommandCompletions(context.CommandParts);
            if (vendorCompletions.Any())
            {
                help.Append("Vendor-specific options:").AppendLine();
                foreach (var completion in vendorCompletions.Take(10))
                {
                    help.Append($"  {completion}").AppendLine();
                }
            }

            return help.ToString();
        }

        /// <summary>
        /// Gets contextual information for the current command
        /// </summary>
        private static string GetContextualInformation(CliContext context)
        {
            var info = new StringBuilder();

            // Add current mode
            var currentMode = context.CurrentMode;
            info.Append($"Current mode: {currentMode}").AppendLine();

            // Add syntax if we have command parts
            if (context.CommandParts.Length > 0)
            {
                info.Append($"Syntax: {string.Join(" ", context.CommandParts)} [options]").AppendLine();
            }

            // Add help instructions
            info.AppendLine();
            info.Append("Help usage:").AppendLine();
            info.Append("  command ?           - Show help for command").AppendLine();
            info.Append("  command subcommand ? - Show help for subcommand").AppendLine();
            info.Append("  <TAB>               - Command completion").AppendLine();

            return info.ToString();
        }

        /// <summary>
        /// Gets mode-specific help information
        /// </summary>
        public static string GetModeSpecificHelp(this CliContext context)
        {
            var help = new StringBuilder();
            var currentMode = context.CurrentMode.ToLowerInvariant();

            help.Append($"Available commands in {currentMode} mode:").AppendLine();

            var modeCommands = currentMode switch
            {
                "user" => new Dictionary<string, string>
                {
                    ["enable"] = "Enter privileged mode",
                    ["ping"] = "Send ICMP echo requests",
                    ["show"] = "Display system information",
                    ["exit"] = "Exit the CLI",
                    ["help"] = "Show help information"
                },
                "privileged" => new Dictionary<string, string>
                {
                    ["configure"] = "Enter configuration mode",
                    ["show"] = "Display system information",
                    ["ping"] = "Send ICMP echo requests",
                    ["write"] = "Save configuration",
                    ["reload"] = "Restart the system",
                    ["disable"] = "Exit to user mode",
                    ["copy"] = "Copy files or configurations",
                    ["debug"] = "Enable debugging",
                    ["exit"] = "Exit the CLI"
                },
                "config" => new Dictionary<string, string>
                {
                    ["interface"] = "Configure interface",
                    ["router"] = "Configure routing protocol",
                    ["hostname"] = "Set system hostname",
                    ["ip"] = "Configure IP parameters",
                    ["vlan"] = "Configure VLAN",
                    ["access-list"] = "Configure access list",
                    ["line"] = "Configure line parameters",
                    ["no"] = "Negate a command",
                    ["exit"] = "Exit configuration mode",
                    ["end"] = "Exit to privileged mode"
                },
                "interface" => new Dictionary<string, string>
                {
                    ["ip"] = "Configure IP parameters",
                    ["shutdown"] = "Shutdown interface",
                    ["no"] = "Negate a command",
                    ["description"] = "Set interface description",
                    ["switchport"] = "Configure switchport",
                    ["spanning-tree"] = "Configure spanning tree",
                    ["exit"] = "Exit interface mode"
                },
                "router" => new Dictionary<string, string>
                {
                    ["network"] = "Configure network",
                    ["neighbor"] = "Configure neighbor",
                    ["router-id"] = "Set router ID",
                    ["version"] = "Set protocol version",
                    ["redistribute"] = "Redistribute routes",
                    ["exit"] = "Exit router mode"
                },
                _ => new Dictionary<string, string>
                {
                    ["?"] = "Show available commands",
                    ["exit"] = "Exit current mode"
                }
            };

            foreach (var (cmd, desc) in modeCommands.OrderBy(x => x.Key))
            {
                help.Append($"  {cmd,-15} {desc}").AppendLine();
            }

            return help.ToString();
        }

        /// <summary>
        /// Gets interface-specific help for interface commands
        /// </summary>
        public static string GetInterfaceHelp(this CliContext context, string interfaceName = "")
        {
            var help = new StringBuilder();

            if (string.IsNullOrEmpty(interfaceName))
            {
                help.Append("Interface command help:").AppendLine();
                help.AppendLine();
                help.Append("Available interface types:").AppendLine();
                help.Append("  ethernet       - Ethernet interface").AppendLine();
                help.Append("  fastethernet   - Fast Ethernet interface").AppendLine();
                help.Append("  gigabitethernet - Gigabit Ethernet interface").AppendLine();
                help.Append("  serial         - Serial interface").AppendLine();
                help.Append("  loopback       - Loopback interface").AppendLine();
                help.Append("  vlan           - VLAN interface").AppendLine();
                help.AppendLine();
                help.Append("Syntax: interface <type><number>").AppendLine();
                help.Append("Example: interface gigabitethernet0/0").AppendLine();
            }
            else
            {
                help.Append($"Interface {interfaceName} configuration:").AppendLine();
                help.AppendLine();
                help.Append("Available commands:").AppendLine();
                help.Append("  ip address     - Set IP address").AppendLine();
                help.Append("  shutdown       - Shutdown interface").AppendLine();
                help.Append("  no shutdown    - Enable interface").AppendLine();
                help.Append("  description    - Set interface description").AppendLine();
                help.Append("  switchport     - Configure switchport").AppendLine();
                help.Append("  spanning-tree  - Configure spanning tree").AppendLine();
            }

            return help.ToString();
        }

        /// <summary>
        /// Gets VLAN-specific help for VLAN commands
        /// </summary>
        public static string GetVlanHelp(this CliContext context, string vlanId = "")
        {
            var help = new StringBuilder();

            if (string.IsNullOrEmpty(vlanId))
            {
                help.Append("VLAN command help:").AppendLine();
                help.AppendLine();
                help.Append("Syntax: vlan <vlan-id>").AppendLine();
                help.Append("Valid VLAN IDs: 1-4094").AppendLine();
                help.AppendLine();
                help.Append("Examples:").AppendLine();
                help.Append("  vlan 10        - Configure VLAN 10").AppendLine();
                help.Append("  vlan 100       - Configure VLAN 100").AppendLine();
            }
            else
            {
                help.Append($"VLAN {vlanId} configuration:").AppendLine();
                help.AppendLine();
                help.Append("Available commands:").AppendLine();
                help.Append("  name          - Set VLAN name").AppendLine();
                help.Append("  state         - Set VLAN state (active/suspend)").AppendLine();
                help.Append("  exit          - Exit VLAN configuration").AppendLine();
            }

            return help.ToString();
        }

        /// <summary>
        /// Gets protocol-specific help for routing protocols
        /// </summary>
        public static string GetProtocolHelp(this CliContext context, string protocol = "")
        {
            var help = new StringBuilder();

            if (string.IsNullOrEmpty(protocol))
            {
                help.Append("Routing protocol help:").AppendLine();
                help.AppendLine();
                help.Append("Available protocols:").AppendLine();
                help.Append("  ospf          - Open Shortest Path First").AppendLine();
                help.Append("  bgp           - Border Gateway Protocol").AppendLine();
                help.Append("  eigrp         - Enhanced Interior Gateway Routing Protocol").AppendLine();
                help.Append("  rip           - Routing Information Protocol").AppendLine();
                help.AppendLine();
                help.Append("Syntax: router <protocol> [process-id/as-number]").AppendLine();
            }
            else
            {
                var protocolToUpper = protocol.ToUpper(CultureInfo.InvariantCulture);
                help.Append($"{protocolToUpper} configuration help:");
                help.AppendLine();
                help.AppendLine();

                switch (protocol.ToLowerInvariant())
                {
                    case "ospf":
                        help.Append("OSPF Commands:").AppendLine();
                        help.Append("  network       - Configure OSPF network").AppendLine();
                        help.Append("  router-id     - Set OSPF router ID").AppendLine();
                        help.Append("  area          - Configure OSPF area").AppendLine();
                        break;
                    case "bgp":
                        help.Append("BGP Commands:").AppendLine();
                        help.Append("  neighbor      - Configure BGP neighbor").AppendLine();
                        help.Append("  network       - Configure BGP network").AppendLine();
                        help.Append("  router-id     - Set BGP router ID").AppendLine();
                        break;
                    case "eigrp":
                        help.Append("EIGRP Commands:").AppendLine();
                        help.Append("  network       - Configure EIGRP network").AppendLine();
                        help.Append("  auto-summary  - Enable auto-summary").AppendLine();
                        help.Append("  variance      - Set EIGRP variance").AppendLine();
                        break;
                    case "rip":
                        help.Append("RIP Commands:").AppendLine();
                        help.Append("  network       - Configure RIP network").AppendLine();
                        help.Append("  version       - Set RIP version").AppendLine();
                        help.Append("  auto-summary  - Enable auto-summary").AppendLine();
                        break;
                }
            }

            return help.ToString();
        }
    }
}
