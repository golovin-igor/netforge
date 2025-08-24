using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP disable command handler
    /// </summary>
    public class DisableCommandHandler : VendorAgnosticCliHandler
    {
        public DisableCommandHandler() : base("disable", "Exit privileged mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            // Move to user mode
            SetMode(context, "user");
            
            return Success("Exiting privileged mode...");
        }
    }
} 
