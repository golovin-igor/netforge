using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Generic write command handler for vendors without a specific implementation
    /// </summary>
    public class CommonWriteCommandHandler() : BaseCliHandler("write", "Save configuration")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            return Success("Configuration saved.\n");
        }
    }
}


