using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.F5
{
    /// <summary>
    /// F5 BIG-IP vendor capabilities implementation
    /// </summary>
    public class F5VendorCapabilities(INetworkDevice device) : IVendorCapabilities
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));
        private readonly HashSet<string> _supportedCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            // Basic commands
            "enable", "disable", "exit", "quit", "help", "?", "ping", "traceroute",

            // Show commands
            "show", "show running-config", "show startup-config", "show version",
            "show interfaces", "show ip interface", "show ip route", "show arp",
            "show vlan", "show trunk", "show spanning-tree", "show cdp",

            // Configuration commands
            "configure", "configure terminal", "hostname", "interface", "ip",
            "no", "shutdown", "no shutdown", "description", "ip address",

            // F5 BIG-IP specific commands
            "tmsh", "bash", "list", "create", "modify", "delete", "show ltm",
            "show gtm", "show asm", "show apm", "show net", "show sys",
            "create ltm pool", "create ltm virtual", "create ltm node",
            "modify ltm pool", "modify ltm virtual", "modify ltm node",
            "delete ltm pool", "delete ltm virtual", "delete ltm node",
            "show ltm pool", "show ltm virtual", "show ltm node"
        };
        private readonly Dictionary<string, string> _helpTexts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["tmsh"] = "Enter TMSH (Traffic Management Shell) mode",
            ["bash"] = "Enter bash shell mode",
            ["list"] = "List objects in current context",
            ["create"] = "Create a new object",
            ["modify"] = "Modify an existing object",
            ["delete"] = "Delete an object",
            ["show ltm"] = "Show LTM (Local Traffic Manager) objects",
            ["show gtm"] = "Show GTM (Global Traffic Manager) objects",
            ["show asm"] = "Show ASM (Application Security Manager) objects",
            ["show apm"] = "Show APM (Access Policy Manager) objects",
            ["show net"] = "Show network configuration",
            ["show sys"] = "Show system configuration"
        };

        // Basic commands
        // Show commands
        // Configuration commands
        // F5 BIG-IP specific commands

        public string VendorName => "F5";
        public int Priority => 160;

        public bool SupportsCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return false;

            // Check exact match first
            if (_supportedCommands.Contains(command))
                return true;

            // Check if command starts with any supported command
            return _supportedCommands.Any(cmd => command.StartsWith(cmd, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetSupportedCommands()
        {
            return _supportedCommands.ToList();
        }

        public string GetHelpText(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "Usage: <command> [options]";

            // Try to find exact match first
            if (_helpTexts.TryGetValue(command, out var helpText))
                return helpText;

            // Try to find partial match
            var partialMatch = _helpTexts.Keys.FirstOrDefault(key =>
                command.StartsWith(key, StringComparison.OrdinalIgnoreCase));

            if (partialMatch != null)
                return _helpTexts[partialMatch];

            // Default help text
            return $"Command '{command}' is supported but no specific help available.";
        }

        public bool SupportsDeviceType(string deviceType)
        {
            var supportedTypes = new[] { "load-balancer", "adc", "firewall", "router" };
            return supportedTypes.Any(type => type.Equals(deviceType, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "load-balancer", "adc", "firewall", "router" };
        }

        public bool SupportsProtocol(string protocol)
        {
            var supportedProtocols = new[] { "http", "https", "tcp", "udp", "ftp", "smtp", "dns" };
            return supportedProtocols.Any(p => p.Equals(protocol, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetSupportedProtocols()
        {
            return new[] { "http", "https", "tcp", "udp", "ftp", "smtp", "dns" };
        }

        public int GetCidrFromMask(string subnetMask)
        {
            if (string.IsNullOrWhiteSpace(subnetMask))
                return 0;

            var parts = subnetMask.Split('.');
            if (parts.Length != 4)
                return 0;

            if (!int.TryParse(parts[0], out var octet1) ||
                !int.TryParse(parts[1], out var octet2) ||
                !int.TryParse(parts[2], out var octet3) ||
                !int.TryParse(parts[3], out var octet4))
                return 0;

            var mask = (octet1 << 24) | (octet2 << 16) | (octet3 << 8) | octet4;
            var cidr = 32 - (int)Math.Log2(~mask + 1);

            return Math.Max(0, Math.Min(32, cidr));
        }

        public string GetVendorSpecificInfo()
        {
            return "F5 BIG-IP - Application Delivery Controller with advanced load balancing, SSL termination, and application security features.";
        }

        // Implement required interface methods with default implementations
        public string GetRunningConfiguration() => "";
        public string GetStartupConfiguration() => "";
        public void SetDeviceMode(string mode) { }
        public string GetDeviceMode() => "operational";
        public bool SupportsMode(string mode) => true;
        public IEnumerable<string> GetAvailableModes() => new[] { "operational", "config", "tmsh", "bash" };
        public string FormatCommandOutput(string command, object? output) => output?.ToString() ?? "";
        public string GetVendorErrorMessage(string errorCode, string? context) => $"F5 Error: {errorCode}";
        public bool SupportsFeature(string feature) => true;
        public string FormatInterfaceName(string interfaceName) => interfaceName;
        public bool ValidateVendorSyntax(string[] commandParts, string vendorName) => true;
        public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask) => true;
        public bool RemoveInterfaceIp(string interfaceName) => true;
        public bool ApplyAccessGroup(string direction, int aclNumber, string interfaceName) => true;
        public bool RemoveAccessGroup(string interfaceName) => true;
        public bool SetInterfaceShutdown(string interfaceName, bool shutdown) => true;
        public bool CreateOrSelectVlan(int vlanId) => true;
        public bool InitializeOspf(int processId) => true;
        public bool InitializeBgp(int asNumber) => true;
        public bool InitializeRip() => true;
        public bool InitializeEigrp(int asNumber) => true;
        public bool SetCurrentRouterProtocol(string protocol) => true;
        public bool AddAclEntry(int aclNumber, object entry) => true;
        public bool SetCurrentAclNumber(int aclNumber) => true;
        public int GetCurrentAclNumber() => 0;
        public bool AppendToRunningConfig(string configLine) => true;
        public bool AddInterfaceToVlan(string interfaceName, int vlanId) => true;
        public bool VlanExists(int vlanId) => false;
        public bool SetVlanName(int vlanId, string name) => true;
        public bool SetStpMode(string mode) => true;
        public bool SetStpVlanPriority(int vlanId, int priority) => true;
        public bool SetStpPriority(int priority) => true;
        public bool EnablePortfast(string interfaceName) => true;
        public bool DisablePortfast(string interfaceName) => true;
        public bool EnablePortfastDefault() => true;
        public bool EnableBpduGuard(string interfaceName) => true;
        public bool DisableBpduGuard(string interfaceName) => true;
        public bool EnableBpduGuardDefault() => true;
        public bool CreateOrUpdatePortChannel(int channelNumber, string protocol, string interfaces) => true;
        public bool EnableCdpGlobal() => true;
        public bool DisableCdpGlobal() => true;
        public bool EnableCdpInterface(string interfaceName) => true;
        public bool DisableCdpInterface(string interfaceName) => true;
        public bool SetCdpTimer(int seconds) => true;
        public bool SetCdpHoldtime(int seconds) => true;
        public bool SetHostname(string hostname) => true;
        public bool SetInterfaceDescription(string interfaceName, string description) => true;
        public bool SetSwitchportMode(string interfaceName, string mode) => true;
        public bool SetInterfaceVlan(string interfaceName, int vlanId) => true;
        public bool SetCurrentInterface(string interfaceName) => true;
        public bool SetInterfaceState(string interfaceName, string state) => true;
        public bool SetInterface(string property, string value, object config) => true;
        public bool SaveConfiguration() => true;
        public bool ReloadDevice() => true;
    }
}
