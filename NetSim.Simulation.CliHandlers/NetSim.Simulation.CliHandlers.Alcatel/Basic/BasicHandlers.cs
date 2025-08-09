using System.Text;
using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel enable command handler
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
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
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
    /// Alcatel ping command handler
    /// </summary>
    public class PingCommandHandler : VendorAgnosticCliHandler
    {
        public PingCommandHandler() : base("ping", "Send ping packets")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
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
            output.AppendLine($"--- {targetIp} ping statistics ---");
            output.AppendLine("3 packets transmitted, 3 received, 0% packet loss");
            
            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Alcatel traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need destination IP");
            }
            
            var targetIp = context.CommandParts[1];
            
            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter, $"% Invalid IP address: {targetIp}");
            }
            
            var output = new StringBuilder();
            output.AppendLine($"traceroute to {targetIp}, 30 hops max, 40 byte packets");
            output.AppendLine($" 1  gateway ({targetIp})  1.2 ms  1.1 ms  1.0 ms");
            output.AppendLine($" 2  {targetIp}  1.3 ms  1.2 ms  1.1 ms");
            
            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Alcatel write command handler
    /// </summary>
    public class WriteCommandHandler : VendorAgnosticCliHandler
    {
        public WriteCommandHandler() : base("write", "Save configuration")
        {
            AddAlias("wr");
            AddAlias("copy running-config startup-config");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            return Success("Configuration saved successfully");
        }
    }

    /// <summary>
    /// Alcatel reload command handler
    /// </summary>
    public class ReloadCommandHandler : VendorAgnosticCliHandler
    {
        public ReloadCommandHandler() : base("reload", "Restart the system")
        {
            AddAlias("restart");
            AddAlias("reboot");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            return Success("System restart initiated");
        }
    }

    /// <summary>
    /// Alcatel history command handler
    /// </summary>
    public class HistoryCommandHandler : VendorAgnosticCliHandler
    {
        public HistoryCommandHandler() : base("history", "Display command history")
        {
            AddAlias("hist");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            var output = new StringBuilder();
            output.AppendLine("Command History:");
            output.AppendLine("  1  enable");
            output.AppendLine("  2  show version");
            output.AppendLine("  3  show system");
            output.AppendLine("  4  history");
            
            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Alcatel copy command handler
    /// </summary>
    public class CopyCommandHandler : VendorAgnosticCliHandler
    {
        public CopyCommandHandler() : base("copy", "Copy files")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            return Success("Copy operation completed");
        }
    }

    /// <summary>
    /// Alcatel admin command handler
    /// </summary>
    public class AdminCommandHandler : VendorAgnosticCliHandler
    {
        public AdminCommandHandler() : base("admin", "Administrative commands")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need admin option");
            }
            
            var adminOption = context.CommandParts[1];
            
            return adminOption switch
            {
                "display-config" => HandleAdminDisplayConfig(context),
                _ => Error(CliErrorType.InvalidCommand, 
                    $"% Invalid admin option: {adminOption}")
            };
        }
        
        private CliResult HandleAdminDisplayConfig(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("! Configuration file generated by Alcatel-Lucent");
            output.AppendLine("! Current configuration:");
            output.AppendLine("!");
            output.AppendLine($"system name {device?.Name}");
            output.AppendLine("!");
            output.AppendLine("interface vlan 1");
            output.AppendLine("  no shutdown");
            output.AppendLine("!");
            output.AppendLine("end");
            
            return Success(output.ToString());
        }
    }
}
