using System.Threading.Tasks;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel copy command handler
    /// </summary>
    public class CopyCommandHandler : VendorAgnosticCliHandler
    {
        public CopyCommandHandler() : base("copy", "Copy files")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            return Success("Copy operation completed");
        }
    }
}