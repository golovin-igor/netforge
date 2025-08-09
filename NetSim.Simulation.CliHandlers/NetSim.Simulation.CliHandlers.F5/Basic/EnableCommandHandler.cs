using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP enable command handler
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
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            // Check if already in privileged mode
            if (IsInMode(context, "privileged"))
            {
                return Success(""); // Already in privileged mode
            }
            
            // Move to privileged mode
            SetMode(context, "privileged");
            
            return Success("Entering privileged mode...\nF5 BIG-IP System\nCopyright (c) 1996-2024 F5 Networks, Inc.\nAll rights reserved.\n");
        }
    }
} 
