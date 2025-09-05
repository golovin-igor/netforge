using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco
{
    /// <summary>
    /// Cisco-specific vendor context implementation
    /// </summary>
    public class CiscoVendorContext(INetworkDevice device) : IVendorContext
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));
        private readonly CiscoVendorCapabilities _capabilities = new(device);

        public string VendorName => "Cisco";
        public IVendorCapabilities Capabilities => _capabilities;
        public ICandidateConfiguration? CandidateConfig => null; // Cisco doesn't use candidate config

        public bool IsInMode(string mode)
        {
            var currentMode = _device.GetCurrentMode();
            return currentMode.Equals(mode, StringComparison.OrdinalIgnoreCase);
        }

        public string GetModePrompt()
        {
            return _device.GetPrompt();
        }

        public string GetCommandHelp(string command)
        {
            return command.ToLower() switch
            {
                "show" => "Display device information",
                "configure" => "Enter configuration mode",
                "interface" => "Configure an interface",
                "router" => "Configure routing protocol",
                "enable" => "Enter privileged mode",
                "disable" => "Exit privileged mode",
                "exit" => "Exit current mode",
                "ping" => "Send ping packets to test connectivity",
                "write" => "Save configuration",
                "reload" => "Restart the system",
                _ => $"No help available for '{command}'"
            };
        }

        public IEnumerable<string> GetCommandCompletions(string[] commandParts)
        {
            if (commandParts.Length == 0)
                return GetModeBasedCompletions();

            var currentMode = _device.GetCurrentMode().ToLower();
            var firstCommand = commandParts[0].ToLower();

            // Handle multi-level completions based on command context
            return firstCommand switch
            {
                "show" => GetShowCompletions(commandParts),
                "configure" => GetConfigureCompletions(commandParts),
                "interface" => GetInterfaceCompletions(commandParts),
                "router" => GetRouterCompletions(commandParts),
                "ip" => GetIpCompletions(commandParts),
                "vlan" => GetVlanCompletions(commandParts),
                "access-list" => GetAccessListCompletions(commandParts),
                "ping" => GetPingCompletions(commandParts),
                "copy" => GetCopyCompletions(commandParts),
                "no" => GetNoCompletions(commandParts),
                _ => GetModeBasedCompletions()
            };
        }

        private IEnumerable<string> GetModeBasedCompletions()
        {
            var currentMode = _device.GetCurrentMode().ToLower();
            return currentMode switch
            {
                "user" => new[] { "enable", "ping", "show", "exit" },
                "privileged" => new[] { "configure", "show", "ping", "write", "reload", "disable", "exit", "copy", "debug", "undebug" },
                "config" => new[] { "interface", "router", "hostname", "ip", "vlan", "access-list", "line", "banner", "service", "no", "exit", "end" },
                "interface" => new[] { "ip", "shutdown", "no", "description", "switchport", "spanning-tree", "exit" },
                "router" => new[] { "network", "neighbor", "router-id", "version", "auto-summary", "redistribute", "default-information", "exit" },
                "line" => new[] { "password", "login", "transport", "access-class", "exec-timeout", "exit" },
                _ => Array.Empty<string>()
            };
        }

        private IEnumerable<string> GetShowCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "version", "interfaces", "running-config", "startup-config", "ip", "arp", "mac", "vlan", "spanning-tree", "cdp", "flash", "memory", "processes", "users", "clock", "history" };

            if (commandParts.Length == 2)
            {
                return commandParts[1].ToLower() switch
                {
                    "ip" => new[] { "route", "arp", "interface", "ospf", "bgp", "eigrp", "rip", "protocols" },
                    "interfaces" => GetInterfaceNames(),
                    "vlan" => new[] { "brief", "id" },
                    "spanning-tree" => new[] { "brief", "root", "interface" },
                    "cdp" => new[] { "neighbors", "entry", "interface", "traffic" },
                    "mac" => new[] { "address-table" },
                    _ => Array.Empty<string>()
                };
            }

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetConfigureCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "terminal" };

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetInterfaceCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return GetInterfaceNames();

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetRouterCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "ospf", "bgp", "eigrp", "rip", "isis" };

            if (commandParts.Length == 2)
            {
                return commandParts[1].ToLower() switch
                {
                    "ospf" => new[] { "1", "100", "200" }, // Common OSPF process IDs
                    "bgp" => new[] { "65001", "65002", "1", "100" }, // Common BGP AS numbers
                    "eigrp" => new[] { "1", "100", "200" }, // Common EIGRP AS numbers
                    _ => Array.Empty<string>()
                };
            }

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetIpCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "address", "route", "default-gateway", "domain-name", "name-server", "host", "routing" };

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetVlanCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "1", "10", "20", "30", "100", "200", "300", "999" }; // Common VLAN IDs

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetAccessListCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "1", "10", "20", "100", "101", "102" }; // Common ACL numbers

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetPingCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "127.0.0.1", "8.8.8.8", "google.com", "localhost" }; // Common ping targets

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetCopyCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "running-config", "startup-config", "flash:", "tftp:", "ftp:" };

            if (commandParts.Length == 2)
                return new[] { "running-config", "startup-config", "flash:", "tftp:", "ftp:" };

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetNoCompletions(string[] commandParts)
        {
            if (commandParts.Length == 1)
                return new[] { "ip", "shutdown", "vlan", "access-list", "router", "interface", "hostname" };

            return Array.Empty<string>();
        }

        private IEnumerable<string> GetInterfaceNames()
        {
            // Common Cisco interface names and aliases
            var interfaces = new List<string>();

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

        public string PreprocessCommand(string command)
        {
            // Handle Cisco-specific command preprocessing
            return command.Trim();
        }

        public string PostprocessOutput(string output)
        {
            // Ensure output ends with proper newline for Cisco format
            if (!string.IsNullOrEmpty(output) && !output.EndsWith('\n'))
                return output + '\n';
            return output;
        }

        public string RenderConfiguration(object configData)
        {
            // Cisco-specific configuration rendering
            return configData?.ToString() ?? "";
        }

        public string GetCurrentInterface()
        {
            // Get the currently selected interface for configuration
            return _device.GetCurrentInterface();
        }
    }


}
