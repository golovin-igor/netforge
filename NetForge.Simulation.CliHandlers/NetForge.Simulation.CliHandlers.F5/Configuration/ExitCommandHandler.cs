using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.F5.Configuration
{
    /// <summary>
    /// F5 BIG-IP configuration exit command handler
    /// </summary>
    public class ExitCommandHandler() : VendorAgnosticCliHandler("exit", "Exit configuration mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            // Exit configuration mode
            SetMode(context, "privileged");
            
            return Success("Exiting configuration mode...");
        }
    }
} 
