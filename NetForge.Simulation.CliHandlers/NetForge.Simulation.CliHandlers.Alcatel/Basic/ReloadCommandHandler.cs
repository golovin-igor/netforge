using System.Threading.Tasks;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel reload command handler
    /// </summary>
    public class ReloadCommandHandler : VendorAgnosticCliHandler
    {
        public ReloadCommandHandler() : base("reload", "Restart the system")
        {
            AddAlias("restart");
            AddAlias("reboot");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            return Success("System restart initiated");
        }
    }
}