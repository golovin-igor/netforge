using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP ping command handler
    /// </summary>
    public class PingCommandHandler() : VendorAgnosticCliHandler("ping", "Ping a host")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "Usage: ping <host>");
            }
            
            var target = context.CommandParts[1];
            
            var output = $@"PING {target}: 56 data bytes
64 bytes from 8.8.8.8: icmp_seq=0. time=1. ms
64 bytes from 8.8.8.8: icmp_seq=1. time=1. ms
64 bytes from 8.8.8.8: icmp_seq=2. time=1. ms
64 bytes from 8.8.8.8: icmp_seq=3. time=1. ms
64 bytes from 8.8.8.8: icmp_seq=4. time=1. ms

----8.8.8.8 PING Statistics----
5 packets transmitted, 5 packets received, 0% packet loss
round-trip (ms)  min/avg/max/stddev = 1.000/1.000/1.000/0.000";
            
            return Success(output);
        }
    }
} 
