using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP exit command handler
    /// </summary>
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit current mode or session")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
