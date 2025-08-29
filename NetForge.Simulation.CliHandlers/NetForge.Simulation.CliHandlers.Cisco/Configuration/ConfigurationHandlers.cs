using System.Text;
using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration
{
    /// <summary>
    /// Cisco configure terminal command handler
    /// </summary>
    public class ConfigureCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigureCommandHandler() : base("configure", "Enter configuration mode")
        {
            AddAlias("conf");
            AddAlias("config");
        }

        public override List<string> GetCompletions(ICliContext context)
        {
            var completions = new List<string>();

            // If we're at the first level (just "configure"), return available options
            if (context.CommandParts.Length <= 1)
            {
                completions.Add("terminal");
                return completions;
            }

            return completions;
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // Check if we have terminal parameter (accept both "terminal" and "t")
            if (context.CommandParts.Length > 1 &&
                (context.CommandParts[1] == "terminal" || context.CommandParts[1] == "t"))
            {
                SetMode(context, "config");
                return Success("Enter configuration commands, one per line.  End with CNTL/Z.");
            }

            // If just "configure" without parameters, show help
            if (context.CommandParts.Length == 1)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Use 'configure terminal' to enter configuration mode.");
            }

            return Error(CliErrorType.InvalidCommand, GetVendorError(context, "invalid_command"));
        }
    }

    /// <summary>
    /// Cisco exit command handler
    /// </summary>
    public class ExitCommandHandler() : VendorAgnosticCliHandler("exit", "Exit from current mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var currentMode = context.Device.GetCurrentMode();

            // Handle mode transitions
            switch (currentMode)
            {
                case "config":
                    SetMode(context, "privileged");
                    break;
                case "privileged":
                    SetMode(context, "user");
                    break;
                case "interface":
                case "router":
                case "vlan":
                case "acl":
                    SetMode(context, "config");
                    break;
                default:
                    SetMode(context, "user");
                    break;
            }

            return Success("");
        }
    }

    /// <summary>
    /// Cisco interface command handler
    /// </summary>
    public class InterfaceCommandHandler : VendorAgnosticCliHandler
    {
        public InterfaceCommandHandler() : base("interface", "Enter interface configuration mode")
        {
            AddAlias("int");
        }

        public override List<string> GetCompletions(ICliContext context)
        {
            var completions = new List<string>();

            // If we're at the first level (just "interface"), return available interfaces
            if (context.CommandParts.Length <= 1)
            {
                completions.AddRange(GetAvailableInterfaces(context));
                return completions;
            }

            return completions;
        }

        private List<string> GetAvailableInterfaces(ICliContext context)
        {
            var interfaces = new List<string>();

            // Common Cisco interface names and aliases
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

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var interfaceName = string.Join(" ", context.CommandParts.Skip(1));

            // Validate interface name using the comprehensive alias handler
            if (!CiscoInterfaceAliasHandler.IsValidInterfaceName(interfaceName))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid interface name: {interfaceName}");
            }

            // Expand interface alias to canonical name
            var canonicalName = CiscoInterfaceAliasHandler.ExpandInterfaceAlias(interfaceName);

            // Set interface mode and store interface context
            SetMode(context, "interface");
            context.Device.SetCurrentInterface(canonicalName);

            return Success("");
        }
    }

    /// <summary>
    /// Cisco IP command handler
    /// </summary>
    public class IpCommandHandler() : VendorAgnosticCliHandler("ip", "Configure IP parameters")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var subCommand = context.CommandParts[1];

            return subCommand switch
            {
                "address" => HandleIpAddress(context),
                "route" => HandleIpRoute(context),
                "routing" => HandleIpRouting(context),
                _ => Error(CliErrorType.InvalidCommand,
                    GetVendorError(context, "invalid_command"))
            };
        }

        private CliResult HandleIpAddress(ICliContext context)
        {
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface configuration mode");
            }

            if (context.CommandParts.Length < 4)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need IP address and subnet mask");
            }

            var ipAddress = context.CommandParts[2];
            var subnetMask = context.CommandParts[3];

            // Get current interface from device context
            var interfaceName = context.Device.GetCurrentInterface();
            if (string.IsNullOrEmpty(interfaceName))
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error: No interface context available");
            }

            // Configure IP address on the interface
            var vendorContext = GetVendorContext(context);
            var success = vendorContext?.Capabilities.ConfigureInterfaceIp(interfaceName, ipAddress, subnetMask) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error: Failed to configure IP address on {interfaceName}");
            }

            return Success("");
        }

        private CliResult HandleIpRoute(ICliContext context)
        {
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            if (context.CommandParts.Length < 5)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need network, mask, and next hop");
            }

            var network = context.CommandParts[2];
            var mask = context.CommandParts[3];
            var nextHop = context.CommandParts[4];

            try
            {
                var device = context.Device;

                // Add the static route with default metric
                device?.AddStaticRoute(network, mask, nextHop, 1);

                // Log the route addition
                device?.AddLogEntry($"Static route added: {network}/{mask} via {nextHop}");

                return Success("");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error adding static route: {ex.Message}");
            }
        }

        private CliResult HandleIpRouting(ICliContext context)
        {
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            try
            {
                var device = context.Device;

                // Log IP routing enable
                device?.AddLogEntry("IP routing enabled");

                return Success("");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error enabling IP routing: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Cisco no command handler
    /// </summary>
    public class NoCommandHandler() : VendorAgnosticCliHandler("no", "Remove or disable configuration")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var subCommand = context.CommandParts[1];

            return subCommand switch
            {
                "ip" => HandleNoIp(context),
                "shutdown" => HandleNoShutdown(context),
                "description" => HandleNoDescription(context),
                _ => Error(CliErrorType.InvalidCommand,
                    GetVendorError(context, "invalid_command"))
            };
        }

        private CliResult HandleNoIp(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command");
            }

            var ipSubCommand = context.CommandParts[2];

            return ipSubCommand switch
            {
                "address" => HandleNoIpAddress(context),
                "route" => HandleNoIpRoute(context),
                "routing" => HandleNoIpRouting(context),
                _ => Error(CliErrorType.InvalidCommand,
                    GetVendorError(context, "invalid_command"))
            };
        }

        private CliResult HandleNoIpAddress(ICliContext context)
        {
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface configuration mode");
            }

            var vendorContext = GetVendorContext(context);
            var interfaceName = vendorContext?.GetCurrentInterface() ?? "";
            if (string.IsNullOrEmpty(interfaceName))
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error: No interface context available");
            }

            // Remove IP address from interface
            var success = vendorContext?.Capabilities.RemoveInterfaceIp(interfaceName) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error: Failed to remove IP address from {interfaceName}");
            }

            return Success("");
        }

        private CliResult HandleNoIpRoute(ICliContext context)
        {
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            if (context.CommandParts.Length < 6)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need network, mask, and next hop");
            }

            var network = context.CommandParts[3];
            var mask = context.CommandParts[4];
            var nextHop = context.CommandParts[5];

            try
            {
                var device = context.Device;

                // Remove the static route (using correct method signature)
                device?.RemoveStaticRoute(network, nextHop);

                // Log the route removal
                device?.AddLogEntry($"Static route removed: {network}/{mask} via {nextHop}");

                return Success("");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error removing static route: {ex.Message}");
            }
        }

        private CliResult HandleNoIpRouting(ICliContext context)
        {
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            try
            {
                var device = context.Device;

                // Log IP routing disable
                device?.AddLogEntry("IP routing disabled");

                return Success("");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error disabling IP routing: {ex.Message}");
            }
        }

        private CliResult HandleNoShutdown(ICliContext context)
        {
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface configuration mode");
            }

                        var vendorContext = GetVendorContext(context);
            var interfaceName = vendorContext?.GetCurrentInterface() ?? "";
            if (string.IsNullOrEmpty(interfaceName))
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error: No interface context available");
            }

            // Enable interface (no shutdown)
            var success = vendorContext?.Capabilities.SetInterfaceShutdown(interfaceName, false) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error: Failed to enable interface {interfaceName}");
            }

            return Success("");
        }

        private CliResult HandleNoDescription(ICliContext context)
        {
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface configuration mode");
            }

                        var vendorContext = GetVendorContext(context);
            var interfaceName = vendorContext?.GetCurrentInterface() ?? "";
            if (string.IsNullOrEmpty(interfaceName))
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error: No interface context available");
            }

            // Remove interface description - simulate for now
            var success = true;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error: Failed to remove description from {interfaceName}");
            }

            return Success("");
        }
    }

    /// <summary>
    /// Cisco hostname command handler
    /// </summary>
    public class HostnameCommandHandler() : VendorAgnosticCliHandler("hostname", "Set system's network name")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var newHostname = context.CommandParts[1];

            // Validate hostname (basic validation)
            if (string.IsNullOrWhiteSpace(newHostname))
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid hostname");
            }

            // Apply the hostname change using vendor-agnostic method
            context.Device.SetHostname(newHostname);
            return Success("");
        }
    }

    /// <summary>
    /// Cisco VLAN command handler
    /// </summary>
    public class VlanCommandHandler() : VendorAgnosticCliHandler("vlan", "Configure VLAN parameters")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: vlan <vlan-id>");
            }

            if (int.TryParse(context.CommandParts[1], out int vlanId))
            {
                if (vlanId >= 1 && vlanId <= 4094)
                {
                                // Create or select VLAN using vendor-agnostic method
            GetVendorContext(context)?.Capabilities.CreateOrSelectVlan(vlanId);
            SetMode(context, "vlan");
            return Success("");
                }
                else
                {
                    return Error(CliErrorType.InvalidParameter,
                        "% Invalid VLAN ID. Valid range is 1-4094");
                }
            }
            else
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid VLAN ID");
            }
        }
    }

    /// <summary>
    /// Cisco router command handler
    /// </summary>
    public class RouterCommandHandler : VendorAgnosticCliHandler
    {
        public RouterCommandHandler() : base("router", "Enable a routing process")
        {
            AddSubHandler("ospf", new RouterOspfHandler());
            AddSubHandler("bgp", new RouterBgpHandler());
            AddSubHandler("rip", new RouterRipHandler());
            AddSubHandler("eigrp", new RouterEigrpHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in config mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Options: ospf, bgp, rip, eigrp");
            }

            return Error(CliErrorType.InvalidCommand,
                GetVendorError(context, "invalid_command"));
        }
    }

    /// <summary>
    /// Router OSPF sub-handler
    /// </summary>
    public class RouterOspfHandler() : VendorAgnosticCliHandler("ospf", "Open Shortest Path First (OSPF)")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: router ospf <process-id>");
            }

            if (int.TryParse(context.CommandParts[1], out int processId))
            {
                            GetVendorContext(context)?.Capabilities.InitializeOspf(processId);
            SetMode(context, "router");
            GetVendorContext(context)?.Capabilities.SetCurrentRouterProtocol("ospf");
                return Success("");
            }
            else
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid OSPF process ID");
            }
        }
    }

    /// <summary>
    /// Router BGP sub-handler
    /// </summary>
    public class RouterBgpHandler() : VendorAgnosticCliHandler("bgp", "Border Gateway Protocol (BGP)")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: router bgp <as-number>");
            }

            if (int.TryParse(context.CommandParts[1], out int asNumber))
            {
                            GetVendorContext(context)?.Capabilities.InitializeBgp(asNumber);
            SetMode(context, "router");
            GetVendorContext(context)?.Capabilities.SetCurrentRouterProtocol("bgp");
                return Success("");
            }
            else
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid AS number");
            }
        }
    }

    /// <summary>
    /// Router RIP sub-handler
    /// </summary>
    public class RouterRipHandler() : VendorAgnosticCliHandler("rip", "Routing Information Protocol (RIP)")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            GetVendorContext(context)?.Capabilities.InitializeRip();
            SetMode(context, "router");
            GetVendorContext(context)?.Capabilities.SetCurrentRouterProtocol("rip");
            return Success("");
        }
    }

    /// <summary>
    /// Router EIGRP sub-handler
    /// </summary>
    public class RouterEigrpHandler() : VendorAgnosticCliHandler("eigrp", "Enhanced Interior Gateway Routing Protocol (EIGRP)")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: router eigrp <as-number>");
            }

            if (int.TryParse(context.CommandParts[1], out int asNumber))
            {
                            GetVendorContext(context)?.Capabilities.InitializeEigrp(asNumber);
            SetMode(context, "router");
            GetVendorContext(context)?.Capabilities.SetCurrentRouterProtocol("eigrp");
                return Success("");
            }
            else
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid AS number");
            }
        }
    }

    /// <summary>
    /// Cisco IP address configuration handler for interfaces
    /// </summary>
    public class IpAddressCommandHandler : VendorAgnosticCliHandler
    {
        public IpAddressCommandHandler() : base("address", "Set IP address")
        {
            AddAlias("addr");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: ip address <address> <mask>");
            }

            var ipAddress = context.CommandParts[1];
            var mask = context.CommandParts[2];

            // Validate IP address
            if (!IsValidIpAddress(ipAddress))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid IP address: {ipAddress}");
            }

            if (!IsValidMask(mask))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid mask: {mask}");
            }

            // Apply configuration using vendor-agnostic method
            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();

            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode,
                    "% Interface not selected");
            }

            var success = vendorContext?.Capabilities.ConfigureInterfaceIp(currentInterface, ipAddress, mask) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error applying configuration");
            }

            return Success("");
        }

        private bool IsValidIpAddress(string ip)
        {
            var parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }
            return true;
        }

        private bool IsValidMask(string mask)
        {
            return IsValidIpAddress(mask);
        }
    }

    /// <summary>
    /// Cisco access group configuration handler
    /// </summary>
    public class IpAccessGroupCommandHandler() : VendorAgnosticCliHandler("access-group", "Apply an access group to interface")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: ip access-group <number> [in|out]");
            }

            if (!int.TryParse(context.CommandParts[1], out int aclNumber))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid access list number: {context.CommandParts[1]}");
            }

            var direction = context.CommandParts[2].ToLower();
            if (direction != "in" && direction != "out")
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Direction must be 'in' or 'out'");
            }

            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();

            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode,
                    "% Interface not selected");
            }

            var success = vendorContext?.Capabilities.ApplyAccessGroup(currentInterface, aclNumber, direction) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error applying access group");
            }

            return Success("");
        }
    }

    /// <summary>
    /// Cisco shutdown command handler
    /// </summary>
    public class ShutdownCommandHandler() : VendorAgnosticCliHandler("shutdown", "Shutdown the interface")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();

            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode,
                    "% Interface not selected");
            }

            var success = vendorContext?.Capabilities.SetInterfaceShutdown(currentInterface, true) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error shutting down interface");
            }

            return Success("");
        }
    }

    /// <summary>
    /// Cisco no shutdown command handler
    /// </summary>
    public class NoShutdownHandler() : VendorAgnosticCliHandler("shutdown", "Enable an interface")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();

            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode,
                    "% Interface not selected");
            }

            var success = vendorContext?.Capabilities.SetInterfaceShutdown(currentInterface, false) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error enabling interface");
            }

            return Success("");
        }
    }

    /// <summary>
    /// Enhanced IP command handler with sub-commands
    /// </summary>
    public class EnhancedIpCommandHandler : VendorAgnosticCliHandler
    {
        public EnhancedIpCommandHandler() : base("ip", "Interface IP configuration")
        {
            AddSubHandler("address", new IpAddressCommandHandler());
            AddSubHandler("access-group", new IpAccessGroupCommandHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            return Error(CliErrorType.IncompleteCommand,
                "% Incomplete command. Available options: address, access-group");
        }
    }

    /// <summary>
    /// Enhanced no command handler with additional sub-commands
    /// </summary>
    public class EnhancedNoCommandHandler : VendorAgnosticCliHandler
    {
        public EnhancedNoCommandHandler() : base("no", "Negate a command or set its defaults")
        {
            AddSubHandler("shutdown", new NoShutdownHandler());
            AddSubHandler("ip", new NoIpHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand,
                "% Incomplete command");
        }
    }

    /// <summary>
    /// No IP command handler
    /// </summary>
    public class NoIpHandler : VendorAgnosticCliHandler
    {
        public NoIpHandler() : base("ip", "Remove IP configuration")
        {
            AddSubHandler("address", new NoIpAddressHandler());
            AddSubHandler("access-group", new NoIpAccessGroupHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand,
                "% Incomplete command");
        }
    }

    /// <summary>
    /// No IP address handler
    /// </summary>
    public class NoIpAddressHandler() : VendorAgnosticCliHandler("address", "Remove IP address")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();

            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode,
                    "% Interface not selected");
            }

            var success = vendorContext?.Capabilities.RemoveInterfaceIp(currentInterface) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error removing IP address");
            }

            return Success("");
        }
    }

    /// <summary>
    /// No IP access group handler
    /// </summary>
    public class NoIpAccessGroupHandler() : VendorAgnosticCliHandler("access-group", "Remove IP access group")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in interface mode");
            }

            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();

            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode,
                    "% Interface not selected");
            }

            var success = vendorContext?.Capabilities.RemoveAccessGroup(currentInterface) ?? false;
            if (!success)
            {
                return Error(CliErrorType.ExecutionError,
                    "% Error removing IP access group");
            }

            return Success("");
        }
    }

    /// <summary>
    /// Cisco VLAN name command handler
    /// </summary>
    public class CiscoVlanNameHandler() : VendorAgnosticCliHandler("name", "Set VLAN name")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "vlan"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in VLAN configuration mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command. Usage: name <vlan-name>");
            }

            var vlanName = context.CommandParts[1];

            // Get vendor capabilities to set VLAN name
            var vendorCaps = GetVendorContext(context)?.Capabilities;
            if (vendorCaps != null)
            {
                // We need to get the current VLAN ID - use device-specific method
                var deviceType = context.Device.GetType();
                var getCurrentVlanMethod = deviceType.GetMethod("GetCurrentVlanId");
                if (getCurrentVlanMethod != null)
                {
                    var currentVlanId = (int)(getCurrentVlanMethod.Invoke(context.Device, null) ?? 1);
                    if (vendorCaps.SetVlanName(currentVlanId, vlanName))
                    {
                        return Success("");
                    }
                }
            }

            return Error(CliErrorType.InvalidParameter,
                "% Failed to set VLAN name");
        }
    }

    /// <summary>
    /// Cisco router mode command handler - handles commands when in router configuration mode
    /// </summary>
    public class CiscoRouterModeCommandHandler() : VendorAgnosticCliHandler("", "Router mode commands")
    {
        public override bool CanHandle(ICliContext context)
        {
            return IsInMode(context, "router");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "router"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this mode");
            }

            var cmd = context.CommandParts[0].ToLower();
            var device = context.Device;

            return cmd switch
            {
                "router-id" => ProcessRouterIdCommand(context, device),
                "network" => ProcessNetworkCommand(context, device),
                "neighbor" => ProcessNeighborCommand(context, device),
                "version" => ProcessVersionCommand(context, device),
                "auto-summary" => ProcessAutoSummaryCommand(context, device),
                "no" => ProcessNoCommand(context, device),
                "exit" => ProcessExitCommand(context),
                _ => Error(CliErrorType.InvalidCommand, "% Invalid command")
            };
        }

        private CliResult ProcessRouterIdCommand(ICliContext context, INetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command. Usage: router-id <ip-address>");
            }

            var routerId = parts[1];
            if (IsValidIpAddress(routerId))
            {
                // Use device-specific methods directly
                var ciscoDevice = device as dynamic;
                if (ciscoDevice != null)
                {
                    try
                    {
                        // Get current protocol from device or mode context
                        var currentProtocol = GetCurrentProtocol(context);

                        if (currentProtocol == "ospf")
                        {
                            ciscoDevice.SetOspfRouterId(routerId);
                        }
                        else if (currentProtocol == "bgp")
                        {
                            ciscoDevice.SetBgpRouterId(routerId);
                        }

                        return Success("");
                    }
                    catch
                    {
                        // Fallback - just log the command
                        device?.AddLogEntry($"Router ID set to {routerId}");
                        return Success("");
                    }
                }
            }

            return Error(CliErrorType.InvalidParameter, "% Invalid router ID");
        }

        private CliResult ProcessNetworkCommand(ICliContext context, INetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            var currentProtocol = GetCurrentProtocol(context);
            var network = parts[1];

            if (currentProtocol == "ospf" && parts.Length >= 5 && parts[3] == "area")
            {
                // OSPF: network 10.0.0.0 0.0.0.3 area 0
                var wildcardMask = parts[2];
                var area = parts[4];
                device?.AddLogEntry($"OSPF network {network} wildcard {wildcardMask} area {area} configured");
                return Success("");
            }
            else if (currentProtocol == "bgp")
            {
                // BGP: network 192.168.1.0 mask 255.255.255.0
                device?.AddLogEntry($"BGP network {network} configured");
                return Success("");
            }
            else if (currentProtocol == "rip")
            {
                // RIP: network 10.0.0.0
                device?.AddLogEntry($"RIP network {network} configured");
                return Success("");
            }

            return Error(CliErrorType.InvalidParameter, "% Invalid network configuration");
        }

        private CliResult ProcessNeighborCommand(ICliContext context, INetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length < 4)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            var neighborIp = parts[1];
            if (parts[2] == "remote-as" && IsValidIpAddress(neighborIp))
            {
                var remoteAs = parts[3];
                device?.AddLogEntry($"BGP neighbor {neighborIp} remote-as {remoteAs} configured");
                return Success("");
            }

            return Error(CliErrorType.InvalidParameter, "% Invalid neighbor configuration");
        }

        private CliResult ProcessVersionCommand(ICliContext context, INetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command. Usage: version <version>");
            }

            var version = parts[1];
            var currentProtocol = GetCurrentProtocol(context);

            if (currentProtocol == "rip")
            {
                device?.AddLogEntry($"RIP version {version} configured");
                return Success("");
            }

            return Error(CliErrorType.InvalidParameter, "% Version command not supported for this protocol");
        }

        private CliResult ProcessAutoSummaryCommand(ICliContext context, INetworkDevice device)
        {
            var currentProtocol = GetCurrentProtocol(context);

            if (currentProtocol == "rip")
            {
                device?.AddLogEntry("RIP auto-summary enabled");
                return Success("");
            }

            return Error(CliErrorType.InvalidParameter, "% Auto-summary command not supported for this protocol");
        }

        private CliResult ProcessNoCommand(ICliContext context, INetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            var subCommand = parts[1].ToLower();
            var currentProtocol = GetCurrentProtocol(context);

            if (subCommand == "auto-summary" && currentProtocol == "rip")
            {
                device?.AddLogEntry("RIP auto-summary disabled");
                return Success("");
            }

            return Error(CliErrorType.InvalidParameter, "% Invalid no command");
        }

        private CliResult ProcessExitCommand(ICliContext context)
        {
            SetMode(context, "config");
            SetCurrentProtocol(context, "");
            return Success("");
        }

        private string GetCurrentProtocol(ICliContext context)
        {
            // Try to get current protocol from device context
            var deviceType = context.Device.GetType();
            var getCurrentProtocolMethod = deviceType.GetMethod("GetCurrentRouterProtocol");
            if (getCurrentProtocolMethod != null)
            {
                var protocol = getCurrentProtocolMethod.Invoke(context.Device, null) as string;
                return protocol ?? "unknown";
            }

            // Fallback - assume based on context or default to ospf
            return "ospf";
        }

        private void SetCurrentProtocol(ICliContext context, string protocol)
        {
            // Try to set current protocol on device context
            var deviceType = context.Device.GetType();
            var setCurrentProtocolMethod = deviceType.GetMethod("SetCurrentRouterProtocol");
            if (setCurrentProtocolMethod != null)
            {
                setCurrentProtocolMethod.Invoke(context.Device, new object[] { protocol });
            }
        }

        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }
}
