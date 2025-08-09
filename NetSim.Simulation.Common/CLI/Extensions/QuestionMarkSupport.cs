using NetSim.Simulation.Interfaces;
using System.Text;

namespace NetSim.Simulation.CliHandlers.Extensions
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
                help.AppendLine($"{cmdName} - {cmdDesc}");
                help.AppendLine();
            }
            
            // Add available sub-commands
            var subCommands = handler.GetSubCommands(context);
            if (subCommands.Any())
            {
                help.AppendLine("Available options:");
                foreach (var (subCmd, subDesc) in subCommands.OrderBy(x => x.Item1))
                {
                    help.AppendLine($"  {subCmd,-15} {subDesc}");
                }
                help.AppendLine();
            }
            
            // Add vendor-specific help
            if (context.VendorContext != null)
            {
                var vendorHelp = GetVendorSpecificHelp(context);
                if (!string.IsNullOrEmpty(vendorHelp))
                {
                    help.AppendLine(vendorHelp);
                    help.AppendLine();
                }
            }
            
            // Add contextual information
            help.AppendLine(GetContextualInformation(context));
            
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
            
            help.AppendLine($"Vendor: {vendorContext.VendorName}");
            
            // Add vendor-specific command completions
            var vendorCompletions = vendorContext.GetCommandCompletions(context.CommandParts);
            if (vendorCompletions.Any())
            {
                help.AppendLine("Vendor-specific options:");
                foreach (var completion in vendorCompletions.Take(10))
                {
                    help.AppendLine($"  {completion}");
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
            info.AppendLine($"Current mode: {currentMode}");
            
            // Add syntax if we have command parts
            if (context.CommandParts.Length > 0)
            {
                info.AppendLine($"Syntax: {string.Join(" ", context.CommandParts)} [options]");
            }
            
            // Add help instructions
            info.AppendLine();
            info.AppendLine("Help usage:");
            info.AppendLine("  command ?           - Show help for command");
            info.AppendLine("  command subcommand ? - Show help for subcommand");
            info.AppendLine("  <TAB>               - Command completion");
            
            return info.ToString();
        }
        
        /// <summary>
        /// Gets mode-specific help information
        /// </summary>
        public static string GetModeSpecificHelp(this CliContext context)
        {
            var help = new StringBuilder();
            var currentMode = context.CurrentMode.ToLower();
            
            help.AppendLine($"Available commands in {currentMode} mode:");
            
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
                help.AppendLine($"  {cmd,-15} {desc}");
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
                help.AppendLine("Interface command help:");
                help.AppendLine();
                help.AppendLine("Available interface types:");
                help.AppendLine("  ethernet       - Ethernet interface");
                help.AppendLine("  fastethernet   - Fast Ethernet interface");
                help.AppendLine("  gigabitethernet - Gigabit Ethernet interface");
                help.AppendLine("  serial         - Serial interface");
                help.AppendLine("  loopback       - Loopback interface");
                help.AppendLine("  vlan           - VLAN interface");
                help.AppendLine();
                help.AppendLine("Syntax: interface <type><number>");
                help.AppendLine("Example: interface gigabitethernet0/0");
            }
            else
            {
                help.AppendLine($"Interface {interfaceName} configuration:");
                help.AppendLine();
                help.AppendLine("Available commands:");
                help.AppendLine("  ip address     - Set IP address");
                help.AppendLine("  shutdown       - Shutdown interface");
                help.AppendLine("  no shutdown    - Enable interface");
                help.AppendLine("  description    - Set interface description");
                help.AppendLine("  switchport     - Configure switchport");
                help.AppendLine("  spanning-tree  - Configure spanning tree");
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
                help.AppendLine("VLAN command help:");
                help.AppendLine();
                help.AppendLine("Syntax: vlan <vlan-id>");
                help.AppendLine("Valid VLAN IDs: 1-4094");
                help.AppendLine();
                help.AppendLine("Examples:");
                help.AppendLine("  vlan 10        - Configure VLAN 10");
                help.AppendLine("  vlan 100       - Configure VLAN 100");
            }
            else
            {
                help.AppendLine($"VLAN {vlanId} configuration:");
                help.AppendLine();
                help.AppendLine("Available commands:");
                help.AppendLine("  name          - Set VLAN name");
                help.AppendLine("  state         - Set VLAN state (active/suspend)");
                help.AppendLine("  exit          - Exit VLAN configuration");
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
                help.AppendLine("Routing protocol help:");
                help.AppendLine();
                help.AppendLine("Available protocols:");
                help.AppendLine("  ospf          - Open Shortest Path First");
                help.AppendLine("  bgp           - Border Gateway Protocol");
                help.AppendLine("  eigrp         - Enhanced Interior Gateway Routing Protocol");
                help.AppendLine("  rip           - Routing Information Protocol");
                help.AppendLine();
                help.AppendLine("Syntax: router <protocol> [process-id/as-number]");
            }
            else
            {
                help.AppendLine($"{protocol.ToUpper()} configuration help:");
                help.AppendLine();
                
                switch (protocol.ToLower())
                {
                    case "ospf":
                        help.AppendLine("OSPF Commands:");
                        help.AppendLine("  network       - Configure OSPF network");
                        help.AppendLine("  router-id     - Set OSPF router ID");
                        help.AppendLine("  area          - Configure OSPF area");
                        break;
                    case "bgp":
                        help.AppendLine("BGP Commands:");
                        help.AppendLine("  neighbor      - Configure BGP neighbor");
                        help.AppendLine("  network       - Configure BGP network");
                        help.AppendLine("  router-id     - Set BGP router ID");
                        break;
                    case "eigrp":
                        help.AppendLine("EIGRP Commands:");
                        help.AppendLine("  network       - Configure EIGRP network");
                        help.AppendLine("  auto-summary  - Enable auto-summary");
                        help.AppendLine("  variance      - Set EIGRP variance");
                        break;
                    case "rip":
                        help.AppendLine("RIP Commands:");
                        help.AppendLine("  network       - Configure RIP network");
                        help.AppendLine("  version       - Set RIP version");
                        help.AppendLine("  auto-summary  - Enable auto-summary");
                        break;
                }
            }
            
            return help.ToString();
        }
    }
} 
