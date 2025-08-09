using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP disable command handler
    /// </summary>
    public class DisableCommandHandler : VendorAgnosticCliHandler
    {
        public DisableCommandHandler() : base("disable", "Exit privileged mode")
        {
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
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
