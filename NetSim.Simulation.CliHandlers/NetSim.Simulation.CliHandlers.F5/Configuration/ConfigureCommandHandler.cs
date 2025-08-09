using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Configuration
{
    /// <summary>
    /// F5 BIG-IP configure command handler
    /// </summary>
    public class ConfigureCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigureCommandHandler() : base("configure", "Enter configuration mode")
        {
            AddAlias("configure terminal");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            // Enter configuration mode
            SetMode(context, "config");
            
            return Success("Entering configuration mode...\nF5 BIG-IP Configuration Mode\nType 'exit' to return to operational mode");
        }
    }
} 
