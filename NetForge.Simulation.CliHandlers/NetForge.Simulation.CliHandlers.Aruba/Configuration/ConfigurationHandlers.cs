using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Aruba.Configuration
{
    public static class ConfigurationHandlers
    {
        /// <summary>
        /// Aruba configure command handler
        /// </summary>
        public class ArubaConfigureHandler() : VendorAgnosticCliHandler("configure", "Enter configuration mode")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "configure", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                try
                {
                    SetMode(context, "config");
                    return Success("");
                }
                catch
                {
                    return Error(CliErrorType.InvalidMode, "Failed to enter configuration mode");
                }
            }
        }

        /// <summary>
        /// Aruba exit command handler
        /// </summary>
        public class ArubaExitHandler() : VendorAgnosticCliHandler("exit", "Exit current mode")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "exit", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var vendorContext = GetVendorContext(context);
                var currentMode = context.Device.GetCurrentMode();

                var newMode = currentMode switch
                {
                    "config" => "privileged",
                    "interface" => "config",
                    "vlan" => "config",
                    "privileged" => "user",
                    _ => "user"
                };

                try
                {
                    SetMode(context, newMode);
                    return Success("");
                }
                catch
                {
                    return Error(CliErrorType.InvalidMode, "Failed to exit mode");
                }
            }
        }

        /// <summary>
        /// Aruba interface command handler
        /// </summary>
        public class ArubaInterfaceHandler() : VendorAgnosticCliHandler("interface", "Configure interface")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "interface", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "config"))
                    return Error(CliErrorType.InvalidMode, "Command only available in configuration mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: interface <interface-name>");

                var interfaceName = args[1];
                var device = context.Device as NetworkDevice;
                
                // Support Aruba interface aliasing: "1" -> "1/1/1", "2" -> "1/1/2", etc.
                var actualInterfaceName = interfaceName;
                if (int.TryParse(interfaceName, out int portNum) && portNum >= 1 && portNum <= 28)
                {
                    actualInterfaceName = $"1/1/{portNum}";
                }
                
                var iface = device?.GetInterface(actualInterfaceName);
                if (iface == null)
                    return Error(CliErrorType.InvalidParameter, $"Interface {interfaceName} not found");

                // Set current interface and enter interface mode
                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                vendorCaps?.SetCurrentInterface(actualInterfaceName);
                
                // Also set the current interface on the device itself for prompt display
                device?.SetCurrentInterface(actualInterfaceName);
                
                try
                {
                    SetMode(context, "interface");
                    return Success("");
                }
                catch
                {
                    return Error(CliErrorType.InvalidMode, "Failed to enter interface mode");
                }
            }
        }

        /// <summary>
        /// Aruba VLAN command handler
        /// </summary>
        public class ArubaVlanHandler() : VendorAgnosticCliHandler("vlan", "Configure VLAN")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "vlan", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "config"))
                    return Error(CliErrorType.InvalidMode, "Command only available in configuration mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: vlan <vlan-id>");

                if (!int.TryParse(args[1], out int vlanId) || vlanId < 1 || vlanId > 4094)
                    return Error(CliErrorType.InvalidParameter, "Invalid VLAN ID (1-4094)");

                // Set current VLAN context for subsequent commands
                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                vendorCaps?.SetCurrentVlan(vlanId);
                
                // Create VLAN if it doesn't exist
                vendorCaps?.CreateOrSelectVlan(vlanId);

                // Enter VLAN configuration mode
                try
                {
                    SetMode(context, "vlan");
                    return Success("");
                }
                catch
                {
                    return Error(CliErrorType.InvalidMode, "Failed to enter VLAN mode");
                }
            }
        }

        /// <summary>
        /// Aruba IP command handler (for interface mode)
        /// </summary>
        public class ArubaIpHandler() : VendorAgnosticCliHandler("ip", "Configure IP settings")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "ip", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: ip <subcommand>");

                var subcommand = args[1].ToLower();
                
                return subcommand switch
                {
                    "address" => HandleIpAddress(context, args),
                    _ => Error(CliErrorType.InvalidParameter, $"Unknown IP subcommand: {subcommand}")
                };
            }

            private CliResult HandleIpAddress(CliContext context, string[] args)
            {
                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "IP address can only be configured in interface mode");

                if (args.Length < 4)
                    return Error(CliErrorType.InvalidParameter, "Usage: ip address <ip-address> <subnet-mask>");

                var ipAddress = args[2];
                var subnetMask = args[3];

                // Get current interface from context
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces().Values;
                var currentInterface = interfaces?.FirstOrDefault(i => i.IsUp); // Simplified interface detection
                
                if (currentInterface == null)
                    return Error(CliErrorType.InvalidParameter, "No current interface found");

                var vendorCaps = GetVendorCapabilities(context);
                if (vendorCaps?.ConfigureInterfaceIp(currentInterface.Name, ipAddress, subnetMask) == true)
                {
                    return Success("");
                }

                return Error(CliErrorType.InvalidParameter, "Failed to set IP address");
            }
        }

        /// <summary>
        /// Aruba switchport command handler
        /// </summary>
        public class ArubaSwitchportHandler() : VendorAgnosticCliHandler("switchport", "Configure switchport settings")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "switchport", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: switchport <subcommand>");

                var subcommand = args[1].ToLower();
                
                return subcommand switch
                {
                    "mode" => HandleSwitchportMode(context, args),
                    "access" => HandleSwitchportAccess(context, args),
                    _ => Error(CliErrorType.InvalidParameter, $"Unknown switchport subcommand: {subcommand}")
                };
            }

            private CliResult HandleSwitchportMode(CliContext context, string[] args)
            {
                if (args.Length < 3)
                    return Error(CliErrorType.InvalidParameter, "Usage: switchport mode <access|trunk>");

                var mode = args[2].ToLower();
                if (mode != "access" && mode != "trunk")
                    return Error(CliErrorType.InvalidParameter, "Mode must be 'access' or 'trunk'");

                // Simulate switchport mode configuration
                return Success("");
            }

            private CliResult HandleSwitchportAccess(CliContext context, string[] args)
            {
                if (args.Length < 4 || args[2].ToLower() != "vlan")
                    return Error(CliErrorType.InvalidParameter, "Usage: switchport access vlan <vlan-id>");

                if (!int.TryParse(args[3], out int vlanId) || vlanId < 1 || vlanId > 4094)
                    return Error(CliErrorType.InvalidParameter, "Invalid VLAN ID (1-4094)");

                // Get current interface and set VLAN
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces().Values;
                var currentInterface = interfaces?.FirstOrDefault(i => i.IsUp);
                
                if (currentInterface != null)
                {
                    var vendorCaps = GetVendorCapabilities(context);
                    vendorCaps?.SetInterfaceVlan(currentInterface.Name, vlanId);
                }

                return Success("");
            }
        }

        /// <summary>
        /// Aruba shutdown command handler
        /// </summary>
        public class ArubaShutdownHandler() : VendorAgnosticCliHandler("shutdown", "Shutdown interface")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "shutdown", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces().Values;
                var currentInterface = interfaces?.FirstOrDefault(i => i.IsUp);
                
                if (currentInterface != null)
                {
                    var vendorCaps = GetVendorCapabilities(context);
                    vendorCaps?.SetInterfaceShutdown(currentInterface.Name, true);
                }

                return Success("");
            }
        }

        /// <summary>
        /// Aruba no command handler
        /// </summary>
        public class ArubaNoHandler() : VendorAgnosticCliHandler("no", "Negate a command")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "no", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: no <command>");

                var command = args[1].ToLower();
                
                return command switch
                {
                    "shutdown" => HandleNoShutdown(context),
                    _ => Error(CliErrorType.InvalidParameter, $"Unknown no command: {command}")
                };
            }

            private CliResult HandleNoShutdown(CliContext context)
            {
                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var device = context.Device as NetworkDevice;
                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                var currentInterface = vendorCaps?.GetCurrentInterface();
                
                if (!string.IsNullOrEmpty(currentInterface))
                {
                    vendorCaps?.SetInterfaceShutdown(currentInterface, false);
                }

                return Success("");
            }
        }

        /// <summary>
        /// Aruba VLAN name command handler
        /// </summary>
        public class ArubaVlanNameHandler() : VendorAgnosticCliHandler("name", "Set VLAN name")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "name", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "vlan"))
                    return Error(CliErrorType.InvalidMode, "Command only available in VLAN mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: name <vlan-name>");

                var vlanName = args[1];
                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                var currentVlanId = vendorCaps?.GetCurrentVlan() ?? 1;

                if (vendorCaps?.SetVlanName(currentVlanId, vlanName) == true)
                {
                    return Success("");
                }

                return Error(CliErrorType.InvalidParameter, "Failed to set VLAN name");
            }
        }

        /// <summary>
        /// Aruba VLAN tagged command handler
        /// </summary>
        public class ArubaVlanTaggedHandler() : VendorAgnosticCliHandler("tagged", "Add tagged ports to VLAN")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "tagged", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "vlan"))
                    return Error(CliErrorType.InvalidMode, "Command only available in VLAN mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: tagged <interface-name>");

                var interfaceName = args[1];
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                
                if (interfaces == null || !interfaces.ContainsKey(interfaceName))
                    return Error(CliErrorType.InvalidParameter, "Interface not found");

                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                var currentVlanId = vendorCaps?.GetCurrentVlan() ?? 1;

                if (vendorCaps?.AddTaggedPort(currentVlanId, interfaceName) == true)
                {
                    return Success("");
                }

                return Error(CliErrorType.InvalidParameter, "Failed to add tagged port");
            }
        }

        /// <summary>
        /// Aruba VLAN untagged command handler
        /// </summary>
        public class ArubaVlanUntaggedHandler() : VendorAgnosticCliHandler("untagged", "Add untagged ports to VLAN")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "untagged", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "vlan"))
                    return Error(CliErrorType.InvalidMode, "Command only available in VLAN mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: untagged <interface-name>");

                var interfaceName = args[1];
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                
                if (interfaces == null || !interfaces.ContainsKey(interfaceName))
                    return Error(CliErrorType.InvalidParameter, "Interface not found");

                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                var currentVlanId = vendorCaps?.GetCurrentVlan() ?? 1;

                if (vendorCaps?.AddUntaggedPort(currentVlanId, interfaceName) == true)
                {
                    return Success("");
                }

                return Error(CliErrorType.InvalidParameter, "Failed to add untagged port");
            }
        }

        /// <summary>
        /// Aruba IP route command handler (for static routes)
        /// </summary>
        public class ArubaIpRouteHandler() : VendorAgnosticCliHandler("route", "Configure static routes")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 2 &&
                       string.Equals(context.CommandParts[0], "ip", StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(context.CommandParts[1], "route", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "config"))
                    return Error(CliErrorType.InvalidMode, "Command only available in configuration mode");

                var args = context.CommandParts;
                if (args.Length < 5)
                    return Error(CliErrorType.InvalidParameter, "Usage: ip route <network> <mask> <next-hop>");

                var network = args[2];
                var mask = args[3]; 
                var nextHop = args[4];
                var metric = 1;

                // Check for optional metric
                if (args.Length > 5 && int.TryParse(args[5], out int parsedMetric))
                {
                    metric = parsedMetric;
                }

                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                if (vendorCaps?.AddStaticRoute(network, mask, nextHop, metric) == true)
                {
                    return Success("");
                }

                return Error(CliErrorType.InvalidParameter, "Failed to add static route");
            }
        }

        /// <summary>
        /// Aruba clear command handler 
        /// </summary>
        public class ArubaClearHandler() : VendorAgnosticCliHandler("clear", "Clear counters and statistics")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "clear", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: clear <counters>");

                var subCommand = args[1].ToLower();
                
                return subCommand switch
                {
                    "counters" => HandleClearCounters(context, args),
                    _ => Error(CliErrorType.InvalidParameter, $"Unknown clear command: {subCommand}")
                };
            }

            private CliResult HandleClearCounters(CliContext context, string[] args)
            {
                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                
                if (args.Length > 2)
                {
                    // Clear specific interface counters
                    var interfaceName = args[2];
                    if (vendorCaps?.ClearInterfaceCounters(interfaceName) == true)
                    {
                        return Success($"Cleared counters for interface {interfaceName}");
                    }
                    return Error(CliErrorType.InvalidParameter, "Interface not found");
                }
                else
                {
                    // Clear all interface counters
                    if (vendorCaps?.ClearInterfaceCounters() == true)
                    {
                        return Success("Cleared all interface counters");
                    }
                    return Error(CliErrorType.InvalidCommand, "Failed to clear counters");
                }
            }
        }

        /// <summary>
        /// Aruba disable command handler (for interfaces)
        /// </summary>
        public class ArubaDisableHandler() : VendorAgnosticCliHandler("disable", "Disable interface")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "disable", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                var currentInterface = vendorCaps?.GetCurrentInterface();
                
                if (!string.IsNullOrEmpty(currentInterface))
                {
                    vendorCaps?.SetInterfaceShutdown(currentInterface, true);
                }

                return Success("");
            }
        }

        /// <summary>
        /// Aruba name command handler (for interfaces)
        /// </summary>
        public class ArubaInterfaceNameHandler() : VendorAgnosticCliHandler("name", "Set interface description")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "name", StringComparison.OrdinalIgnoreCase) &&
                       IsInMode(context, "interface");
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: name <description>");

                var description = string.Join(" ", args.Skip(1)).Trim('"');
                var vendorCaps = GetVendorCapabilities(context) as ArubaVendorCapabilities;
                var currentInterface = vendorCaps?.GetCurrentInterface();
                
                if (!string.IsNullOrEmpty(currentInterface))
                {
                    vendorCaps?.SetInterfaceDescription(currentInterface, description);
                }

                return Success("");
            }
        }

        /// <summary>
        /// Aruba speed command handler (for interfaces)
        /// </summary>
        public class ArubaSpeedHandler() : VendorAgnosticCliHandler("speed", "Set interface speed")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "speed", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: speed <speed>");

                var speed = args[1];
                // Just acknowledge the command - interface speed configuration
                return Success("");
            }
        }

        /// <summary>
        /// Aruba duplex command handler (for interfaces)
        /// </summary>
        public class ArubaDuplexHandler() : VendorAgnosticCliHandler("duplex", "Set interface duplex")
        {
            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "duplex", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "interface"))
                    return Error(CliErrorType.InvalidMode, "Command only available in interface mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: duplex <full|half|auto>");

                var duplex = args[1];
                // Just acknowledge the command - interface duplex configuration
                return Success("");
            }
        }
    }
} 
