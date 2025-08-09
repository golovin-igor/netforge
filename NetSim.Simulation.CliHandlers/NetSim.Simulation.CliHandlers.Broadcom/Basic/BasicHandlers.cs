using System.Text;
using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.Broadcom.Basic
{
    /// <summary>
    /// Broadcom enable command handler
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
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
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
    /// Broadcom ping command handler
    /// </summary>
    public class PingCommandHandler : VendorAgnosticCliHandler
    {
        public PingCommandHandler() : base("ping", "Send ping packets")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
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
    /// Broadcom write command handler
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
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
            }
            
            return Success("Configuration saved successfully.");
        }
    }

    /// <summary>
    /// Broadcom reload command handler
    /// </summary>
    public class ReloadCommandHandler : VendorAgnosticCliHandler
    {
        public ReloadCommandHandler() : base("reload", "Restart the system")
        {
            AddAlias("restart");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
            }
            
            return Success("System will restart in 5 seconds... [Press Ctrl-C to cancel]");
        }
    }

    /// <summary>
    /// Broadcom history command handler
    /// </summary>
    public class HistoryCommandHandler : VendorAgnosticCliHandler
    {
        public HistoryCommandHandler() : base("history", "Show command history")
        {
            AddAlias("hist");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
            }
            
            var output = new StringBuilder();
            output.AppendLine("Command History:");
            output.AppendLine("  1  show version");
            output.AppendLine("  2  show interfaces");
            output.AppendLine("  3  show running-config");
            output.AppendLine("  4  history");
            
            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Broadcom copy command handler
    /// </summary>
    public class CopyCommandHandler : VendorAgnosticCliHandler
    {
        public CopyCommandHandler() : base("copy", "Copy files")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
            }
            
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need source and destination");
            }
            
            var source = context.CommandParts[1];
            var destination = context.CommandParts[2];
            
            return Success($"Copying {source} to {destination}... [OK]");
        }
    }

    /// <summary>
    /// Broadcom traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Broadcom"))
            {
                return RequireVendor(context, "Broadcom");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need destination address");
            }
            
            var targetIp = context.CommandParts[1];
            
            // Validate IP address format
            if (!IsValidIpAddress(targetIp))
            {
                return Error(CliErrorType.InvalidParameter, 
                    $"% Invalid IP address: {targetIp}");
            }
            
            // Simulate traceroute result (Broadcom style)
            var output = new StringBuilder();
            output.AppendLine($"traceroute to {targetIp} (64 byte packets, 30 hops max)");
            output.AppendLine($" 1  192.168.1.1  1.234 ms  1.123 ms  1.045 ms");
            output.AppendLine($" 2  10.0.0.1  2.456 ms  2.234 ms  2.123 ms");
            output.AppendLine($" 3  {targetIp}  3.678 ms  3.456 ms  3.234 ms");
            
            return Success(output.ToString());
        }
        
        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }
}
