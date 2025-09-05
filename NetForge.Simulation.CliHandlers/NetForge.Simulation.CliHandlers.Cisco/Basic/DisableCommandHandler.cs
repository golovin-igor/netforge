using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Basic
{
    /// <summary>
    /// Cisco disable command handler
    /// </summary>
    public class DisableCommandHandler() : VendorAgnosticCliHandler("disable", "Exit privileged mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }
            
            // Move to user mode
            SetMode(context, "user");
            
            return Success("");
        }
    }
}