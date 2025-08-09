using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Configuration
{
    /// <summary>
    /// F5 BIG-IP configuration exit command handler
    /// </summary>
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit configuration mode")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
