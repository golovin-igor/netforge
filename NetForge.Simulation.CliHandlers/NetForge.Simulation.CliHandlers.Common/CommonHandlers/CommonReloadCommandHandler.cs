using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Generic reload command handler for vendors without a specific implementation
    /// </summary>
    public class CommonReloadCommandHandler() : BaseCliHandler("reload", "Restart the system")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            // Simply acknowledge the reload request
            return Success("System configuration has been modified. Save? [yes/no]: ");
        }
    }
}


