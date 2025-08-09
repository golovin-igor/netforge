using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Fortinet.Basic
{
    /// <summary>
    /// Fortinet enable command handler
    /// </summary>
    public class EnableCommandHandler : VendorAgnosticCliHandler
    {
        public EnableCommandHandler() : base("enable", "Enter privileged mode")
        {
            AddAlias("en");
            AddAlias("ena");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (IsInMode(context, "privileged"))
            {
                return Success("");
            }
            
            SetMode(context, "privileged");
            return Success("");
        }
    }

    /// <summary>
    /// Fortinet ping command handler
    /// </summary>
    public class PingCommandHandler : VendorAgnosticCliHandler
    {
        public PingCommandHandler() : base("ping", "Send ping packets")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need IP address");
            }
            
            var targetIp = context.CommandParts[1];
            
            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter, $"% Invalid IP address: {targetIp}");
            }
            
            var output = new StringBuilder();
            output.AppendLine($"PING {targetIp}");
            output.AppendLine($"64 bytes from {targetIp}: time=1.2 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.1 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.0 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=0.9 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.3 ms");
            output.AppendLine($"--- {targetIp} ping statistics ---");
            output.AppendLine("5 packets transmitted, 5 packets received, 0% packet loss");
            
            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Fortinet execute command handler
    /// </summary>
    public class ExecuteCommandHandler : VendorAgnosticCliHandler
    {
        public ExecuteCommandHandler() : base("execute", "Execute operational commands")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }
            
            var subCommand = context.CommandParts[1];
            
            if (subCommand.Equals("ping", StringComparison.OrdinalIgnoreCase))
            {
                return ExecutePing(context);
            }
            
            return Error(CliErrorType.InvalidCommand, $"% Unknown command: execute {subCommand}");
        }
        
        private CliResult ExecutePing(CliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need IP address");
            }
            
            var targetIp = context.CommandParts[2];
            
            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter, $"% Invalid IP address: {targetIp}");
            }
            
            var output = new StringBuilder();
            output.AppendLine($"PING {targetIp}");
            output.AppendLine($"64 bytes from {targetIp}: time=1.2 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.1 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.0 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=0.9 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.3 ms");
            output.AppendLine($"--- {targetIp} ping statistics ---");
            output.AppendLine("5 packets transmitted, 5 packets received, 0% packet loss");
            
            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Fortinet config command handler
    /// </summary>
    public class ConfigCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigCommandHandler() : base("config", "Enter configuration mode")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "Command parse error");
            }
            
            var configType = context.CommandParts[1];
            
            // Handle global configuration
            if (configType.Equals("global", StringComparison.OrdinalIgnoreCase))
            {
                SetMode(context, "global_config");
                return Success("");
            }
            
            // Handle known config types
            if (configType.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                if (context.CommandParts.Length < 3)
                {
                    return Error(CliErrorType.IncompleteCommand, "Command parse error");
                }
                
                var systemType = context.CommandParts[2];
                
                // Handle system interface, etc.
                if (systemType.Equals("interface", StringComparison.OrdinalIgnoreCase))
                {
                    SetMode(context, "system_if");
                    return Success("");
                }
                
                if (systemType.Equals("hostname", StringComparison.OrdinalIgnoreCase) ||
                    systemType.Equals("global", StringComparison.OrdinalIgnoreCase))
                {
                    SetMode(context, "config-system");
                    return Success("");
                }
                
                return Error(CliErrorType.InvalidCommand, "Command parse error");
            }
            
            if (configType.Equals("router", StringComparison.OrdinalIgnoreCase))
            {
                if (context.CommandParts.Length < 3)
                {
                    return Error(CliErrorType.IncompleteCommand, "Command parse error");
                }
                
                var routerType = context.CommandParts[2];
                
                if (routerType.Equals("ospf", StringComparison.OrdinalIgnoreCase))
                {
                    SetMode(context, "router_ospf");
                    return Success("");
                }
                
                if (routerType.Equals("bgp", StringComparison.OrdinalIgnoreCase))
                {
                    SetMode(context, "router_bgp");
                    return Success("");
                }
                
                return Error(CliErrorType.InvalidCommand, "Command parse error");
            }
            
            if (configType.Equals("firewall", StringComparison.OrdinalIgnoreCase))
            {
                SetMode(context, "config-firewall");
                return Success("");
            }
            
            // Invalid config type
            return Error(CliErrorType.InvalidCommand, "Command parse error");
        }
    }

    /// <summary>
    /// Fortinet edit command handler
    /// </summary>
    public class EditCommandHandler : VendorAgnosticCliHandler
    {
        public EditCommandHandler() : base("edit", "Edit configuration object")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "Command parse error");
            }
            
            var currentMode = GetCurrentMode(context);
            var objectName = context.CommandParts[1];
            
            // Handle interface editing
            if (currentMode == "system_if")
            {
                SetMode(context, "interface");
                // Store current interface for later reference
                if (context.Device is NetworkDevice device)
                {
                    device.AddLogEntry($"Editing interface: {objectName}");
                }
                return Success("");
            }
            
            return Success("");
        }
    }

    /// <summary>
    /// Fortinet set command handler
    /// </summary>
    public class SetCommandHandler : VendorAgnosticCliHandler
    {
        public SetCommandHandler() : base("set", "Set configuration parameter")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "Command parse error");
            }
            
            var parameter = context.CommandParts[1];
            var currentMode = GetCurrentMode(context);
            
            // Handle IP address setting in interface mode
            if (currentMode == "interface" && parameter.Equals("ip", StringComparison.OrdinalIgnoreCase))
            {
                if (context.CommandParts.Length < 4)
                {
                    return Error(CliErrorType.IncompleteCommand, "Command parse error");
                }
                
                var ipAddress = context.CommandParts[2];
                var subnetMask = context.CommandParts[3];
                
                // Set IP on current interface (simplified)
                if (context.Device is NetworkDevice device)
                {
                    device.AddLogEntry($"Set IP: {ipAddress}/{subnetMask}");
                }
                
                return Success("");
            }
            
            // Handle description setting
            if (currentMode == "interface" && parameter.Equals("description", StringComparison.OrdinalIgnoreCase))
            {
                if (context.CommandParts.Length < 3)
                {
                    return Error(CliErrorType.IncompleteCommand, "Command parse error");
                }
                
                var description = string.Join(" ", context.CommandParts.Skip(2)).Trim('"');
                
                if (context.Device is NetworkDevice device)
                {
                    device.AddLogEntry($"Set description: {description}");
                }
                
                return Success("");
            }
            
            // Handle hostname setting in global config mode
            if (currentMode == "global_config" && parameter.Equals("hostname", StringComparison.OrdinalIgnoreCase))
            {
                if (context.CommandParts.Length < 3)
                {
                    return Error(CliErrorType.IncompleteCommand, "Command parse error");
                }
                
                var hostname = context.CommandParts[2].Trim('"');
                
                if (context.Device is NetworkDevice device)
                {
                    device.AddLogEntry($"Set hostname: {hostname}");
                }
                
                return Success("");
            }
            
            return Success("");
        }
    }
}
