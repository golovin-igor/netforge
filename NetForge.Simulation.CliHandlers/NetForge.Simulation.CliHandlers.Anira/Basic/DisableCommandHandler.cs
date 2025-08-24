using System.Threading.Tasks;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Anira.Basic
{
    /// <summary>
    /// Anira disable command handler
    /// </summary>
    public class DisableCommandHandler : VendorAgnosticCliHandler
    {
        public DisableCommandHandler() : base("disable", "Exit privileged mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode, "Not in privileged mode");
            }
            
            SetMode(context, "user");
            return Success("");
        }
    }
}