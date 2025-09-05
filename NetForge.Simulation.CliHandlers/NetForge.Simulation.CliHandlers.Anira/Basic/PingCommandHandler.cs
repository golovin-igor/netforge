using System.Text;
using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Anira.Basic
{
    /// <summary>
    /// Anira ping command handler
    /// </summary>
    public class PingCommandHandler() : VendorAgnosticCliHandler("ping", "Send ping packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
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
            var device = context.Device;
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
