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
            
            // Handle known config types
            if (configType.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                if (context.CommandParts.Length < 3)
                {
                    return Error(CliErrorType.IncompleteCommand, "Command parse error");
                }
                
                var systemType = context.CommandParts[2];
                
                // Handle system interface, etc.
                if (systemType.Equals("interface", StringComparison.OrdinalIgnoreCase) ||
                    systemType.Equals("hostname", StringComparison.OrdinalIgnoreCase) ||
                    systemType.Equals("global", StringComparison.OrdinalIgnoreCase))
                {
                    SetMode(context, "config-system");
                    return Success("");
                }
                
                return Error(CliErrorType.InvalidCommand, "Command parse error");
            }
            
            if (configType.Equals("router", StringComparison.OrdinalIgnoreCase))
            {
                SetMode(context, "config-router");
                return Success("");
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
}
