using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

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

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }

            return Success("System restart initiated");
        }
    }
}
