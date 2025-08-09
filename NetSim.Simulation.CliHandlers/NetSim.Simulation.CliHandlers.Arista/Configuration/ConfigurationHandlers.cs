using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.CliHandlers;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Arista.Configuration
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
        
        protected override CliResult ExecuteCommand(CliContext context)
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
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit current mode")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
        
        protected override CliResult ExecuteCommand(CliContext context)
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
    public class HostnameCommandHandler : VendorAgnosticCliHandler
    {
        public HostnameCommandHandler() : base("hostname", "Set device hostname")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
            var device = context.Device as NetworkDevice;
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
    public class VlanCommandHandler : VendorAgnosticCliHandler
    {
        public VlanCommandHandler() : base("vlan", "Configure VLAN")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
        
        private void SetCurrentVlan(CliContext context, int vlanId)
        {
            var device = context.Device as NetworkDevice;
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
    public class NoCommandHandler : VendorAgnosticCliHandler
    {
        public NoCommandHandler() : base("no", "Negate a command")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
        
        private CliResult HandleNoShutdown(CliContext context)
        {
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% This command requires interface mode");
            }
            
            var device = context.Device as NetworkDevice;
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
        
        private CliResult HandleNoIp(CliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Success(""); // Generic no ip command
            }
            
            var ipCommand = context.CommandParts[2];
            
            if (ipCommand == "address" && IsInMode(context, "interface"))
            {
                var device = context.Device as NetworkDevice;
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
        
        private CliResult HandleNoHostname(CliContext context)
        {
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% This command requires configuration mode");
            }
            
            var device = context.Device as NetworkDevice;
            if (device != null)
            {
                device.SetHostname("Switch"); // Default name
                device.AddLogEntry("Hostname reset to default");
            }
            
            return Success("");
        }
    }
}
