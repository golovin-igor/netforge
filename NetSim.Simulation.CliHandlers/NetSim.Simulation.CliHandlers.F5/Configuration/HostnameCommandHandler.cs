using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Configuration
{
    /// <summary>
    /// F5 BIG-IP hostname command handler
    /// </summary>
    public class HostnameCommandHandler : VendorAgnosticCliHandler
    {
        public HostnameCommandHandler() : base("hostname", "Set device hostname")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "Usage: hostname <name>");
            }
            
            var newHostname = context.CommandParts[1];
            var device = context.Device as NetworkDevice;
            if (device != null)
            {
                device.SetHostname(newHostname);
            }
            
            return Success($"Hostname set to: {newHostname}");
        }
    }
} 
