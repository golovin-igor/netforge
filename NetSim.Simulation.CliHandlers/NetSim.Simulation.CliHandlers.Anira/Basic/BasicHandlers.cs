using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.CliHandlers;

namespace NetSim.Simulation.CliHandlers.Anira.Basic
{
    /// <summary>
    /// Anira enable command handler
    /// </summary>
    public class EnableCommandHandler : VendorAgnosticCliHandler
    {
        public EnableCommandHandler() : base("enable", "Enter privileged mode")
        {
            AddAlias("en");
            AddAlias("ena");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
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
    /// Anira disable command handler
    /// </summary>
    public class DisableCommandHandler : VendorAgnosticCliHandler
    {
        public DisableCommandHandler() : base("disable", "Exit privileged mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode, "Not in privileged mode");
            }
            
            SetMode(context, "user");
            return Success("");
        }
    }

    /// <summary>
    /// Anira exit command handler
    /// </summary>
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit current mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            var currentMode = context.Device.GetCurrentMode();
            
            switch (currentMode)
            {
                case "interface":
                    SetMode(context, "config");
                    break;
                case "config":
                    SetMode(context, "privileged");
                    break;
                case "privileged":
                    SetMode(context, "user");
                    break;
                case "user":
                    // Stay in user mode
                    break;
                default:
                    SetMode(context, "user");
                    break;
            }
            
            return Success("");
        }
    }

    /// <summary>
    /// Anira ping command handler
    /// </summary>
    public class PingCommandHandler : VendorAgnosticCliHandler
    {
        public PingCommandHandler() : base("ping", "Send ping packets")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "Incomplete command\nUsage: ping <destination>");
            }
            
            var targetIp = context.CommandParts[1];
            
            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter, "Invalid IP address");
            }
            
            // Use the device's built-in ping simulation if available
            var device = context.Device as NetworkDevice;
            if (device != null)
            {
                try
                {
                    var pingResult = device.ExecutePing(targetIp);
                    return Success(pingResult);
                }
                catch
                {
                    // Fall back to simple ping simulation
                }
            }
            
            var output = new StringBuilder();
            output.AppendLine($"PING {targetIp}");
            output.AppendLine("--- ping statistics ---");
            output.AppendLine("5 packets transmitted, 5 received, 0% packet loss");
            
            return Success(output.ToString());
        }
    }
}
