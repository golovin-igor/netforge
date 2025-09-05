using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel copy command handler
    /// </summary>
    public class CopyCommandHandler() : VendorAgnosticCliHandler("copy", "Copy files")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }

            return Success("Copy operation completed");
        }
    }
}
