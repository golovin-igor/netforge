using System.Text;
using System.Threading.Tasks;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Anira.Basic
{
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