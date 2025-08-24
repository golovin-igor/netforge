using System.Threading.Tasks;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Basic
{
    /// <summary>
    /// Cisco enable command handler
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
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }
            
            // Check if already in privileged mode
            if (IsInMode(context, "privileged"))
            {
                return Success(""); // Already in privileged mode
            }
            
            // Move to privileged mode
            SetMode(context, "privileged");
            
            return Success("");
        }
    }
}