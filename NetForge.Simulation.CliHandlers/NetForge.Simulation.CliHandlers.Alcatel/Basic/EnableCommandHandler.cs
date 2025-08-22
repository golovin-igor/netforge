using System.Threading.Tasks;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel enable command handler
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
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            if (IsInMode(context, "privileged"))
            {
                return Success("");
            }
            
            SetMode(context, "privileged");
            return Success("");
        }
    }
}