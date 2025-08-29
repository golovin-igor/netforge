using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP exit command handler
    /// </summary>
    public class ExitCommandHandler() : VendorAgnosticCliHandler("exit", "Exit current mode or session")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            // Exit current mode
            SetMode(context, "user");
            
            return Success("Exiting current mode...");
        }
    }
} 
