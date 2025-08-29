using System.Text;
using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Arista.Configuration
{
    /// <summary>
    /// Arista configure command handler
    /// </summary>
    public class ConfigureCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigureCommandHandler() : base("configure", "Enter configuration mode")
        {
            AddAlias("conf");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            // Handle 'configure terminal' or just 'configure'
            if (context.CommandParts.Length > 1 && context.CommandParts[1] != "terminal")
            {
                return Error(CliErrorType.InvalidCommand,
                    "% Invalid configure command");
            }

            SetMode(context, "config");
            return Success("");
        }
    }

    /// <summary>
    /// Arista exit command handler
    /// </summary>
    public class ExitCommandHandler() : VendorAgnosticCliHandler("exit", "Exit current mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            var currentMode = GetCurrentMode(context);

            var newMode = currentMode switch
            {
                "config" => "privileged",
                "interface" => "config",
                "router" => "config",
                "vlan" => "config",
                "privileged" => "user",
                _ => "user"
            };

            SetMode(context, newMode);
            return Success("");
        }
    }

    /// <summary>
    /// Arista interface command handler
    /// </summary>
    public class InterfaceCommandHandler : VendorAgnosticCliHandler
    {
        public InterfaceCommandHandler() : base("interface", "Configure an interface")
        {
            AddAlias("int");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires configuration mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need interface name");
            }

            var interfaceName = context.CommandParts[1];

            // Validate interface name format
            if (!IsValidInterfaceName(interfaceName))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid interface name: {interfaceName}");
            }

            // Set current interface and switch to interface mode
            SetCurrentInterface(context, interfaceName);
            SetMode(context, "interface");

            return Success("");
        }

        private bool IsValidInterfaceName(string name)
        {
            // Common Arista interface naming patterns
            var patterns = new[] { "Ethernet", "Management", "Vlan", "Port-Channel", "Loopback", "Vxlan" };
            return patterns.Any(pattern => name.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Arista hostname command handler
    /// </summary>
    public class HostnameCommandHandler() : VendorAgnosticCliHandler("hostname", "Set device hostname")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires configuration mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need hostname");
            }

            var hostname = context.CommandParts[1];

            // Validate hostname
            if (!IsValidHostname(hostname))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid hostname: {hostname}");
            }

            // Set hostname
            var device = context.Device;
            if (device != null)
            {
                device.SetHostname(hostname);
                device.AddLogEntry($"Hostname changed to {hostname}");
            }

            return Success("");
        }

        private bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname) || hostname.Length > 63)
                return false;

            return hostname.All(c => char.IsLetterOrDigit(c) || c == '-') &&
                   !hostname.StartsWith('-') && !hostname.EndsWith('-');
        }
    }

    /// <summary>
    /// Arista VLAN command handler
    /// </summary>
    public class VlanCommandHandler() : VendorAgnosticCliHandler("vlan", "Configure VLAN")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires configuration mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need VLAN ID");
            }

            var vlanId = context.CommandParts[1];

            // Validate VLAN ID
            if (!int.TryParse(vlanId, out int vlanNumber) || vlanNumber < 1 || vlanNumber > 4094)
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid VLAN ID: {vlanId}");
            }

            // Set current VLAN and switch to VLAN mode
            SetCurrentVlan(context, vlanNumber);
            SetMode(context, "vlan");

            return Success("");
        }

        private void SetCurrentVlan(ICliContext context, int vlanId)
        {
            var device = context.Device;
            if (device != null)
            {
                // Create or select VLAN using vendor capabilities
                var capabilities = GetVendorCapabilities(context);
                capabilities?.CreateOrSelectVlan(vlanId);
                device.AddLogEntry($"Entered VLAN {vlanId} configuration mode");
            }
        }
    }

    /// <summary>
    /// Arista no command handler
    /// </summary>
    public class NoCommandHandler() : VendorAgnosticCliHandler("no", "Negate a command")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need command to negate");
            }

            var command = context.CommandParts[1];

            return command switch
            {
                "shutdown" => HandleNoShutdown(context),
                "ip" => HandleNoIp(context),
                "hostname" => HandleNoHostname(context),
                _ => Error(CliErrorType.InvalidCommand,
                    $"% Cannot negate command: {command}")
            };
        }

        private CliResult HandleNoShutdown(ICliContext context)
        {
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires interface mode");
            }

            var device = context.Device;
            var interfaceName = device?.GetCurrentInterface();

            if (device != null && !string.IsNullOrEmpty(interfaceName))
            {
                var interfaces = device.GetAllInterfaces();
                if (interfaces.ContainsKey(interfaceName))
                {
                    interfaces[interfaceName].IsShutdown = false;
                    device.AddLogEntry($"Interface {interfaceName} enabled");
                }
            }

            return Success("");
        }

        private CliResult HandleNoIp(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Success(""); // Generic no ip command
            }

            var ipCommand = context.CommandParts[2];

            if (ipCommand == "address" && IsInMode(context, "interface"))
            {
                var device = context.Device;
                var interfaceName = device?.GetCurrentInterface();

                if (device != null && !string.IsNullOrEmpty(interfaceName))
                {
                    var interfaces = device.GetAllInterfaces();
                    if (interfaces.ContainsKey(interfaceName))
                    {
                        interfaces[interfaceName].IpAddress = "";
                        interfaces[interfaceName].SubnetMask = "";
                        device.AddLogEntry($"IP address removed from interface {interfaceName}");
                    }
                }
            }

            return Success("");
        }

        private CliResult HandleNoHostname(ICliContext context)
        {
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires configuration mode");
            }

            var device = context.Device;
            if (device != null)
            {
                device.SetHostname("Switch"); // Default name
                device.AddLogEntry("Hostname reset to default");
            }

            return Success("");
        }
    }

    /// <summary>
    /// Arista ip command handler for configuration mode
    /// </summary>
    public class IpCommandHandler() : VendorAgnosticCliHandler("ip", "IP configuration commands")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires configuration mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command");
            }

            var subCommand = context.CommandParts[1];

            if (subCommand.Equals("access-list", StringComparison.OrdinalIgnoreCase))
            {
                return HandleAccessList(context);
            }

            if (subCommand.Equals("route", StringComparison.OrdinalIgnoreCase))
            {
                return HandleStaticRoute(context);
            }

            return Error(CliErrorType.InvalidCommand,
                "% Invalid IP command");
        }

        private CliResult HandleAccessList(ICliContext context)
        {
            if (context.CommandParts.Length < 4)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need access list type and name");
            }

            var aclType = context.CommandParts[2]; // standard or extended
            var aclName = context.CommandParts[3];

            if (!aclType.Equals("standard", StringComparison.OrdinalIgnoreCase) &&
                !aclType.Equals("extended", StringComparison.OrdinalIgnoreCase))
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid access list type");
            }

            // Enter ACL configuration mode
            SetMode(context, "acl");

            // Store ACL context
            if (context.Device is {} device)
            {
                device.AddLogEntry($"Entered {aclType} access list {aclName}");
            }

            return Success("");
        }

        private CliResult HandleStaticRoute(ICliContext context)
        {
            if (context.CommandParts.Length < 5)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need network, mask, and next-hop");
            }

            var network = context.CommandParts[2];
            var mask = context.CommandParts[3];
            var nextHop = context.CommandParts[4];

            if (context.Device is {} device)
            {
                device.AddLogEntry($"Static route added: {network}/{mask} via {nextHop}");
            }

            return Success("");
        }
    }
}
