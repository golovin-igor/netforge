using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP exit command handler
    /// </summary>
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit current mode or session")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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
