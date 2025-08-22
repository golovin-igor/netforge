using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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
            
            if (!IPAddress.TryParse(targetIp, out _))
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
}